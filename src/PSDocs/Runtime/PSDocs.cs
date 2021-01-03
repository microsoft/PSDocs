using System.Management.Automation;
using System.Threading;

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

        /// <summary>
        /// The current culture.
        /// </summary>
        public string Culture
        {
            get
            {
                return GetContext().Culture;
            }
        }


        //public string Generator
        //{
        //    get
        //    {
        //        return GetContext().Generator;
        //    }
        //}

        /// <summary>
        /// Format a string with arguments.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Exposed as instance method for PowerShell.")]
        public string Format(string value, params object[] args)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (args == null || args.Length == 0)
                return value;
            else
                return string.Format(Thread.CurrentThread.CurrentCulture, value, args);
        }

        private RunspaceContext GetContext()
        {
            if (_Context == null)
                return RunspaceContext.CurrentThread;

            return _Context;
        }
    }
}
