// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSDocs.Models
{
    internal sealed class TestModel
    {
        public TestModel()
        {
            Name = "Test";
            Description = "This is a\r\ndescription\r\nsplit\r\nover\r\nmultiple\r\nlines.";
            Generator = "PSDocs";
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Generator { get; set; }
    }
}
