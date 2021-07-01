// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace PSDocs.Runtime
{
    internal sealed class Host : PSHost
    {
        public override CultureInfo CurrentCulture => throw new NotImplementedException();

        public override CultureInfo CurrentUICulture => throw new NotImplementedException();

        public override Guid InstanceId => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override PSHostUserInterface UI => throw new NotImplementedException();

        public override Version Version => throw new NotImplementedException();

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
        }
    }
}
