// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Definitions.Selectors;

namespace PSDocs.Definitions
{
    public abstract class Spec
    {
        private const string FullNameSeparator = "/";

        public static string GetFullName(string apiVersion, string name)
        {
            return string.Concat(apiVersion, FullNameSeparator, name);
        }
    }

    internal static class Specs
    {
        internal const string V1 = "github.com/microsoft/PSDocs/v1";
        internal const string Selector = "Selector";

        public readonly static ISpecDescriptor[] BuiltinTypes = new ISpecDescriptor[]
        {
            new SpecDescriptor<SelectorV1, SelectorV1Spec>(V1, Selector),
        };
    }
}
