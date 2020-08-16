using System.Collections.Generic;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// Handles binding of instance names to objects.
    /// </summary>
    internal sealed class InstanceNameBinder
    {
        private readonly string[] _InstanceName;

        internal InstanceNameBinder(string[] instanceName)
        {
            _InstanceName = instanceName;
        }

        internal IEnumerable<string> GetInstanceName(string defaultInstanceName)
        {
            if (_InstanceName == null || _InstanceName.Length == 0)
                yield return defaultInstanceName;

            for (var i = 0; _InstanceName != null && i < _InstanceName.Length; i++)
                yield return _InstanceName[i];
        }
    }
}
