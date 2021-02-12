
using PSDocs.Models;
using System.Collections;
using System.Management.Automation;
using System.Threading;

namespace PSDocs.Runtime
{
    /// <summary>
    /// A set of context properties that are exposed at runtime through the $PSDocs variable.
    /// </summary>
    public sealed class PSDocs : ScopedItem
    {
        private Configuration _Configuration;
        private PSDocsDocument _Document;

        public PSDocs() { }

        internal PSDocs(RunspaceContext context)
            : base(context) { }

        public sealed class PSDocsDocument : ScopedItem
        {
            internal PSDocsDocument(RunspaceContext context)
                : base(context) { }

            public string InstanceName
            {
                get
                {
                    RequireScope(RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess | RunspaceScope.Condition | RunspaceScope.Document);
                    return GetContext().DocumentContext.InstanceName;
                }
                set
                {
                    RequireScope(RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess);
                    if (string.IsNullOrEmpty(value))
                        return;

                    GetContext().DocumentContext.InstanceName = value;
                }
            }

            public string OutputPath
            {
                get
                {
                    RequireScope(RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess | RunspaceScope.Condition | RunspaceScope.Document);
                    return GetContext().DocumentContext.OutputPath;
                }
                set
                {
                    RequireScope(RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess);
                    if (string.IsNullOrEmpty(value))
                        return;

                    GetContext().DocumentContext.OutputPath = value;
                }
            }

            /// <summary>
            /// Custom data for this document.
            /// </summary>
            public Hashtable Data
            {
                get
                {
                    RequireScope(RunspaceScope.Document | RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess);
                    return GetContext().DocumentContext.Data;
                }
            }
        }

        /// <summary>
        /// The current target object.
        /// </summary>
        public PSObject TargetObject
        {
            get
            {
                RequireScope(RunspaceScope.Document | RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess | RunspaceScope.Condition);
                return GetContext().TargetObject;
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
        /// Custom configuration values.
        /// </summary>
        public Configuration Configuration
        {
            get { return GetConfiguration(); }
        }

        /// <summary>
        /// The current culture.
        /// </summary>
        public string Culture
        {
            get
            {
                RequireScope(RunspaceScope.Document);
                return GetContext().DocumentContext.Culture;
            }
        }

        public PSDocsDocument Document
        {
            get { return GetDocument(); }
        }

        public IEnumerable Output
        {
            get
            {
                RequireScope(RunspaceScope.ConventionEnd);
                return GetContext().Output;
            }
        }

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

        #region Helper methods

        private Configuration GetConfiguration()
        {
            RequireScope(RunspaceScope.All);
            if (_Configuration == null)
                _Configuration = new Configuration(GetContext());

            return _Configuration;
        }

        private PSDocsDocument GetDocument()
        {
            RequireScope(RunspaceScope.Runtime);
            if (_Document == null)
                _Document = new PSDocsDocument(GetContext());

            return _Document;
        }

        #endregion Helper methods
    }
}
