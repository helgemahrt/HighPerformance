using System;

namespace helgemahrt.HighPerformance.Strings
{
    public class Splitter
    {
        private readonly char _separator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="separator">The separator character this Splitter uses to parse strings.</param>
        public Splitter(char separator)
        {
            _separator = separator;
        }

        /// <summary>
        /// Parses stringToParse for separator characters and returns parts to the processor. 
        /// The tail end of the string will be returned and not passed to the processor if skipAndReturnLastLine is set to true. (Allowing for further composition of the part when using this with a small buffer)
        /// </summary>
        /// <param name="stringToParse"></param>
        /// <param name="processor"></param>
        /// <param name="skipAndReturnLastLine"></param>
        /// <returns>ReadOnlySpan<char>.Empty or the tail end of the stringToParse when skipAndReturnLastLine is set to true.</returns>
        public ReadOnlySpan<char> ExtractParts(ReadOnlySpan<char> stringToParse, IPartProcessor processor, bool skipAndReturnLastLine = false)
        {
            ReadOnlySpan<char> remainingString = stringToParse;

            int remainingStringEnd = remainingString.Length;
            int currentPartEnd = GetPartEnd(remainingString);

            while (remainingStringEnd > 0 &&
                    currentPartEnd != -1 &&
                    currentPartEnd < remainingStringEnd)
            {
                ReadOnlySpan<char> part = remainingString.Slice(0, currentPartEnd);
                processor.OnPart(part);

                remainingString = remainingString.Slice(currentPartEnd + 1);
                remainingStringEnd = remainingString.Length;
                currentPartEnd = GetPartEnd(remainingString);
            }

            if (currentPartEnd == -1)
            {
                ReadOnlySpan<char> part = remainingString.Slice(0, remainingStringEnd);
                if (skipAndReturnLastLine)
                {
                    return part;
                }
                else
                {
                    if (part.Length > 0)
                    {
                        processor.OnPart(part);
                    }
                }
            }

            return ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        /// Get the index of the next separator character.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private int GetPartEnd(ReadOnlySpan<char> text)
        {
            int partEnd = -1;

            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == _separator)
                {
                    partEnd = i;
                    break;
                }
            }

            return partEnd;
        }
    }
}
