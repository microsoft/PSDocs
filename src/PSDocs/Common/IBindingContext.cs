// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs
{
    internal interface IBindingContext
    {
        bool GetNameToken(string expression, out NameToken nameToken);

        void CacheNameToken(string expression, NameToken nameToken);
    }
}
