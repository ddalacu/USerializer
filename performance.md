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
|   USerializerSerialize |  9.693 ms |  3.214 ms | 0.1762 ms |
| USerializerDeserialize | 28.946 ms | 23.800 ms | 1.3045 ms |
|   MessagePackSerialize |  8.732 ms |  6.404 ms | 0.3510 ms |
| MessagePackDeserialize | 30.252 ms |  6.694 ms | 0.3669 ms |
|         CerasSerialize | 54.959 ms | 17.169 ms | 0.9411 ms |
|       CerasDeserialize | 57.327 ms | 36.736 ms | 2.0136 ms |
