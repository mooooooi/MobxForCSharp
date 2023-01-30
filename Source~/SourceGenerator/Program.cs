using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Collections.Generic;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Threading;
using System.Linq;
using System.Xml.Linq;

namespace SourceGenerator
{
    public class GeneratorBase
    {
        public static readonly string ObservableObjectAttriFullName
            = typeof(Higo.Mobx.Attribute.ObservableObjectAttribute).FullName;
        public static readonly string ObservableObjectAttriName
            = typeof(Higo.Mobx.Attribute.ObservableObjectAttribute).Name;
        public static readonly string ObservableFieldtAttriFullName
            = typeof(Higo.Mobx.Attribute.ObservableFieldAttribute).FullName;

        public delegate void AddSourceDel(string hintName, SourceText sourceText);
        public delegate void ReportDiagnosticDel(Diagnostic diagnostic);

        public static void Execute(
            GeneratorExecutionContext ctx,
            AddSourceDel AddSource, ReportDiagnosticDel ReportDiagnostic, IReadOnlyCollection<InterfaceDeclarationSyntax> classInfos)
        {
            try
            {

                var unit = SF.CompilationUnit()
                    .AddUsings(
                        SF.UsingDirective(SF.ParseName("System"))
                    );
                var @namespace = SF.NamespaceDeclaration(
                    SF.ParseName("Higo.Mobx.Generated"));

                @namespace = @namespace.AddMembers(classInfos.Select(x => ClassInfo.Parse(ctx, x, ctx.CancellationToken)).ToArray());

                unit = unit.AddMembers(@namespace);
                AddSource("generated.cs", SourceText.From(
                    unit.NormalizeWhitespace().ToFullString(),
                    Encoding.UTF8));
            }
            catch (Exception ex)
            {
                ReportDiagnostic(Diagnostic.Create("Source Generator", "Generate Exception", (LocalizableString)ex.ToString(), DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
            }
        }
    }

    public static class ClassInfo
    {
        public static StringBuilder strBuilder = new StringBuilder();
        public static ClassDeclarationSyntax Parse(GeneratorExecutionContext ctx, InterfaceDeclarationSyntax @interface, CancellationToken token)
        {

            var semanticModel = ctx.Compilation.GetSemanticModel(@interface.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(@interface, token);
            var attris = symbol.GetAttributes();
            var observableObjectAttri = attris.FirstOrDefault(
                attri => attri.AttributeClass.ToDisplayString() == GeneratorBase.ObservableObjectAttriFullName);
            Console.WriteLine(symbol.Name);
            if (observableObjectAttri == null) return null;
            var absClass = SF.ClassDeclaration(getTypeName(@interface.Identifier.Text))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .WithMembers(SF.List(getMembers(ctx, @interface.Members)))
                .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName(symbol.ToDisplayString())));
            //var symbol = @interface.SyntaxTree;
            return absClass;
        }

        private static string getTypeName(string typeName)
        {
            strBuilder.Clear();
            if (typeName[0] == 'I')
                strBuilder.Append(typeName.Substring(1));
            else
                strBuilder.Append(typeName);
            strBuilder[0] = char.ToUpper(strBuilder[0]);
            strBuilder.Append("Base");
            return strBuilder.ToString();
        }

        private static IEnumerable<MemberDeclarationSyntax> getMembers(GeneratorExecutionContext ctx, SyntaxList<MemberDeclarationSyntax> members)
        {
            foreach (var m in members)
            {
                var semanticModel = ctx.Compilation.GetSemanticModel(m.SyntaxTree);
                if (m is not PropertyDeclarationSyntax pm) continue;
                var tt = SF.ParseTypeName($"Higo.Mobx.ObservableValue<{pm.Type.ToFullString()}>");

                var nn = "__ober_" + pm.Identifier.Text;
                yield return SF.FieldDeclaration(
                    SF.VariableDeclaration(
                        tt, SF.SingletonSeparatedList(SF.VariableDeclarator(nn))
                        )
                    )
                    .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.ProtectedKeyword)));

                yield return SF.PropertyDeclaration(
                    pm.Type, pm.Identifier
                ).WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .AddAccessorListAccessors(
                    SF.AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        default,
                        default,
                        null,
                        SF.ArrowExpressionClause(SF.ParseExpression($"{nn}.Value"))
                        ).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)),
                    SF.AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration,
                        default,
                        default,
                        null,
                        SF.ArrowExpressionClause(SF.ParseExpression($"{nn}.Value = value"))
                        ).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken))
                );
            }
        }
    }

    public static class Ext
    {
    }

    public class PropInfo
    {

    }

    public class GeneratorVistor : ISyntaxReceiver
    {
        public List<InterfaceDeclarationSyntax> classInfos = new();

        private readonly CancellationToken cancellationToken;

        public GeneratorVistor(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (syntaxNode is InterfaceDeclarationSyntax { AttributeLists: { Count: > 0 } } @interface)
            {
                if (@interface.AttributeLists.SelectMany(x => x.Attributes).Any(attri => attri.Name + "Attribute" == GeneratorBase.ObservableObjectAttriName))
                {
                    classInfos.Add(@interface);
                }

            }
        }
    }

    [Generator]
    public class ExampleSourceGenerator : GeneratorBase, ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var visitor = context.SyntaxReceiver as GeneratorVistor;
            //context.Compilation.GetSemanticModel()
            Execute(
                context,
                context.AddSource,
                context.ReportDiagnostic,
                visitor.classInfos);

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(
                () => new GeneratorVistor(context.CancellationToken));
        }
    }
}