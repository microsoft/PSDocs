
using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

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
}
