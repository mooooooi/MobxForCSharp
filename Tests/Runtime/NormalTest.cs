using Higo.Mobx;
using NUnit.Framework;

public class PersonBaseInfo : ObservableObject
{
    ObservableValue<int> m_id;
    public int id
    {
        set => m_id.Value = value;
        get => m_id.Value;
    }

    ObservableValue<string> m_name;
    public string name
    {
        set => m_name.Value = value;
        get => m_name.Value;
    }

    protected override void OnBind()
    {
        BindValue(ref m_id);
        BindValue(ref m_name);
    }
}

public class PersonInfo : ObservableObject
{
    ObservableValue<float> m_age;
    public float age
    {
        set => m_age.Value = value;
        get => m_age.Value;
    }

    PersonBaseInfo m_baseInfo;
    public PersonBaseInfo BaseInfo => m_baseInfo;

    protected override void OnBind()
    {
        BindValue(ref m_age);
        BindObject(ref m_baseInfo);
    }
}

public static class NormalTest
{
    [Test]
    public static void Single()
    {
        var baseInfo = Store.AsRoot<PersonBaseInfo>();

        var testNum = 0;

        baseInfo.AutoRun(() =>
        {
            testNum++;
            _ = baseInfo.id;
        });
        Assert.AreEqual(1, testNum);

        var testNum2 = 0;
        baseInfo.AutoRun(() =>
        {
            Assert.AreEqual(baseInfo.name, testNum2 == 0 ? null : "IAmChanging!");
            testNum2++;
        });

        using (baseInfo.CreateActionScope())
        {
            baseInfo.name = "IAmChanging!";
        }
        Assert.AreEqual(1, testNum);

        using (baseInfo.CreateActionScope())
        {
            baseInfo.id = 0;
        }
        Assert.AreEqual(2, testNum);

        using (baseInfo.CreateActionScope())
        {
            baseInfo.id++;
        }
        Assert.AreEqual(3, testNum);
    }

    [Test]
    public static void Multiple()
    {
        var store = new Store();
        var info = Store.AsRoot<PersonInfo>();

        var testNum = 0;

        info.AutoRun(() =>
        {
            testNum++;
            _ = info.BaseInfo.name;
            _ = info.age;
        });
        Assert.AreEqual(1, testNum);

        using (info.CreateActionScope())
        {
            info.BaseInfo.id++;
        }
        Assert.AreEqual(1, testNum);

        using (info.CreateActionScope())
        {
            info.BaseInfo.name = "111";
        }
        Assert.AreEqual(2, testNum);

        using (info.CreateActionScope())
        {
            info.age--;
        }
        Assert.AreEqual(3, testNum);

        using (info.CreateActionScope())
        {
            info.BaseInfo.name = "111";
            info.age--;
        }
        Assert.AreEqual(4, testNum);
    }

    [Test]
    public static void Ext()
    {
        var baseInfo = Store.AsRoot<PersonBaseInfo>();

        var testNum = 0;
        baseInfo.AutoRun(b =>
        {
            testNum++;
            _ = b.id;
        });
        Assert.AreEqual(1, testNum);

        using (baseInfo.CreateActionScope())
        {
            baseInfo.id++;
        }
        Assert.AreEqual(2, testNum);

        using (baseInfo.CreateActionScope())
        {
            baseInfo.name = "123123";
        }
        Assert.AreEqual(2, testNum);
    }
}
