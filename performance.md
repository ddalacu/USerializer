``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.301
  [Host]   : .NET Core 5.0.7 (CoreCLR 5.0.721.25508, CoreFX 5.0.721.25508), X64 RyuJIT
  ShortRun : .NET Core 5.0.7 (CoreCLR 5.0.721.25508, CoreFX 5.0.721.25508), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.387 ms | 0.5029 ms | 0.0276 ms |
| USerializerDeserialize | 27.658 ms | 2.8502 ms | 0.1562 ms |
|   MessagePackSerialize |  9.552 ms | 0.1173 ms | 0.0064 ms |
| MessagePackDeserialize | 29.916 ms | 7.4578 ms | 0.4088 ms |
|         CerasSerialize | 58.814 ms | 5.1625 ms | 0.2830 ms |
|       CerasDeserialize | 55.627 ms | 3.4787 ms | 0.1907 ms |
