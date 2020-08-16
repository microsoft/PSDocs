using System.Management.Automation;

namespace PSDocs.Runtime
{
    /// <summary>
    /// A set of context properties that are exposed at runtime through the $PSDocs variable.
    /// </summary>
    public sealed class PSDocs
    {
        private readonly RunspaceContext _Context;

        public PSDocs() { }

        internal PSDocs(RunspaceContext context)
        {
            _Context = context;
        }

        /// <summary>
        /// The current target object.
        /// </summary>
        public PSObject TargetObject
        {
            get
            {
                return GetContext().TargetObject;
            }
        }

        private RunspaceContext GetContext()
        {
            if (_Context == null)
                return RunspaceContext.CurrentThread;

            return _Context;
        }
    }
}
