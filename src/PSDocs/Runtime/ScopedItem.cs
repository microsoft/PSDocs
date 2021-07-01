// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Pipeline;

namespace PSDocs.Runtime
{
    public abstract class ScopedItem
    {
        private readonly RunspaceContext _Context;

        internal ScopedItem()
        {

        }

        internal ScopedItem(RunspaceContext context)
        {
            _Context = context;
        }

        #region Helper methods

        internal void RequireScope(RunspaceScope scope)
        {
            if (GetContext().IsScope(scope))
                return;

            throw new RuntimeException();
        }

        internal RunspaceContext GetContext()
        {
            if (_Context == null)
                return RunspaceContext.CurrentThread;

            return _Context;
        }

        #endregion Helper methods
    }
}
