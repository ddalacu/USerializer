``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 22.04
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=7.0.102
  [Host]   : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT
  ShortRun : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.867 ms | 0.4631 ms | 0.0254 ms |
| USerializerDeserialize | 23.159 ms | 0.5415 ms | 0.0297 ms |
|   MessagePackSerialize |  6.371 ms | 0.0743 ms | 0.0041 ms |
| MessagePackDeserialize | 21.043 ms | 6.2283 ms | 0.3414 ms |
|         CerasSerialize | 45.322 ms | 4.8499 ms | 0.2658 ms |
|       CerasDeserialize | 43.117 ms | 5.0959 ms | 0.2793 ms |
