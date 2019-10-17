using System;

namespace helgemahrt.HighPerformance.Json
{
    public interface IJsonTokenProcessor
    {
        void OnJsonToken(JsonTokenTypeEnum jsonTokenType, ReadOnlySpan<char> token);
    }
}
