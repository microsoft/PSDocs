// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace PSDocs
{
    /// <summary>
    /// A YAML converter for deserializing a string array.
    /// </summary>
    internal sealed class StringArrayTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(string[]) || type == typeof(IEnumerable<string>);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var result = new List<string>();
            var isSequence = parser.TryConsume<SequenceStart>(out _);
            while ((isSequence || result.Count == 0) && parser.TryConsume(out Scalar scalar))
                result.Add(scalar.Value);

            if (isSequence)
            {
                parser.Require<SequenceEnd>();
                parser.MoveNext();
            }
            return result.ToArray();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class PSObjectTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _Parent;
        private readonly ITypeResolver _TypeResolver;
        private readonly INamingConvention _NamingConvention;

        public PSObjectTypeInspector(ITypeInspector typeInspector)
        {
            _Parent = typeInspector;
            _TypeResolver = new StaticTypeResolver();
            _NamingConvention = CamelCaseNamingConvention.Instance;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            if (container is PSObject pso)
                return GetPropertyDescriptor(pso);

            return _Parent.GetProperties(type, container);
        }

        private IEnumerable<IPropertyDescriptor> GetPropertyDescriptor(PSObject pso)
        {
            foreach (var prop in pso.Properties)
            {
                if (prop.IsGettable && prop.IsInstance)
                    yield return new Property(prop, _TypeResolver, _NamingConvention);
            }
        }

        private sealed class Property : IPropertyDescriptor
        {
            private readonly PSPropertyInfo _PropertyInfo;
            private readonly Type _PropertyType;
            private readonly ITypeResolver _TypeResolver;
            private readonly INamingConvention _NamingConvention;

            public Property(PSPropertyInfo propertyInfo, ITypeResolver typeResolver, INamingConvention namingConvention)
            {
                _PropertyInfo = propertyInfo;
                _PropertyType = propertyInfo.Value.GetType();
                _TypeResolver = typeResolver;
                _NamingConvention = namingConvention;
                ScalarStyle = ScalarStyle.Any;
            }

            string IPropertyDescriptor.Name => _NamingConvention.Apply(_PropertyInfo.Name);

            public Type Type => _PropertyType;

            public Type TypeOverride { get; set; }

            int IPropertyDescriptor.Order { get; set; }

            bool IPropertyDescriptor.CanWrite => false;

            public ScalarStyle ScalarStyle { get; set; }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return default;
            }

            void IPropertyDescriptor.Write(object target, object value)
            {
                throw new NotImplementedException();
            }

            IObjectDescriptor IPropertyDescriptor.Read(object target)
            {
                var propertyValue = _PropertyInfo.Value;
                var actualType = TypeOverride ?? _TypeResolver.Resolve(Type, propertyValue);
                return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
            }
        }
    }
}
