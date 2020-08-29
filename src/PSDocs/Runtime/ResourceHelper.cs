namespace PSDocs.Runtime
{
    internal static class ResourceHelper
    {
        private const string LooseModuleName = ".";
        private const char ModuleSeparator = '\\';

        internal static string GetId(string moduleName, string name)
        {
            return string.Concat(string.IsNullOrEmpty(moduleName) ? LooseModuleName : moduleName, ModuleSeparator, name);
        }
    }
}
