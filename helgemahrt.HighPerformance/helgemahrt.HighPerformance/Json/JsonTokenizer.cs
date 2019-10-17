using System;

namespace helgemahrt.HighPerformance.Json
{
    public class JsonTokenizer
    {
        public int CurrentIndex { get; set; } = 0;

        private bool _insideProperty = false;

        public void Reset()
        {
            CurrentIndex = 0;
        }

        /// <summary>
        /// Seeks the jsonString for the next token, starting at the CurrentIndex.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns>Returns: (json token type, start index, length of token)</returns>
        public (JsonTokenTypeEnum, int, int) GetNextToken(ReadOnlySpan<char> jsonString)
        {
            for (; CurrentIndex < jsonString.Length; ++CurrentIndex)
            {
                switch (jsonString[CurrentIndex])
                {
                    case '{':
                        {
                            return (JsonTokenTypeEnum.StartObject, CurrentIndex++, 1);
                        }
                    case '}':
                        {
                            return (JsonTokenTypeEnum.EndObject, CurrentIndex++, 1);
                        }
                    case '[':
                        {
                            return (JsonTokenTypeEnum.StartArray, CurrentIndex++, 1);
                        }
                    case ']':
                        {
                            return (JsonTokenTypeEnum.EndArray, CurrentIndex++, 1);
                        }
                    case ':':
                        {
                            _insideProperty = true;
                            break;
                        }
                    case ',':
                    case '\n':
                    case '\r':
                        {
                            _insideProperty = false;
                            break;
                        }
                    case '\t':
                    case ' ':
                        {
                            // ignore whitespaces
                            break;
                        }
                    case '"':
                    default:
                        {
                            (int index, int length) = GetTokenContent(jsonString);
                            if (!_insideProperty)
                            {
                                return (JsonTokenTypeEnum.PropertyName, index, length);
                            }
                            else
                            {
                                _insideProperty = false;
                                return (JsonTokenTypeEnum.PropertyValue, index, length);
                            }
                        }
                }
            }

            return (JsonTokenTypeEnum.None, CurrentIndex, 0);
        }

        private (int, int) GetTokenContent(ReadOnlySpan<char> jsonString)
        {
            if (jsonString[CurrentIndex] == '"')
            {
                // seek closing "
                return GetQuotationContent(jsonString);
            }
            else
            {
                // seek closing ,
                int startIndex = CurrentIndex;
                int lastIndexWithValue = startIndex;
                while (!IsPropertyValueEnd(jsonString[CurrentIndex++])) { }
                return (startIndex, CurrentIndex - startIndex - 1);
            }
        }

        private bool IsPropertyValueEnd(char value)
        {
            return
                value == ',' ||
                value == ' ' ||
                value == '\t' ||
                value == '\n' ||
                value == '\r';
        }

        private (int, int) GetQuotationContent(ReadOnlySpan<char> jsonString)
        {
            ++CurrentIndex; // skip leading "
            int startIndex = CurrentIndex;
            while (jsonString[CurrentIndex++] != '"')
            {
                // seek closing "
            }

            return (startIndex, CurrentIndex - startIndex - 1);
        }

        public static ReadOnlySpan<char> GetTokenValue((JsonTokenTypeEnum jsonTokenType, int startIndex, int length) tokenData, ReadOnlySpan<char> jsonString)
        {
            switch (tokenData.jsonTokenType)
            {
                case JsonTokenTypeEnum.PropertyName:
                case JsonTokenTypeEnum.PropertyValue:
                    return jsonString.Slice(tokenData.startIndex, tokenData.length);
                case JsonTokenTypeEnum.None:
                case JsonTokenTypeEnum.StartObject:
                case JsonTokenTypeEnum.StartArray:
                case JsonTokenTypeEnum.EndObject:
                case JsonTokenTypeEnum.EndArray:
                default:
                    return ReadOnlySpan<char>.Empty;
            }
        }
    }
}
