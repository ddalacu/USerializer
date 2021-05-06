``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.135 ms |  1.278 ms | 0.0701 ms |
| USerializerDeserialize | 27.150 ms | 14.819 ms | 0.8123 ms |
|   MessagePackSerialize |  9.664 ms |  5.257 ms | 0.2881 ms |
| MessagePackDeserialize | 31.966 ms |  7.356 ms | 0.4032 ms |
|         CerasSerialize | 56.595 ms | 27.439 ms | 1.5040 ms |
|       CerasDeserialize | 59.140 ms | 12.711 ms | 0.6967 ms |
