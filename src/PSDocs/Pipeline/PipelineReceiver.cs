// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net;
using Newtonsoft.Json;
using PSDocs.Configuration;
using PSDocs.Data;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PSDocs.Pipeline
{
    internal delegate IEnumerable<TargetObject> VisitTargetObject(TargetObject targetObject);
    internal delegate IEnumerable<TargetObject> VisitTargetObjectAction(TargetObject targetObject, VisitTargetObject next);

    internal static class PipelineReceiverActions
    {
        private static readonly TargetObject[] EmptyArray = Array.Empty<TargetObject>();

        public static IEnumerable<TargetObject> PassThru(TargetObject targetObject)
        {
            yield return targetObject;
        }

        public static IEnumerable<TargetObject> DetectInputFormat(TargetObject targetObject, VisitTargetObject next)
        {
            var pathExtension = GetPathExtension(targetObject);

            // Handle JSON
            if (pathExtension == ".json" || pathExtension == ".jsonc")
            {
                return ConvertFromJson(targetObject, next);
            }
            // Handle YAML
            else if (pathExtension == ".yaml" || pathExtension == ".yml")
            {
                return ConvertFromYaml(targetObject, next);
            }
            // Handle PowerShell Data
            else if (pathExtension == ".psd1")
            {
                return ConvertFromPowerShellData(targetObject, next);
            }
            return new TargetObject[] { targetObject };
        }

        public static IEnumerable<TargetObject> ConvertFromJson(TargetObject targetObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string, file or URI
            if (!IsAcceptedType(targetObject))
                return new TargetObject[] { targetObject };

            var json = ReadAsString(targetObject.Value, out var sourceInfo);
            var value = JsonConvert.DeserializeObject<PSObject[]>(json, new PSObjectArrayJsonConverter());
            return VisitItems(value, sourceInfo, next);
        }

        public static IEnumerable<TargetObject> ConvertFromYaml(TargetObject targetObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string, file or URI
            if (!IsAcceptedType(targetObject))
                return new TargetObject[] { targetObject };

            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSObjectYamlTypeResolver())
                .Build();

            var reader = ReadAsReader(targetObject.Value, out var sourceInfo);
            var parser = new YamlDotNet.Core.Parser(reader);
            var result = new List<TargetObject>();
            parser.TryConsume<StreamStart>(out _);
            while (parser.Current is DocumentStart)
            {
                var item = d.Deserialize<PSObject>(parser: parser);
                if (item == null)
                    continue;

                result.AddRange(VisitItem(item, sourceInfo, next));
            }
            return result.Count == 0 ? EmptyArray : result.ToArray();
        }

        public static IEnumerable<TargetObject> ConvertFromPowerShellData(TargetObject targetObject, VisitTargetObject next)
        {
            // Only attempt to deserialize if the input is a string or a file
            if (!IsAcceptedType(targetObject))
                return new TargetObject[] { targetObject };

            var data = ReadAsString(targetObject.Value, out var sourceInfo);
            var ast = System.Management.Automation.Language.Parser.ParseInput(data, out _, out _);
            var hashtables = ast.FindAll(item => item is System.Management.Automation.Language.HashtableAst, false);
            if (hashtables == null)
                return EmptyArray;

            var result = new List<PSObject>();
            foreach (var hashtable in hashtables)
            {
                if (hashtable?.Parent?.Parent?.Parent?.Parent == ast)
                    result.Add(PSObject.AsPSObject(hashtable.SafeGetValue()));
            }
            return VisitItems(result, sourceInfo, next);
        }

        public static IEnumerable<TargetObject> ReadObjectPath(TargetObject targetObject, VisitTargetObject source, string objectPath, bool caseSensitive)
        {
            if (!ObjectHelper.GetField(bindingContext: null, targetObject: targetObject, name: objectPath, caseSensitive: caseSensitive, value: out var nestedObject))
                return EmptyArray;

            var nestedType = nestedObject.GetType();
            if (typeof(IEnumerable).IsAssignableFrom(nestedType))
            {
                var result = new List<TargetObject>();
                foreach (var item in (nestedObject as IEnumerable))
                    result.Add(new TargetObject(PSObject.AsPSObject(item)));

                return result.ToArray();
            }
            else
            {
                return new TargetObject[] { new TargetObject(PSObject.AsPSObject(nestedObject)) };
            }
        }

        private static string GetPathExtension(TargetObject targetObject)
        {
            if (targetObject.Value.BaseObject is InputFileInfo inputFileInfo)
                return inputFileInfo.Extension;

            if (targetObject.Value.BaseObject is FileInfo fileInfo)
                return fileInfo.Extension;

            if (targetObject.Value.BaseObject is Uri uri)
                return Path.GetExtension(uri.OriginalString);

            return null;
        }

        private static bool IsAcceptedType(TargetObject targetObject)
        {
            return targetObject.Value.BaseObject is string ||
                targetObject.Value.BaseObject is InputFileInfo ||
                targetObject.Value.BaseObject is FileInfo ||
                targetObject.Value.BaseObject is Uri;
        }

        private static string ReadAsString(PSObject sourceObject, out InputFileInfo sourceInfo)
        {
            sourceInfo = null;
            if (sourceObject.BaseObject is string)
            {
                return sourceObject.BaseObject.ToString();
            }
            else if (sourceObject.BaseObject is InputFileInfo inputFileInfo)
            {
                sourceInfo = inputFileInfo;
                using var reader = new StreamReader(inputFileInfo.FullName);
                return reader.ReadToEnd();
            }
            else if (sourceObject.BaseObject is FileInfo fileInfo)
            {
                sourceInfo = new InputFileInfo(PSDocumentOption.GetRootedBasePath(""), fileInfo.FullName);
                using var reader = new StreamReader(fileInfo.FullName);
                return reader.ReadToEnd();
            }
            else
            {
                var uri = sourceObject.BaseObject as Uri;
                sourceInfo = new InputFileInfo(null, uri.ToString());
                using var webClient = new WebClient();
                return webClient.DownloadString(uri);
            }
        }

        private static TextReader ReadAsReader(PSObject sourceObject, out InputFileInfo sourceInfo)
        {
            sourceInfo = null;
            if (sourceObject.BaseObject is string)
            {
                return new StringReader(sourceObject.BaseObject.ToString());
            }
            else if (sourceObject.BaseObject is InputFileInfo inputFileInfo)
            {
                sourceInfo = inputFileInfo;
                return new StreamReader(inputFileInfo.FullName);
            }
            else if (sourceObject.BaseObject is FileInfo fileInfo)
            {
                sourceInfo = new InputFileInfo(PSDocumentOption.GetRootedBasePath(""), fileInfo.FullName);
                return new StreamReader(fileInfo.FullName);
            }
            else
            {
                var uri = sourceObject.BaseObject as Uri;
                sourceInfo = new InputFileInfo(null, uri.ToString());
                using var webClient = new WebClient();
                return new StringReader(webClient.DownloadString(uri));
            }
        }

        private static IEnumerable<TargetObject> VisitItem(PSObject value, InputFileInfo sourceInfo, VisitTargetObject next)
        {
            if (value == null)
                return EmptyArray;

            var items = next(new TargetObject(value));
            if (items == null)
                return EmptyArray;

            foreach (var i in items)
                NoteSource(i, sourceInfo);

            return items;
        }

        private static IEnumerable<TargetObject> VisitItems(IEnumerable<PSObject> value, InputFileInfo sourceInfo, VisitTargetObject next)
        {
            if (value == null)
                return EmptyArray;

            var result = new List<TargetObject>();
            foreach (var item in value)
                result.AddRange(VisitItem(item, sourceInfo, next));

            return result.Count == 0 ? EmptyArray : result.ToArray();
        }

        private static void NoteSource(TargetObject value, InputFileInfo source)
        {
            if (value == null || source == null)
                return;

            value.SetSourceInfo(source);
        }
    }
}
