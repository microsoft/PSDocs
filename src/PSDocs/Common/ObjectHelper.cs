
using System.Management.Automation;

namespace PSDocs
{
    internal static class ObjectHelper
    {
        public static object GetBaseObject(object o)
        {
            return o is PSObject pso && pso.BaseObject != null ? pso.BaseObject : o;
        }
    }
}
