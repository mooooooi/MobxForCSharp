using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Higo.Mobx.Attribute
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ObservableObjectAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ObservableFieldAttribute : System.Attribute
    {

    }
}
