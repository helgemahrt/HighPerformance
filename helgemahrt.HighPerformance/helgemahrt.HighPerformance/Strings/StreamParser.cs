using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace helgemahrt.HighPerformance.Strings
{
    public class StreamParser
    {
        private readonly Splitter _splitter;
        private readonly Encoding _encoding;
        private readonly int _bufferSize;
        private readonly int _bytesPerChar;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splitter">Splitter used for parsing</param>
        /// <param name="encoding">Encoding of the string</param>
        /// <param name="bufferSize">The size of the buffer to use for parsing. Should be bigger than at least half of the largest part size. Buffers <100kb will be created on the stack (faster). Buffers >=100kb will be created on the ArrayPool.</param>
        public StreamParser(Splitter splitter, Encoding encoding, int bufferSize = 1024 * 10)
        {
            _bufferSize = bufferSize;

            _splitter = splitter ?? throw new ArgumentNullException(nameof(splitter));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            
            _bytesPerChar = _encoding.GetByteCount("a");
        }

        public void ParseStream(Stream stream, IPartProcessor partProcessor)
        {
            // don't try to allocate more than 300kb on the stack for parsing
            // it easily leads to a stack overflow
            if (_bufferSize < 1024 * 100)
            {
                ParseStreamOnStack(stream, partProcessor);
            }
            else
            {
                ParseStreamOnArrayPool(stream, partProcessor);
            }
        }

        private void ParseStreamOnArrayPool(Stream stream, IPartProcessor partProcessor)
        {
            byte[] bytesFromPool = ArrayPool<byte>.Shared.Rent(_bufferSize);
            char[] charsFromPool = ArrayPool<char>.Shared.Rent(bytesFromPool.Length / _bytesPerChar * 2);

            try
            {
                ParseStreamInternal(stream, partProcessor, bytesFromPool, charsFromPool);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytesFromPool);
                ArrayPool<char>.Shared.Return(charsFromPool);
            }
        }

        private void ParseStreamOnStack(Stream stream, IPartProcessor partProcessor)
        {
            Span<byte> byteSpan = stackalloc byte[_bufferSize];
            Span<char> stringSpan = stackalloc char[byteSpan.Length / _bytesPerChar * 2];

            ParseStreamInternal(stream, partProcessor, byteSpan, stringSpan);
        }

        private void ParseStreamInternal(Stream stream, IPartProcessor partProcessor, Span<byte> byteSpan, Span<char> stringSpan)
        {
            int remainingLength = 0;

            int read = stream.Read(byteSpan);
            while (read > 0)
            {
                Span<char> stringToEncode = stringSpan.Slice(remainingLength, stringSpan.Length - remainingLength);
                int endOfString = _encoding.GetChars(byteSpan.Slice(0, read), stringToEncode) + remainingLength;

                ReadOnlySpan<char> remaining = _splitter.ExtractParts(stringSpan.Slice(0, endOfString), partProcessor, true);

                remaining.CopyTo(stringSpan);
                remainingLength = remaining.Length;

                read = stream.Read(byteSpan);
            }

            if (remainingLength > 0)
            {
                partProcessor.OnPart(stringSpan.Slice(0, remainingLength));
            }
        }
    }
}
