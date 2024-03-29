﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.IO;
using System.Management.Automation;
using PSDocs.Models;
using PSDocs.Resources;
using PSDocs.Runtime;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Add, LanguageKeywords.Include)]
    [OutputType(typeof(Include))]
    internal sealed class IncludeCommand : KeywordCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string FileName { get; set; }

        [Parameter(Mandatory = false)]
        public string BaseDirectory { get; set; }

        [Parameter(Mandatory = false)]
        public string Culture { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter UseCulture { get; set; }

        [Parameter(Mandatory = false)]
        public IDictionary Replace { get; set; }

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(Culture))
                Culture = RunspaceContext.CurrentThread.Culture;
        }

        protected override void EndProcessing()
        {
            var result = ModelHelper.Include(BaseDirectory, Culture, FileName, UseCulture, Replace);
            if (result == null || !result.Exists)
            {
                WriteError(new ErrorRecord(
                    exception: new FileNotFoundException(PSDocsResources.IncludeNotFound, result?.Path),
                    errorId: "PSDocs.Runtime.IncludeNotFound",
                    errorCategory: ErrorCategory.ObjectNotFound,
                    targetObject: result?.Path
                ));
                return;
            }
            WriteObject(result);
        }
    }
}
