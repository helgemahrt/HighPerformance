using helgemahrt.HighPerformance.Strings;
using System;

namespace helgemahrt.HighPerformance.UnitTests.Strings
{

    internal class MockPartProcessor : IPartProcessor
    {
        public int Count { get; set; }

        public void OnPart(ReadOnlySpan<char> part)
        {
            ++Count;
        }
    }
}
