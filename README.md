# HighPerformance
High performance .NET Core utility classes I use in my development.

So far there are the following utilities:
* Splitter - allows splitting strings by an arbitrary character (e.g. by the new-line character)
* StreamParser - allows splitting streams using Splitter (e.g. to extract lines)
* JsonTokenizer - forward-only JSON parsing

## Usage
### Splitter
```csharp
public class PartCounter : IPartProcessor
{
    public int Count { get; set; }

    public void OnPart(ReadOnlySpan<char> part)
    {
        Count++;
    }
}

public int CountLines(string stringToSplit)
{
    Splitter newLineSplitter = new Splitter('\n');
    PartCounter partCounter = new PartCounter();
    newLineSplitter.ExtractParts(stringToSplit, partCounter);
    return partCounter.Count;
}
```

### StreamParser
```csharp
public int CountLinesInFile_OnStackMemory(string fileName)
{
    StreamParser stackStreamParser = new StreamParser(new Splitter('\n'), Encoding.UTF8);
    PartCounter partCounter = new PartCounter();
    using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    {
        stackStreamParser.ParseStream(stream, partCounter);
    }
    return partCounter.Count;
}

public int CountLinesInFile_OnHeap(string fileName)
{
    StreamParser arrayPoolStreamParser = new StreamParser(new Splitter('\n'), Encoding.UTF8, 1024 * 1024);
    PartCounter partCounter = new PartCounter();
    using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    {
        arrayPoolStreamParser.ParseStream(stream, partCounter);
    }
    return partCounter.Count;
}
```

### JsonTokenizer
```csharp
private MessageRefStruct DeserializeMessage(ReadOnlySpan<char> line)
{
    ReadOnlySpan<char> Name = ReadOnlySpan<char>.Empty;
    ReadOnlySpan<char> Value = ReadOnlySpan<char>.Empty;
    DateTime TimeStamp = DateTime.MinValue;
    int SequenceNumber = 0;

    JsonTokenizer tokenizer = new JsonTokenizer();

    tokenizer.GetNextToken(line); // object start

    tokenizer.GetNextToken(line); // Name
    Name = JsonTokenizer.GetTokenValue(tokenizer.GetNextToken(line), line);
    
    tokenizer.GetNextToken(line); // Value
    Value = JsonTokenizer.GetTokenValue(tokenizer.GetNextToken(line), line);

    tokenizer.GetNextToken(line); // TimeStamp
    TimeStamp = DateTime.Parse(JsonTokenizer.GetTokenValue(tokenizer.GetNextToken(line), line));

    tokenizer.GetNextToken(line); // SequenceNumber
    SequenceNumber = int.Parse(JsonTokenizer.GetTokenValue(tokenizer.GetNextToken(line), line));

    return new MessageRefStruct(
        Name,
        Value,
        TimeStamp,
        SequenceNumber);
}
```

## Benchmark results
``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.19002
Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.100-preview1-014459
  [Host]     : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.2 (CoreCLR 4.6.27317.07, CoreFX 4.6.27318.02), 64bit RyuJIT


```
|                                  Method |           Mean |         Error |        StdDev |      Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
|---------------------------------------- |---------------:|--------------:|--------------:|-----------:|---------:|---------:|------------:|
|             StringSplit_HighPerformance |       8.660 us |     0.3577 us |      1.038 us |          - |        - |        - |        24 B |
|                      StringSplit_DotNet |      24.122 us |     0.7960 us |      2.322 us |     5.0964 |        - |        - |     21432 B |

|                                  Method |           Mean |         Error |        StdDev |      Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
|---------------------------------------- |---------------:|--------------:|--------------:|-----------:|---------:|---------:|------------:|
|          FileSplit_Stack_HighPerfomance |     427.712 us |     8.5323 us |      8.380 us |          - |        - |        - |       576 B |
|                        FileSplit_DotNet |     630.402 us |    12.4553 us |     25.722 us |    85.9375 |        - |        - |    364288 B |
|      FileSplit_ArrayPool_HighPerfomance |   1,082.128 us |    22.2175 us |     57.746 us |   498.0469 | 498.0469 | 498.0469 |   4194904 B |

|                                  Method |           Mean |         Error |        StdDev |      Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
|---------------------------------------- |---------------:|--------------:|--------------:|-----------:|---------:|---------:|------------:|
|     ParseJsonFile_Stack_HighPerformance |     300.905 us |     3.8917 us |      3.450 us |          - |        - |        - |       576 B |
| ParseJsonFile_ArrayPool_HighPerformance |   1,076.529 us |    21.4944 us |     51.499 us |   457.0313 | 457.0313 | 457.0313 |   4195306 B |
|            ParseJsonFile_NewtonsoftJson | 249,141.393 us | 6,172.4109 us | 18,199.493 us | 25000.0000 |        - |        - | 106397240 B |
