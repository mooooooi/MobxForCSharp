# MobxForCSharp
Simple, scalable state management for CSharp or Unity

## Install and Update via UPM

1. Open UPM: "Window/Package Manager"
2. Click "+" button at the top left
3. Select "Add package from git URL" and paste following URL:
https://github.com/mooooooi/MobxForCSharp.git

## Example

**Prerequisite**
```cs
// 1. import namespace.
using higo.mobx;

// 2. Define observable object.
public class Data : ObservableObject
{
    // 3. Define observable value.
    private ObservableValue<int> m_id;
    // 4. (Option)Make proxy for observable value.
    public int Id { get => m_id.Value; set => m_id.Value = value; }
    
    private ObservableValue<string> m_name;
    public string Name { get => m_name.Value; set => m_name.Value = value; }
    
    // 5. Implement OnBind.
    public override void OnBind()
    {
        BindValue(m_id);
        BindValue(m_name);
    }
}
```

**Use**
```cs
var data = Store.AsRoot<Data>();

data.AutoRun(() => 
{
    Console.WriteLine($"Data's reaction: Id({data.Id})");
});
// Console: Data's reaction: Id(0)

using(data.CreateActionScope())
{
    data.Id = 2;
}
// Console: Data's reaction: Id(2)

using(data.CreateActionScope())
{
    data.Name = "Changing!";
}
// Console no printing!

```
