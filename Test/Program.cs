// See https://aka.ms/new-console-template for more information
using Higo.Mobx;
using Higo.Mobx2;

public class State : Observable<State>
{
    ProxyValue<int> m_id;
    public int id
    {
        set => m_id.Value = value;
        get => m_id.Value;
    }

    ProxyValue<string> m_name;
    public string name
    {
        set => m_name.Value = value;
        get => m_name.Value;
    }

    public State()
    {
        m_id = CreateValue(0);
        m_name = CreateValue(string.Empty);
    }
}

public class Data1 : ObservableObject
{
    public Data0 data0 = new Data0();

    protected override void OnBind()
    {
        Bind(ref data0);
    }
}

public class Data0 : ObservableObject
{
    ObservableValue<int> m_id;
    public int Id
    {
        set => m_id.Value = value;
        get => m_id.Value;
    }

    ObservableValue<string> m_name;
    public string Name
    {
        set => m_name.Value = value;
        get => m_name.Value;
    }

    protected override void OnBind()
    {
        Bind(ref m_id);
        Bind(ref m_name);
    }
}

public class Program
{
    public static void Main()
    {
        var state = new State();

        state.AutoRun(onReaction);

        using (state.CreateActionScope())
        {
            state.name = "asdasd";
            state.id = 123;
        }

        ObservableValue<int> a;
    }

    private static void onReaction(State observable)
    {
        Console.WriteLine(observable.id);
    }
}
