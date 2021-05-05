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
|   USerializerSerialize |  9.329 ms |  3.657 ms | 0.2004 ms |
| USerializerDeserialize | 28.537 ms | 17.293 ms | 0.9479 ms |
|   MessagePackSerialize |  8.372 ms |  2.724 ms | 0.1493 ms |
| MessagePackDeserialize | 27.969 ms | 17.266 ms | 0.9464 ms |
|         CerasSerialize | 52.494 ms |  7.656 ms | 0.4196 ms |
|       CerasDeserialize | 53.062 ms | 26.188 ms | 1.4355 ms |
