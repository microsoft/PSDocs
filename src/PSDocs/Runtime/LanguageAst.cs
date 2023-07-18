// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using PSDocs.Definitions;
using PSDocs.Pipeline;
using PSDocs.Resources;

namespace PSDocs.Runtime
{
    internal sealed class LanguageAst : AstVisitor
    {
        private const string PARAMETER_NAME = "Name";
        private const string PARAMETER_BODY = "Body";
        private const string PARAMETER_ERRORACTION = "ErrorAction";
        private const string PARAMETER_WITH = "With";
        private const string DOCUMENT_KEYWORD = "Document";
        private const string ERRORID_PARAMETERNOTFOUND = "PSDocs.Parse.DefinitionParameterNotFound";
        private const string ERRORID_INVALIDDOCUMENTNESTING = "PSDocs.Parse.InvalidDocumentNesting";
        private const string ERRORID_INVALIDERRORACTION = "PSDocs.Parse.InvalidErrorAction";
        private const string ERRORID_SELECTORNOTFOUND = "PSDocs.Parse.SelectorNotFound";

        private readonly PipelineContext _Context;
        private readonly StringComparer _Comparer;

        internal List<ErrorRecord> Errors;

        internal LanguageAst(Pipeline.PipelineContext context)
        {
            _Context = context;
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }

        private sealed class ParameterBindResult
        {
            public ParameterBindResult()
            {
                Bound = new Dictionary<string, CommandElementAst>(StringComparer.OrdinalIgnoreCase);
                Unbound = new List<CommandElementAst>();
                _Offset = 0;
            }

            public Dictionary<string, CommandElementAst> Bound;
            public List<CommandElementAst> Unbound;

            private int _Offset;

            public bool Has<TAst>(string parameterName, out TAst parameterValue) where TAst : CommandElementAst
            {
                var result = Bound.TryGetValue(parameterName, out var value) && value is TAst;
                parameterValue = result ? value as TAst : null;
                return result;
            }

            public bool Has<TAst>(string parameterName, int position, out TAst value) where TAst : CommandElementAst
            {
                // Try bound
                if (Has<TAst>(parameterName, out value))
                {
                    _Offset++;
                    return true;
                }
                var relative = position - _Offset;
                var result = Unbound.Count > relative && Unbound[relative] is TAst;
                value = result ? Unbound[relative] as TAst : null;
                return result;
            }
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (IsDefinition(commandAst))
            {
                var valid = NotNested(commandAst) &&
                    HasValidErrorAction(commandAst) &&
                    HasRequiredParameters(commandAst) &&
                    HasValidSelector(commandAst);

                return valid ? base.VisitCommand(commandAst) : AstVisitAction.SkipChildren;
            }
            return base.VisitCommand(commandAst);
        }

        /// <summary>
        /// Determines if the definition has a Body parameter.
        /// </summary>
        private bool HasBodyParameter(CommandAst commandAst, ParameterBindResult bindResult)
        {
            if (bindResult.Has(PARAMETER_BODY, 1, out ScriptBlockExpressionAst _))
                return true;

            ReportError(ERRORID_PARAMETERNOTFOUND, PSDocsResources.DefinitionParameterNotFound, PARAMETER_BODY, ReportExtent(commandAst.Extent));
            return false;
        }

        /// <summary>
        /// Determines if the definition has a Name parameter.
        /// </summary>
        private bool HasNameParameter(CommandAst commandAst, ParameterBindResult bindResult)
        {
            if (bindResult.Has(PARAMETER_NAME, 0, out StringConstantExpressionAst value) && !string.IsNullOrEmpty(value.Value))
                return true;

            ReportError(ERRORID_PARAMETERNOTFOUND, PSDocsResources.DefinitionParameterNotFound, PARAMETER_NAME, ReportExtent(commandAst.Extent));
            return false;
        }

        /// <summary>
        /// Determines if the definition is nested in another definition.
        /// </summary>
        private bool NotNested(CommandAst commandAst)
        {
            if (GetParentBlock(commandAst)?.Parent == null)
                return true;

            ReportError(ERRORID_INVALIDDOCUMENTNESTING, PSDocsResources.InvalidDocumentNesting, ReportExtent(commandAst.Extent));
            return false;
        }

        /// <summary>
        /// Determines if the definition has required parameters.
        /// </summary>
        private bool HasRequiredParameters(CommandAst commandAst)
        {
            var bindResult = BindParameters(commandAst);
            return HasNameParameter(commandAst, bindResult) && HasBodyParameter(commandAst, bindResult);
        }

        /// <summary>
        /// Determine if the definition has allowed ErrorAction options.
        /// </summary>
        private bool HasValidErrorAction(CommandAst commandAst)
        {
            var bindResult = BindParameters(commandAst);
            if (!bindResult.Has(PARAMETER_ERRORACTION, 0, out StringConstantExpressionAst value))
                return true;

            if (!Enum.TryParse(value.Value, out ActionPreference result) || (result == ActionPreference.Ignore || result == ActionPreference.Stop))
                return true;

            ReportError(ERRORID_INVALIDERRORACTION, PSDocsResources.InvalidErrorAction, value.Value, ReportExtent(commandAst.Extent));
            return false;
        }

        private bool HasValidSelector(CommandAst commandAst)
        {
            var bindResult = BindParameters(commandAst);
            if (!bindResult.Has(PARAMETER_WITH, out StringConstantExpressionAst value))
                return true;

            var selectorId = ResourceHelper.GetId(RunspaceContext.CurrentThread.Source.File.ModuleName, value.Value);
            if (_Context.Selector.ContainsKey(selectorId))
                return true;

            ReportError(ERRORID_SELECTORNOTFOUND, PSDocsResources.SelectorNotFound, value.Value, ReportExtent(commandAst.Extent));
            return false;
        }

        /// <summary>
        /// Determines if the command is a document definition.
        /// </summary>
        private bool IsDefinition(CommandAst commandAst)
        {
            return _Comparer.Equals(commandAst.GetCommandName(), DOCUMENT_KEYWORD);
        }

        private static ParameterBindResult BindParameters(CommandAst commandAst)
        {
            var result = new ParameterBindResult();
            var i = 1;
            var next = 2;
            for (; i < commandAst.CommandElements.Count; i++, next++)
            {
                // Is named parameter
                if (commandAst.CommandElements[i] is CommandParameterAst parameter && next < commandAst.CommandElements.Count)
                {
                    result.Bound.Add(parameter.ParameterName, commandAst.CommandElements[next]);
                    i++;
                    next++;
                }
                else
                {
                    result.Unbound.Add(commandAst.CommandElements[i]);
                }
            }
            return result;
        }

        private void ReportError(string errorId, string message, params object[] args)
        {
            ReportError(new global::PSDocs.Pipeline.ParseException(
                message: string.Format(Thread.CurrentThread.CurrentCulture, message, args),
                errorId: errorId
            ));
        }

        private void ReportError(global::PSDocs.Pipeline.ParseException exception)
        {
            if (Errors == null)
            {
                Errors = new List<ErrorRecord>();
            }

            Errors.Add(new ErrorRecord(
                exception: exception,
                errorId: exception.ErrorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            ));
        }

        private static string ReportExtent(IScriptExtent extent)
        {
            return string.Concat(extent.File, " line ", extent.StartLineNumber);
        }

        private static ScriptBlockAst GetParentBlock(Ast ast)
        {
            var block = ast;
            while (block != null && block is not ScriptBlockAst)
                block = block.Parent;

            return (ScriptBlockAst)block;
        }
    }
}
