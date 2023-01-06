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
        public static readonly string ObservableFieldtAttriFullName
            = typeof(Higo.Mobx.Attribute.ObservableFieldAttribute).FullName;

        public delegate void AddSourceDel(string hintName, SourceText sourceText);
        public delegate void ReportDiagnosticDel(Diagnostic diagnostic);

        public static void Execute(
            AddSourceDel AddSource, ReportDiagnosticDel ReportDiagnostic, IReadOnlyCollection<ClassDeclarationSyntax> classInfos)
        {
            var unit = SF.CompilationUnit()
                .AddUsings(
                    SF.UsingDirective(SF.ParseName("System"))
                );
            var @namespace = SF.NamespaceDeclaration(
                SF.ParseName("Higo.Mobx.Generated"));

            @namespace  = @namespace.AddMembers(classInfos.ToArray());

            unit = unit.AddMembers(@namespace);
            AddSource("generated.cs", SourceText.From(
                unit.NormalizeWhitespace().ToFullString(),
                Encoding.UTF8));
        }
    }

    public class ClassInfo
    {
        public static StringBuilder strBuilder = new StringBuilder();
        public static ClassDeclarationSyntax Parse(GeneratorSyntaxContext ctx, InterfaceDeclarationSyntax @interface, CancellationToken token)
        {
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(@interface, token);
            var attris = symbol.GetAttributes();
            var observableObjectAttri = attris.FirstOrDefault(
                attri => attri.AttributeClass.ToDisplayString() == GeneratorBase.ObservableObjectAttriFullName);
            Console.WriteLine(symbol.Name);
            if (observableObjectAttri == null) return null;
            var absClass = SF.ClassDeclaration(getTypeName(@interface.Identifier.Text))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.AbstractKeyword)))
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
    }

    public class PropInfo
    {

    }

    public class GeneratorVistor : ISyntaxContextReceiver
    {
        public List<ClassDeclarationSyntax> classInfos = new();

        private CancellationToken cancellationToken;

        public GeneratorVistor(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext ctx)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ctx.Node is InterfaceDeclarationSyntax { AttributeLists: { Count: > 0 } } @interface)
            {
                var info = ClassInfo.Parse(ctx, @interface, cancellationToken);
                if (info is not null)
                {
                    classInfos.Add(info);
                }

            }
        }
    }

    [Generator]
    public class ExampleSourceGenerator : GeneratorBase, ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var visitor = context.SyntaxContextReceiver as GeneratorVistor;
            Execute(
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