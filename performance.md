``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.102
  [Host]   : .NET Core 9.0.14 (CoreCLR 9.0.1426.11910, CoreFX 9.0.1426.11910), X64 RyuJIT
  ShortRun : .NET Core 9.0.14 (CoreCLR 9.0.1426.11910, CoreFX 9.0.1426.11910), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  9.864 ms |  4.4520 ms | 0.2440 ms |
| USerializerDeserialize | 29.243 ms |  0.2146 ms | 0.0118 ms |
|    MemoryPackSerialize | 14.800 ms |  0.3660 ms | 0.0201 ms |
|  MemoryPackDeserialize | 20.941 ms |  2.7364 ms | 0.1500 ms |
|         CerasSerialize | 44.033 ms |  1.2448 ms | 0.0682 ms |
|       CerasDeserialize | 48.399 ms | 10.1007 ms | 0.5537 ms |
