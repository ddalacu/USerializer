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
|   USerializerSerialize |  8.518 ms |  1.029 ms | 0.0564 ms |
| USerializerDeserialize | 28.430 ms |  7.457 ms | 0.4087 ms |
|   MessagePackSerialize |  9.866 ms |  8.523 ms | 0.4672 ms |
| MessagePackDeserialize | 32.449 ms | 10.526 ms | 0.5770 ms |
|         CerasSerialize | 69.088 ms |  1.584 ms | 0.0868 ms |
|       CerasDeserialize | 60.920 ms | 13.640 ms | 0.7477 ms |
