
using System.Management.Automation;

namespace PSDocs
{
    internal static class PSObjectExtensions
    {
        /// <summary>
        /// Determines if the PSObject has any note properties.
        /// </summary>
        public static bool HasNoteProperty(this PSObject o)
        {
            foreach (var property in o.Properties)
            {
                if (property.MemberType == PSMemberTypes.NoteProperty)
                    return true;
            }
            return false;
        }
    }
}
