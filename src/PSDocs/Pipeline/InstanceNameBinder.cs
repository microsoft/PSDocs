namespace PSDocs.Pipeline
{
    /// <summary>
    /// Handles binding of instance names to objects.
    /// </summary>
    internal sealed class InstanceNameBinder
    {
        private readonly string[] _InstanceName;
        private int _Counter;

        internal InstanceNameBinder(string[] instanceName)
        {
            _InstanceName = instanceName;
        }

        internal string GetInstanceName()
        {
            if (_InstanceName == null || _Counter >= _InstanceName.Length)
                return null;

            return _InstanceName[_Counter++];
        }
    }
}
