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
|   USerializerSerialize |  8.943 ms |  2.677 ms | 0.1467 ms |
| USerializerDeserialize | 29.283 ms | 10.862 ms | 0.5954 ms |
|   MessagePackSerialize |  9.879 ms |  3.510 ms | 0.1924 ms |
| MessagePackDeserialize | 32.249 ms |  6.873 ms | 0.3767 ms |
|         CerasSerialize | 59.182 ms | 16.284 ms | 0.8926 ms |
|       CerasDeserialize | 61.825 ms |  7.796 ms | 0.4273 ms |
