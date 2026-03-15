``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.102
  [Host]   : .NET Core 9.0.14 (CoreCLR 9.0.1426.11910, CoreFX 9.0.1426.11910), X64 RyuJIT
  ShortRun : .NET Core 9.0.14 (CoreCLR 9.0.1426.11910, CoreFX 9.0.1426.11910), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 11.52 ms | 0.763 ms | 0.042 ms |
| USerializerDeserialize | 26.44 ms | 1.291 ms | 0.071 ms |
|    MemoryPackSerialize | 14.81 ms | 0.360 ms | 0.020 ms |
|  MemoryPackDeserialize | 20.57 ms | 4.248 ms | 0.233 ms |
|         CerasSerialize | 44.61 ms | 0.608 ms | 0.033 ms |
|       CerasDeserialize | 47.59 ms | 4.875 ms | 0.267 ms |
