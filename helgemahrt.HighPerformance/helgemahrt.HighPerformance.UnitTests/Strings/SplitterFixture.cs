using helgemahrt.HighPerformance.Strings;
using System;
using Xunit;

namespace helgemahrt.HighPerformance.UnitTests.Strings
{
    public class SplitterFixture
    {
        [Fact]
        public void CommaSeparatedSplitting_Works()
        {
            // arrange
            MockPartProcessor mockPartProcessor = new MockPartProcessor();
            string toParse = "Hello,World,!!!";

            Splitter sut = new Splitter(',');

            // act
            sut.ExtractParts(toParse, mockPartProcessor);

            // assert
            Assert.Equal(3, mockPartProcessor.Count);
        }

        [Fact]
        public void CommaSeparatedSplitting_ReturnsTailEnd_Works()
        {
            // arrange
            MockPartProcessor mockPartProcessor = new MockPartProcessor();
            string toParse = "Hello,World,!!!";

            Splitter sut = new Splitter(',');

            // act
            ReadOnlySpan<char> tail = sut.ExtractParts(toParse, mockPartProcessor, true);

            // assert
            Assert.Equal(2, mockPartProcessor.Count);
            Assert.True(tail.SequenceEqual("!!!"));
        }
    }
}
