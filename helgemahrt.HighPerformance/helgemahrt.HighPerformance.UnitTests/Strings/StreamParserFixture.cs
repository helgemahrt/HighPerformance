using helgemahrt.HighPerformance.Strings;
using System.IO;
using System.Text;
using Xunit;

namespace helgemahrt.HighPerformance.UnitTests.Strings
{
    public class StreamParserFixture
    {
        [Fact]
        public void ParsingOnStack_Works()
        {
            // arrange
            string toParse = "Hello World !!!";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(toParse));
            memoryStream.Seek(0, SeekOrigin.Begin);
            MockPartProcessor mockPartProcessor = new MockPartProcessor();

            StreamParser sut = new StreamParser(new Splitter(' '), Encoding.UTF8);

            // act
            sut.ParseStream(memoryStream, mockPartProcessor);

            // assert
            Assert.Equal(3, mockPartProcessor.Count);
        }

        [Fact]
        public void ParsingOnStack_BufferSmallerThanString_Works()
        {
            // arrange
            string toParse = "Hello World !!!";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(toParse));
            memoryStream.Seek(0, SeekOrigin.Begin);
            MockPartProcessor mockPartProcessor = new MockPartProcessor();

            StreamParser sut = new StreamParser(new Splitter(' '), Encoding.UTF8, 4);

            // act
            sut.ParseStream(memoryStream, mockPartProcessor);

            // assert
            Assert.Equal(3, mockPartProcessor.Count);
        }

        [Fact]
        public void ParsingOnPool_Works()
        {
            // arrange
            string toParse = "Hello World !!!";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(toParse));
            memoryStream.Seek(0, SeekOrigin.Begin);
            MockPartProcessor mockPartProcessor = new MockPartProcessor();

            StreamParser sut = new StreamParser(new Splitter(' '), Encoding.UTF8, 1024 * 1024);

            // act
            sut.ParseStream(memoryStream, mockPartProcessor);

            // assert
            Assert.Equal(3, mockPartProcessor.Count);
        }
    }
}
