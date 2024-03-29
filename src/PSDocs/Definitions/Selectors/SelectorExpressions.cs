﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PSDocs.Pipeline;
using PSDocs.Resources;

namespace PSDocs.Definitions.Selectors
{
    internal delegate bool SelectorExpressionFn(SelectorContext context, SelectorInfo info, object[] args, object o);

    internal delegate bool SelectorExpressionOuterFn(SelectorContext context, object o);

    internal enum SelectorExpressionType
    {
        Operator = 1,

        Condition = 2
    }

    internal interface ISelectorExpresssionDescriptor
    {
        string Name { get; }

        SelectorExpressionType Type { get; }

        SelectorExpression CreateInstance(SourceFile source, SelectorExpression.PropertyBag properties);
    }

    internal sealed class SelectorExpresssionDescriptor : ISelectorExpresssionDescriptor
    {
        public SelectorExpresssionDescriptor(string name, SelectorExpressionType type, SelectorExpressionFn fn)
        {
            Name = name;
            Type = type;
            Fn = fn;
        }

        public string Name { get; }

        public SelectorExpressionType Type { get; }

        public SelectorExpressionFn Fn { get; }

        public SelectorExpression CreateInstance(SourceFile source, SelectorExpression.PropertyBag properties)
        {
            if (Type == SelectorExpressionType.Operator)
                return new SelectorOperator(this);

            if (Type == SelectorExpressionType.Condition)
                return new SelectorCondition(this, properties);

            return null;
        }
    }

    internal sealed class SelectorInfo
    {
        private readonly string path;

        public SelectorInfo(string path)
        {
            this.path = path;
        }
    }

    internal sealed class SelectorExpressionFactory
    {
        private readonly Dictionary<string, ISelectorExpresssionDescriptor> _Descriptors;

        public SelectorExpressionFactory()
        {
            _Descriptors = new Dictionary<string, ISelectorExpresssionDescriptor>(SelectorExpressions.Builtin.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var d in SelectorExpressions.Builtin)
                With(d);
        }

        public bool TryDescriptor(string name, out ISelectorExpresssionDescriptor descriptor)
        {
            return _Descriptors.TryGetValue(name, out descriptor);
        }

        public bool IsOperator(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == SelectorExpressionType.Operator;
        }

        public bool IsCondition(string name)
        {
            return TryDescriptor(name, out var d) && d != null && d.Type == SelectorExpressionType.Condition;
        }

        private void With(ISelectorExpresssionDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.Name, descriptor);
        }
    }

    internal sealed class SelectorExpressionBuilder
    {
        private const char Dot = '.';
        private const char OpenBracket = '[';
        private const char CloseBracket = '[';

        private readonly bool _Debugger;

        public SelectorExpressionBuilder(bool debugger = true)
        {
            _Debugger = debugger;
        }

        public SelectorExpressionOuterFn Build(SelectorIf selectorIf)
        {
            return Expression(string.Empty, selectorIf.Expression);
        }

        private SelectorExpressionOuterFn Expression(string path, SelectorExpression expression)
        {
            path = Path(path, expression);
            if (expression is SelectorOperator selectorOperator)
                return Debugger(Operator(path, selectorOperator), path);
            else if (expression is SelectorCondition selectorCondition)
                return Debugger(Condition(path, selectorCondition), path);

            throw new InvalidOperationException();
        }

        private static SelectorExpressionOuterFn Condition(string path, SelectorCondition expression)
        {
            var info = new SelectorInfo(path);
            return (context, o) => expression.Descriptor.Fn(context, info, new object[] { expression.Property }, o);
        }

        private static string Path(string path, SelectorExpression expression)
        {
            path = string.Concat(path, Dot, expression.Descriptor.Name);
            return path;
        }

        private SelectorExpressionOuterFn Operator(string path, SelectorOperator expression)
        {
            var inner = new List<SelectorExpressionOuterFn>(expression.Children.Count);
            for (var i = 0; i < expression.Children.Count; i++)
            {
                var childPath = string.Concat(path, OpenBracket, i, CloseBracket);
                inner.Add(Expression(childPath, expression.Children[i]));
            }
            var innerA = inner.ToArray();
            var info = new SelectorInfo(path);
            return (context, o) => expression.Descriptor.Fn(context, info, innerA, o);
        }

        private SelectorExpressionOuterFn Debugger(SelectorExpressionOuterFn expression, string path)
        {
            if (!_Debugger)
                return expression;

            return (context, o) => DebuggerFn(context, path, expression, o);
        }

        private static bool DebuggerFn(SelectorContext context, string path, SelectorExpressionOuterFn expression, object o)
        {
            var result = expression(context, o);
            context.Debug(PSDocsResources.SelectorTrace, path, result);
            return result;
        }
    }

    /// <summary>
    /// Expressions that can be used with selectors.
    /// </summary>
    internal sealed class SelectorExpressions
    {
        // Conditions
        private const string EXISTS = "exists";
        private const string EQUALS = "equals";
        private const string NOTEQUALS = "notEquals";
        private const string HASVALUE = "hasValue";
        private const string MATCH = "match";
        private const string NOTMATCH = "notMatch";
        private const string IN = "in";
        private const string NOTIN = "notIn";
        private const string LESS = "less";
        private const string LESSOREQUALS = "lessOrEquals";
        private const string GREATER = "greater";
        private const string GREATEROREQUALS = "greaterOrEquals";
        private const string STARTSWITH = "startsWith";
        private const string ENDSWITH = "endsWith";
        private const string CONTAINS = "contains";
        private const string ISSTRING = "isString";
        private const string ISLOWER = "isLower";
        private const string ISUPPER = "isUpper";

        // Operators
        private const string IF = "if";
        private const string ANYOF = "anyOf";
        private const string ALLOF = "allOf";
        private const string NOT = "not";
        private const string FIELD = "field";

        // Define built-ins
        internal static readonly ISelectorExpresssionDescriptor[] Builtin = new ISelectorExpresssionDescriptor[]
        {
            // Operators
            new SelectorExpresssionDescriptor(IF, SelectorExpressionType.Operator, If),
            new SelectorExpresssionDescriptor(ANYOF, SelectorExpressionType.Operator, AnyOf),
            new SelectorExpresssionDescriptor(ALLOF, SelectorExpressionType.Operator, AllOf),
            new SelectorExpresssionDescriptor(NOT, SelectorExpressionType.Operator, Not),

            // Conditions
            new SelectorExpresssionDescriptor(EXISTS, SelectorExpressionType.Condition, Exists),
            new SelectorExpresssionDescriptor(EQUALS, SelectorExpressionType.Condition, Equals),
            new SelectorExpresssionDescriptor(NOTEQUALS, SelectorExpressionType.Condition, NotEquals),
            new SelectorExpresssionDescriptor(HASVALUE, SelectorExpressionType.Condition, HasValue),
            new SelectorExpresssionDescriptor(MATCH, SelectorExpressionType.Condition, Match),
            new SelectorExpresssionDescriptor(NOTMATCH, SelectorExpressionType.Condition, NotMatch),
            new SelectorExpresssionDescriptor(IN, SelectorExpressionType.Condition, In),
            new SelectorExpresssionDescriptor(NOTIN, SelectorExpressionType.Condition, NotIn),
            new SelectorExpresssionDescriptor(LESS, SelectorExpressionType.Condition, Less),
            new SelectorExpresssionDescriptor(LESSOREQUALS, SelectorExpressionType.Condition, LessOrEquals),
            new SelectorExpresssionDescriptor(GREATER, SelectorExpressionType.Condition, Greater),
            new SelectorExpresssionDescriptor(GREATEROREQUALS, SelectorExpressionType.Condition, GreaterOrEquals),
            new SelectorExpresssionDescriptor(STARTSWITH, SelectorExpressionType.Condition, StartsWith),
            new SelectorExpresssionDescriptor(ENDSWITH, SelectorExpressionType.Condition, EndsWith),
            new SelectorExpresssionDescriptor(CONTAINS, SelectorExpressionType.Condition, Contains),
            new SelectorExpresssionDescriptor(ISSTRING, SelectorExpressionType.Condition, IsString),
            new SelectorExpresssionDescriptor(ISLOWER, SelectorExpressionType.Condition, IsLower),
            new SelectorExpresssionDescriptor(ISUPPER, SelectorExpressionType.Condition, IsUpper),
        };

        #region Operators

        internal static bool If(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            if (inner.Length > 0)
                return inner[0](context, o);

            return false;
        }

        internal static bool AnyOf(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (inner[i](context, o))
                    return true;
            }
            return false;
        }

        internal static bool AllOf(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            for (var i = 0; i < inner.Length; i++)
            {
                if (!inner[i](context, o))
                    return false;
            }
            return true;
        }

        internal static bool Not(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var inner = GetInner(args);
            if (inner.Length > 0)
                return !inner[0](context, o);

            return false;
        }

        #endregion Operators

        #region Conditions

        internal static bool Exists(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, EXISTS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, EXISTS, field, propertyValue);
                return propertyValue == ExpressionHelpers.Exists(context, o, field, caseSensitive: false);
            }
            return false;
        }

        internal static bool Equals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, EQUALS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, EQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return false;

                // int, string, bool
                return ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true);
            }
            return false;
        }

        internal static bool NotEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTEQUALS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, NOTEQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                // int, string, bool
                return !ExpressionHelpers.Equal(propertyValue, value, caseSensitive: false, convertExpected: true);
            }
            return false;
        }

        internal static bool HasValue(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, HASVALUE, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, HASVALUE, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return !propertyValue.Value;

                return !propertyValue.Value == ExpressionHelpers.NullOrEmpty(value);
            }
            return false;
        }

        internal static bool Match(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, MATCH, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, MATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return false;

                return ExpressionHelpers.Match(propertyValue, value, caseSensitive: false);
            }
            return false;
        }

        internal static bool NotMatch(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryProperty(properties, NOTMATCH, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, NOTMATCH, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                return !ExpressionHelpers.Match(propertyValue, value, caseSensitive: false);
            }
            return false;
        }

        internal static bool In(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, IN, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, IN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return false;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return true;
                }
                return false;
            }
            return false;
        }

        internal static bool NotIn(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyArray(properties, NOTIN, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, NOTIN, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.AnyValue(value, propertyValue.GetValue(i), caseSensitive: false, out _))
                        return false;
                }
                return true;
            }
            return false;
        }

        internal static bool Less(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, LESS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                if (value == null)
                    return 0 < propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out var compare, value: out _))
                    return compare < 0;
            }
            return false;
        }

        internal static bool LessOrEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, LESSOREQUALS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, LESSOREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                if (value == null)
                    return 0 <= propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out var compare, value: out _))
                    return compare <= 0;
            }
            return false;
        }

        internal static bool Greater(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATER, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, GREATER, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                if (value == null)
                    return 0 > propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out var compare, value: out _))
                    return compare > 0;
            }
            return false;
        }

        internal static bool GreaterOrEquals(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyLong(properties, GREATEROREQUALS, out var propertyValue) && TryField(properties, out var field))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, GREATEROREQUALS, field, propertyValue);
                if (!ObjectHelper.GetField(context, o, field, caseSensitive: false, out var value))
                    return true;

                if (value == null)
                    return 0 >= propertyValue;

                if (ExpressionHelpers.CompareNumeric(value, propertyValue, convert: false, compare: out var compare, value: out _))
                    return compare >= 0;
            }
            return false;
        }

        internal static bool StartsWith(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, STARTSWITH, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, STARTSWITH, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out var value))
                    return false;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.StartsWith(value, propertyValue[i], caseSensitive: false))
                        return true;
                }
                return false;
            }
            return false;
        }

        internal static bool EndsWith(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, ENDSWITH, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, ENDSWITH, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out var value))
                    return false;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.EndsWith(value, propertyValue[i], caseSensitive: false))
                        return true;
                }
                return false;
            }
            return false;
        }

        internal static bool Contains(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyStringArray(properties, CONTAINS, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, CONTAINS, operand, propertyValue);
                if (!ExpressionHelpers.TryString(operand, out var value))
                    return false;

                for (var i = 0; propertyValue != null && i < propertyValue.Length; i++)
                {
                    if (ExpressionHelpers.Contains(value, propertyValue[i], caseSensitive: false))
                        return true;
                }
                return false;
            }
            return false;
        }

        internal static bool IsString(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISSTRING, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                context.Debug(PSDocsResources.SelectorExpressionTrace, ISSTRING, operand, propertyValue);
                return propertyValue == ExpressionHelpers.TryString(operand, out _);
            }
            return false;
        }

        internal static bool IsLower(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISLOWER, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                if (!ExpressionHelpers.TryString(operand, out var value))
                    return !propertyValue.Value;

                context.Debug(PSDocsResources.SelectorExpressionTrace, ISLOWER, operand, propertyValue);
                return propertyValue == ExpressionHelpers.IsLower(value, requireLetters: false, notLetter: out _);
            }
            return false;
        }

        internal static bool IsUpper(SelectorContext context, SelectorInfo info, object[] args, object o)
        {
            var properties = GetProperties(args);
            if (TryPropertyBool(properties, ISUPPER, out var propertyValue) && TryOperand(context, o, properties, out var operand))
            {
                if (!ExpressionHelpers.TryString(operand, out var value))
                    return !propertyValue.Value;

                context.Debug(PSDocsResources.SelectorExpressionTrace, ISUPPER, operand, propertyValue);
                return propertyValue == ExpressionHelpers.IsUpper(value, requireLetters: false, notLetter: out _);
            }
            return false;
        }

        #endregion Conditions

        #region Helper methods

        private static bool TryProperty(SelectorExpression.PropertyBag properties, string propertyName, out object propertyValue)
        {
            return properties.TryGetValue(propertyName, out propertyValue);
        }

        private static bool TryPropertyBool(SelectorExpression.PropertyBag properties, string propertyName, out bool? propertyValue)
        {
            return properties.TryGetBool(propertyName, out propertyValue);
        }

        private static bool TryPropertyLong(SelectorExpression.PropertyBag properties, string propertyName, out long? propertyValue)
        {
            return properties.TryGetLong(propertyName, out propertyValue);
        }

        private static bool TryField(SelectorExpression.PropertyBag properties, out string field)
        {
            return properties.TryGetString(FIELD, out field);
        }

        private static bool TryOperand(SelectorContext context, object o, SelectorExpression.PropertyBag properties, out object operand)
        {
            operand = null;
            if (properties.TryGetString(FIELD, out var field))
                return ObjectHelper.GetField(context, o, field, caseSensitive: false, out operand);

            return false;
        }

        private static bool TryPropertyArray(SelectorExpression.PropertyBag properties, string propertyName, out Array propertyValue)
        {
            if (properties.TryGetValue(propertyName, out var array) && array is Array arrayValue)
            {
                propertyValue = arrayValue;
                return true;
            }
            propertyValue = null;
            return false;
        }

        private static bool TryPropertyStringArray(SelectorExpression.PropertyBag properties, string propertyName, out string[] propertyValue)
        {
            if (properties.TryGetStringArray(propertyName, out propertyValue))
            {
                return true;
            }
            else if (properties.TryGetString(propertyName, out var s))
            {
                propertyValue = new string[] { s };
                return true;
            }
            propertyValue = null;
            return false;
        }

        private static SelectorExpression.PropertyBag GetProperties(object[] args)
        {
            return (SelectorExpression.PropertyBag)args[0];
        }

        private static SelectorExpressionOuterFn[] GetInner(object[] args)
        {
            return (SelectorExpressionOuterFn[])args;
        }

        #endregion Helper methods
    }
}
