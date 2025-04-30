``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=9.0.203
  [Host]   : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT
  ShortRun : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  9.094 ms |  0.2801 ms | 0.0154 ms |
| USerializerDeserialize | 28.532 ms | 11.7201 ms | 0.6424 ms |
|    MemoryPackSerialize | 14.605 ms |  0.4668 ms | 0.0256 ms |
|  MemoryPackDeserialize | 20.414 ms |  1.6700 ms | 0.0915 ms |
|         CerasSerialize | 43.861 ms |  0.2094 ms | 0.0115 ms |
|       CerasDeserialize | 47.236 ms |  3.5801 ms | 0.1962 ms |
