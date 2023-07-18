// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSDocs.Data;
using PSDocs.Definitions.Selectors;

namespace PSDocs.Pipeline
{
    internal abstract class TargetObjectAnnotation
    {

    }

    internal sealed class SelectorTargetAnnotation : TargetObjectAnnotation
    {
        private readonly Dictionary<Guid, bool> _Results;

        public SelectorTargetAnnotation()
        {
            _Results = new Dictionary<Guid, bool>();
        }

        public bool TryGetSelectorResult(SelectorVisitor selector, out bool result)
        {
            return _Results.TryGetValue(selector.InstanceId, out result);
        }

        public void SetSelectorResult(SelectorVisitor selector, bool result)
        {
            _Results[selector.InstanceId] = result;
        }
    }

    internal sealed class TargetObject
    {
        private readonly Dictionary<Type, TargetObjectAnnotation> _Annotations;

        internal TargetObject(PSObject o)
        {
            Value = o;
            _Annotations = new Dictionary<Type, TargetObjectAnnotation>();
        }

        public PSObject Value { get; }

        public InputFileInfo Source { get; private set; }

        public T GetAnnotation<T>() where T : TargetObjectAnnotation, new()
        {
            if (!_Annotations.TryGetValue(typeof(T), out var value))
            {
                value = new T();
                _Annotations.Add(typeof(T), value);
            }
            return (T)value;
        }

        internal void SetSourceInfo(InputFileInfo sourceInfo)
        {
            Source = sourceInfo;
        }
    }
}
