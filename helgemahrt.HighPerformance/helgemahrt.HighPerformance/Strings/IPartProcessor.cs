using System;

namespace helgemahrt.HighPerformance.Strings
{
    public interface IPartProcessor
    {
        void OnPart(ReadOnlySpan<char> part);
    }
}
