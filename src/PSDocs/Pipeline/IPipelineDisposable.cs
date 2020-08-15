using System;

namespace PSDocs.Pipeline
{
    /// <summary>
    /// Objects that follow the pipeline lifecycle.
    /// </summary>
    public interface IPipelineDisposable : IDisposable
    {
        void Begin();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches PowerShell pipeline.")]
        void End();
    }
}
