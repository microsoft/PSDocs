using System;
using System.Collections.Generic;

namespace PSDocs.Models
{
    public static class PSDocsHelper
    {
        /// <summary>
        /// Check if the defined tags match the expected tags.
        /// </summary>
        /// <param name="definitionTags">The tags of the document definition.</param>
        /// <param name="match">The tags to match.</param>
        /// <returns>Returns true when all the matching tags are found on the document definition.</returns>
        public static bool MatchTags(string[] definitionTags, string[] match)
        {
            if (match == null || match.Length == 0)
            {
                return true;
            }

            if (definitionTags == null || definitionTags.Length < match.Length)
            {
                return false;
            }

            var tags = new HashSet<string>(definitionTags, StringComparer.InvariantCultureIgnoreCase);

            foreach (var m in match)
            {
                if (!tags.Contains(m))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
