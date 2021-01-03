
using System.Dynamic;

namespace PSDocs.Runtime
{
    public sealed class LocalizedData : DynamicObject
    {
        private readonly RunspaceContext _Context;

        public LocalizedData() { }

        internal LocalizedData(RunspaceContext context)
        {
            _Context = context;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var hashtable = GetContext().GetLocalizedStrings();
            if (hashtable.Count > 0 && binder != null && !string.IsNullOrEmpty(binder.Name) && hashtable.ContainsKey(binder.Name))
            {
                result = hashtable[binder.Name];
                return true;
            }
            result = null;
            return false;
        }

        private RunspaceContext GetContext()
        {
            if (_Context == null)
                return RunspaceContext.CurrentThread;

            return _Context;
        }
    }
}
