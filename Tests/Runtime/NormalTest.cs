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

    public PersonBaseInfo(Store store)
    {
        store.Bind(this);
        Bind(ref m_id);
        Bind(ref m_name);
    }
}

public class PersonAgeInfo : ObservableObject
{
    ObservableValue<float> m_age;
    public float age
    {
        set => m_age.Value = value;
        get => m_age.Value;
    }

    public PersonAgeInfo(Store store)
    {
        store.Bind(this);
        Bind(ref m_age);
    }
}

public static class NormalTest
{
    [Test]
    public static void Single()
    {
        var store = new Store();
        var baseInfo = new PersonBaseInfo(store);

        var testNum = 0;

        store.AutoRun(() =>
        {
            testNum++;
            _ = baseInfo.id;
        });
        Assert.AreEqual(1, testNum);

        var testNum2 = 0;
        store.AutoRun(() =>
        {
            Assert.AreEqual(baseInfo.name, testNum2 == 0 ? null : "IAmChanging!");
            testNum2++;
        });

        using (store.CreateActionScope())
        {
            baseInfo.name = "IAmChanging!";
        }
        Assert.AreEqual(1, testNum);

        using (store.CreateActionScope())
        {
            baseInfo.id = 0;
        }
        Assert.AreEqual(2, testNum);

        using (store.CreateActionScope())
        {
            baseInfo.id++;
        }
        Assert.AreEqual(3, testNum);
    }

    [Test]
    public static void Multiple()
    {
        var store = new Store();
        var baseInfo = new PersonBaseInfo(store);
        var ageInfo = new PersonAgeInfo(store);

        var testNum = 0;

        store.AutoRun(() =>
        {
            testNum++;
            _ = baseInfo.name;
            _ = ageInfo.age;
        });
        Assert.AreEqual(1, testNum);

        using (store.CreateActionScope())
        {
            baseInfo.id++;
        }
        Assert.AreEqual(1, testNum);

        using (store.CreateActionScope())
        {
            baseInfo.name = "111";
        }
        Assert.AreEqual(2, testNum);

        using (store.CreateActionScope())
        {
            ageInfo.age--;
        }
        Assert.AreEqual(3, testNum);

        using (store.CreateActionScope())
        {
            baseInfo.name = "111";
            ageInfo.age--;
        }
        Assert.AreEqual(4, testNum);
    }
}
