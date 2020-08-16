using System.ComponentModel;

namespace PSDocs.Models
{
    internal sealed class TestModel
    {
        public TestModel()
        {
            Name = "Test";
            Description = "This is a\r\ndescription\r\nsplit\r\nover\r\nmultiple\r\nlines.";
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
