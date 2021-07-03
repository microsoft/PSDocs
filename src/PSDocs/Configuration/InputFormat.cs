// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSDocs.Configuration
{
    /// <summary>
    /// The formats to convert input from.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputFormat
    {
        None = 0,

        Yaml = 1,

        Json = 2,

        PowerShellData = 3,

        Detect = 255
    }
}
