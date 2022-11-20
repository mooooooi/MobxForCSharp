using Higo.Mobx;
using System.Diagnostics;
using UnityEngine;
using NTest;

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

public class Program
{
    static PersonBaseInfo baseInfo;
    static PersonAgeInfo ageInfo;
    public static void Main()
    {
        var store = new Store();
        baseInfo = new PersonBaseInfo(store);
        ageInfo = new PersonAgeInfo(store);

        var num1 = 300;
        while (num1-- > 0)
        {
            store.AutoRun(() =>
            {
                _ = baseInfo.id;
                _ = ageInfo.age;
            });
            store.AutoRun(() =>
            {
                _ = baseInfo.id;
                _ = baseInfo.name;
            });
        }

        var watch = new Stopwatch();
        watch.Start();

        var num = 10000;
        while (num-- > 0)
        {
            using (store.CreateActionScope())
            {
                baseInfo.id = 123 + num;
                baseInfo.name = "asdasd";
            }

            using (store.CreateActionScope())
            {
                ageInfo.age++;
            }
        }

        UnityEngine.Debug.Log($"总耗时：{watch.Elapsed.TotalMilliseconds}");
    }
}
