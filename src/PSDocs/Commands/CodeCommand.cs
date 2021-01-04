
using Newtonsoft.Json;
using PSDocs.Models;
using PSDocs.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSDocs.Commands
{
    [Cmdlet(VerbsCommon.Format, LanguageKeywords.Code)]
    internal sealed class CodeCommand : KeywordCmdlet
    {
        private const string ParameterSet_Pipeline = "Pipeline";
        private const string ParameterSet_InfoString = "InfoString";
        private const string ParameterSet_PipelineInfoString = "PipelineInfoString";
        private const string ParameterSet_Default = "Default";

        private const string Info_ScriptBlock = "powershell";

        private List<string> _Content;

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_InfoString)]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_PipelineInfoString)]
        public string Info { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ParameterSet_Default, ValueFromPipeline = false)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ParameterSet_InfoString, ValueFromPipeline = false)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_Pipeline, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_PipelineInfoString, ValueFromPipeline = true)]
        [AllowNull]
        [AllowEmptyString]
        [AllowEmptyCollection]
        public object InputObject { get; set; }

        protected override void BeginProcessing()
        {
            _Content = new List<string>();
        }

        protected override void ProcessRecord()
        {
            var content = ParameterSetName == ParameterSet_Pipeline || ParameterSetName == ParameterSet_PipelineInfoString ? InputObject : Body;
            AddContent(content);
        }

        protected override void EndProcessing()
        {
            try
            {
                var node = ModelHelper.NewCode();
                node.Info = Info;
                node.Content = string.Join(Environment.NewLine, _Content.ToArray());
                WriteObject(node);
            }
            finally
            {
                _Content.Clear();
            }
        }

        private void AddContent(object input)
        {
            var result = TryConvertScriptBlock(input, Info, out StringContent content) ||
                TryConvertJson(input, Info, out content) ||
                TryConvertYaml(input, Info, out content) ||
                TryConvertString(input, Info, out content);

            if (!result || content == null)
                return;

            _Content.AddRange(content.ReadLines());
            Info = content.Info;
        }

        private static bool TryConvertJson(object input, string info, out StringContent content)
        {
            content = null;
            if (!IsJson(info))
                return false;

            var baseObject = ObjectHelper.GetBaseObject(input);
            var baseType = baseObject.GetType();
            if (baseObject is string || baseType.IsValueType)
                return false;

            using (StringWriter sw = new StringWriter())
            {
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    writer.Indentation = 4;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Insert(0, new PSObjectJsonConverter());
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, input);
                }
                content = new StringContent(sw.ToString(), info);
            }
            return true;
        }

        private static bool TryConvertYaml(object input, string info, out StringContent content)
        {
            content = null;
            if (!IsYaml(info))
                return false;

            var baseObject = ObjectHelper.GetBaseObject(input);
            var baseType = baseObject.GetType();
            if (baseObject is string || baseObject is Include || baseType.IsValueType)
                return false;

            var s = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeInspector(inspector => new PSObjectTypeInspector(inspector))
                .Build();

            content = new StringContent(s.Serialize(input), info);
            return true;
        }

        private static bool TryConvertScriptBlock(object input, string info, out StringContent content)
        {
            content = null;
            if (!(input is ScriptBlock s))
                return false;

            content = new StringContent(s.ToString(), info ?? Info_ScriptBlock);
            return true;
        }

        private static bool TryConvertString(object input, string info, out StringContent content)
        {
            content = null;
            if (input == null)
                return false;

            content = new StringContent(input.ToString(), info);
            return true;
        }

        private static bool IsYaml(string info)
        {
            return StringComparer.OrdinalIgnoreCase.Equals("yaml", info) ||
                StringComparer.OrdinalIgnoreCase.Equals("yml", info);
        }

        private static bool IsJson(string info)
        {
            return StringComparer.OrdinalIgnoreCase.Equals("json", info);
        }
    }
}
