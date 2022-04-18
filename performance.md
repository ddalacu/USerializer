``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.407
  [Host]   : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT
  ShortRun : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  8.735 ms |  0.4834 ms | 0.0265 ms |
| USerializerDeserialize | 27.654 ms | 10.6957 ms | 0.5863 ms |
|   MessagePackSerialize | 10.116 ms |  8.3918 ms | 0.4600 ms |
| MessagePackDeserialize | 32.502 ms | 21.1293 ms | 1.1582 ms |
|         CerasSerialize | 52.403 ms | 11.5287 ms | 0.6319 ms |
|       CerasDeserialize | 55.457 ms | 20.2564 ms | 1.1103 ms |
