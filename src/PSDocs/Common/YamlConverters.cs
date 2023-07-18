// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSDocs.Annotations;
using PSDocs.Configuration;
using PSDocs.Definitions;
using PSDocs.Definitions.Selectors;
using PSDocs.Runtime;
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

    /// <summary>
    /// A YAML converter to deserialize a map/ object as a PSObject.
    /// </summary>
    internal sealed class PSObjectYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(PSObject);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            // Handle empty objects
            if (parser.TryConsume<Scalar>(out _))
            {
                parser.TryConsume<Scalar>(out _);
                return null;
            }

            var result = new PSObject();
            if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out Scalar scalar))
                {
                    var name = scalar.Value;
                    var property = ReadNoteProperty(parser, name);
                    if (property == null)
                        throw new NotImplementedException();

                    result.Properties.Add(property);
                }
                parser.Require<MappingEnd>();
                parser.MoveNext();
            }
            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }

        private PSNoteProperty ReadNoteProperty(IParser parser, string name)
        {
            if (parser.TryConsume<SequenceStart>(out _))
            {
                var values = new List<PSObject>();
                while (parser.Current is not SequenceEnd)
                {
                    if (parser.Current is MappingStart)
                    {
                        values.Add(PSObject.AsPSObject(ReadYaml(parser, typeof(PSObject))));
                    }
                    else if (parser.TryConsume(out Scalar scalar))
                    {
                        values.Add(PSObject.AsPSObject(scalar.Value));
                    }
                }
                parser.Require<SequenceEnd>();
                parser.MoveNext();
                return new PSNoteProperty(name, values.ToArray());
            }
            else if (parser.Current is MappingStart)
            {
                return new PSNoteProperty(name, ReadYaml(parser, typeof(PSObject)));
            }
            else if (parser.TryConsume(out Scalar scalar))
            {
                return new PSNoteProperty(name, scalar.Value);
            }
            return null;
        }
    }

    /// <summary>
    /// A YAML resolver to convert any dictionary types to PSObjects instead.
    /// </summary>
    internal sealed class PSObjectYamlTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(Dictionary<object, object>) || nodeEvent is MappingStart)
            {
                currentType = typeof(PSObject);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A YAML converter for de/serializing a field map.
    /// </summary>
    internal sealed class FieldMapYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(FieldMap);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var result = new FieldMap();
            if (parser.TryConsume<MappingStart>(out _))
            {
                while (parser.TryConsume(out Scalar scalar))
                {
                    var fieldName = scalar.Value;
                    if (parser.TryConsume<SequenceStart>(out _))
                    {
                        var fields = new List<string>();
                        while (!parser.Accept<SequenceEnd>(out _))
                        {
                            if (parser.TryConsume<Scalar>(out scalar))
                                fields.Add(scalar.Value);
                        }
                        result.Set(fieldName, fields.ToArray());
                        parser.Require<SequenceEnd>();
                        parser.MoveNext();
                    }
                }
                parser.Require<MappingEnd>();
                parser.MoveNext();
            }
            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(FieldMap) && value == null)
            {
                emitter.Emit(new MappingStart());
                emitter.Emit(new MappingEnd());
            }
            if (value is not FieldMap map)
                return;

            emitter.Emit(new MappingStart());
            foreach (var field in map)
            {
                emitter.Emit(new Scalar(field.Key));
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                for (var i = 0; i < field.Value.Length; i++)
                {
                    emitter.Emit(new Scalar(field.Value[i]));
                }
                emitter.Emit(new SequenceEnd());
            }
            emitter.Emit(new MappingEnd());
        }
    }

    internal sealed class LanguageBlockDeserializer : INodeDeserializer
    {
        private const string FIELD_APIVERSION = "apiVersion";
        private const string FIELD_KIND = "kind";
        private const string FIELD_METADATA = "metadata";
        private const string FIELD_SPEC = "spec";

        private readonly INodeDeserializer _Next;
        private readonly SpecFactory _Factory;

        public LanguageBlockDeserializer(INodeDeserializer next)
        {
            _Next = next;
            _Factory = new SpecFactory();
        }

        bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (typeof(ResourceObject).IsAssignableFrom(expectedType))
            {
                var comment = HostHelper.GetCommentMeta(RunspaceContext.CurrentThread.Source.File.Path, reader.Current.Start.Line - 2, reader.Current.Start.Column);
                var resource = MapResource(reader, nestedObjectDeserializer, comment);
                value = new ResourceObject(resource);
                return true;
            }
            else
            {
                return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }
        }

        private IResource MapResource(IParser reader, Func<IParser, Type, object> nestedObjectDeserializer, CommentMetadata comment)
        {
            IResource result = null;
            string apiVersion = null;
            string kind = null;
            ResourceMetadata metadata = null;
            if (reader.TryConsume<MappingStart>(out _))
            {
                while (reader.TryConsume(out Scalar scalar))
                {
                    // Read apiVersion
                    if (TryApiVersion(reader, scalar, out var apiVersionValue))
                    {
                        apiVersion = apiVersionValue;
                    }
                    // Read kind
                    else if (TryKind(reader, scalar, out var kindValue))
                    {
                        kind = kindValue;
                    }
                    // Read metadata
                    else if (TryMetadata(reader, scalar, nestedObjectDeserializer, out var metadataValue))
                    {
                        metadata = metadataValue;
                    }
                    // Read spec
                    else if (apiVersion != null && kind != null && TrySpec(reader, scalar, apiVersion, kind, nestedObjectDeserializer, metadata, comment, out var resource))
                    {
                        result = resource;
                    }
                    else
                    {
                        reader.SkipThisAndNestedEvents();
                    }
                }
                reader.Require<MappingEnd>();
                reader.MoveNext();
            }
            return result;
        }

        private static bool TryApiVersion(IParser reader, Scalar scalar, out string kind)
        {
            kind = null;
            if (scalar.Value == FIELD_APIVERSION)
            {
                kind = reader.Consume<Scalar>().Value;
                return true;
            }
            return false;
        }

        private static bool TryKind(IParser reader, Scalar scalar, out string kind)
        {
            kind = null;
            if (scalar.Value == FIELD_KIND)
            {
                kind = reader.Consume<Scalar>().Value;
                return true;
            }
            return false;
        }

        private bool TryMetadata(IParser reader, Scalar scalar, Func<IParser, Type, object> nestedObjectDeserializer, out ResourceMetadata metadata)
        {
            metadata = null;
            if (scalar.Value != FIELD_METADATA)
                return false;

            if (reader.Current is MappingStart)
            {
                if (!_Next.Deserialize(reader, typeof(ResourceMetadata), nestedObjectDeserializer, out var value))
                    return false;

                metadata = (ResourceMetadata)value;
                return true;
            }
            return false;
        }

        private bool TrySpec(IParser reader, Scalar scalar, string apiVersion, string kind, Func<IParser, Type, object> nestedObjectDeserializer, ResourceMetadata metadata, CommentMetadata comment, out IResource spec)
        {
            spec = null;
            if (scalar.Value != FIELD_SPEC)
                return false;

            return TryResource(reader, apiVersion, kind, nestedObjectDeserializer, metadata, comment, out spec);
        }

        private bool TryResource(IParser reader, string apiVersion, string kind, Func<IParser, Type, object> nestedObjectDeserializer, ResourceMetadata metadata, CommentMetadata comment, out IResource spec)
        {
            spec = null;
            if (_Factory.TryDescriptor(apiVersion, kind, out var descriptor) && reader.Current is MappingStart)
            {
                if (!_Next.Deserialize(reader, descriptor.SpecType, nestedObjectDeserializer, out var value))
                    return false;

                spec = descriptor.CreateInstance(RunspaceContext.CurrentThread.Source.File, metadata, comment, value);
                return true;
            }
            return false;
        }
    }

    internal sealed class SelectorExpressionDeserializer : INodeDeserializer
    {
        private const string OPERATOR_IF = "if";

        private readonly INodeDeserializer _Next;
        private readonly SelectorExpressionFactory _Factory;

        public SelectorExpressionDeserializer(INodeDeserializer next)
        {
            _Next = next;
            _Factory = new SelectorExpressionFactory();
        }

        bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (typeof(SelectorExpression).IsAssignableFrom(expectedType))
            {
                var resource = MapOperator(OPERATOR_IF, reader, nestedObjectDeserializer);
                value = new SelectorIf(resource);
                return true;
            }
            else
            {
                return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }
        }

        private SelectorExpression MapOperator(string type, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            if (TryExpression(reader, type, nestedObjectDeserializer, out SelectorOperator result))
            {
                // If and Not
                if (reader.TryConsume<MappingStart>(out _))
                {
                    result.Add(MapExpression(reader, nestedObjectDeserializer));
                    reader.Require<MappingEnd>();
                    reader.MoveNext();
                }
                // AllOf and AnyOf
                else if (reader.TryConsume<SequenceStart>(out _))
                {
                    while (reader.TryConsume<MappingStart>(out _))
                    {
                        result.Add(MapExpression(reader, nestedObjectDeserializer));
                        reader.Require<MappingEnd>();
                        reader.MoveNext();
                    }
                    reader.Require<SequenceEnd>();
                    reader.MoveNext();
                }
            }
            return result;
        }

        private SelectorExpression MapCondition(string type, SelectorExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            if (TryExpression(reader, type, nestedObjectDeserializer, out SelectorCondition result))
            {
                while (!reader.Accept(out MappingEnd end))
                {
                    MapProperty(properties, reader, nestedObjectDeserializer, out _);
                }
                result.Add(properties);
            }
            return result;
        }

        private SelectorExpression MapExpression(IParser reader, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            SelectorExpression result = null;
            var properties = new SelectorExpression.PropertyBag();
            MapProperty(properties, reader, nestedObjectDeserializer, out var key);
            if (key != null && TryCondition(key))
            {
                result = MapCondition(key, properties, reader, nestedObjectDeserializer);
            }
            else if (TryOperator(key) && reader.Accept(out MappingStart mapping))
            {
                result = MapOperator(key, reader, nestedObjectDeserializer);
            }
            else if (TryOperator(key) && reader.Accept(out SequenceStart sequence))
            {
                result = MapOperator(key, reader, nestedObjectDeserializer);
            }
            return result;
        }

        private void MapProperty(SelectorExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object> nestedObjectDeserializer, out string name)
        {
            name = null;
            while (reader.TryConsume(out Scalar scalar))
            {
                var key = scalar.Value;
                if (TryCondition(key) || TryOperator(key))
                    name = key;

                if (reader.TryConsume(out scalar))
                {
                    properties[key] = scalar.Value;
                }
                else if (TryCondition(key) && reader.TryConsume<SequenceStart>(out _))
                {
                    var objects = new List<string>();
                    while (!reader.TryConsume<SequenceEnd>(out _))
                    {
                        if (reader.TryConsume(out scalar))
                        {
                            objects.Add(scalar.Value);
                        }
                    }
                    properties[key] = objects.ToArray();
                }
            }
        }

        private bool TryOperator(string key)
        {
            return _Factory.IsOperator(key);
        }

        private bool TryCondition(string key)
        {
            return _Factory.IsCondition(key);
        }

        private bool TryExpression<T>(IParser reader, string type, Func<IParser, Type, object> nestedObjectDeserializer, out T expression) where T : SelectorExpression
        {
            expression = null;
            if (_Factory.TryDescriptor(type, out var descriptor))
            {
                expression = (T)descriptor.CreateInstance(RunspaceContext.CurrentThread.Source.File, null);
                return expression != null;
            }
            return false;
        }
    }
}
