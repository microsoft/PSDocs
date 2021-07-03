// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSDocs
{
    internal static class PSObjectExtensions
    {
        public static bool HasProperty(this PSObject o, string propertyName)
        {
            return o.Properties[propertyName] != null;
        }

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
