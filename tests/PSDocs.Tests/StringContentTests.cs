// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Runtime;

namespace PSDocs
{
    public sealed class StringContentTests
    {
        [Fact]
        public void ReadLines()
        {
            var lines = new StringContent(GetTestString1()).ReadLines();
            Assert.Equal("Line 1", lines[0]);
            Assert.Equal("Line 2", lines[1]);
            Assert.Equal("Line 3", lines[2]);

            lines = new StringContent(GetTestString2()).ReadLines();
            Assert.Equal("Line1", lines[0]);
            Assert.Equal("Line2", lines[1]);
            Assert.Equal("  Line3", lines[2]);

            lines = new StringContent(GetTestString3()).ReadLines();
            Assert.Equal("Line 1", lines[0]);
            Assert.Equal("Line 2", lines[1]);
            Assert.Equal(string.Empty, lines[2]);
            Assert.Equal("Line 3", lines[3]);
        }

        private static string GetTestString1()
        {
            return @"
Line 1
Line 2
Line 3
";
        }

        private static string GetTestString2()
        {
            return @"
    Line1
    Line2
      Line3
";
        }

        private static string GetTestString3()
        {
            return @"
  Line 1
  Line 2

  Line 3
";
        }
    }
}
