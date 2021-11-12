``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.403
  [Host]   : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT
  ShortRun : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  9.994 ms |  2.355 ms | 0.1291 ms |
| USerializerDeserialize | 30.417 ms |  5.217 ms | 0.2860 ms |
|   MessagePackSerialize | 10.305 ms | 13.430 ms | 0.7362 ms |
| MessagePackDeserialize | 33.234 ms | 19.560 ms | 1.0722 ms |
|         CerasSerialize | 59.015 ms | 25.715 ms | 1.4095 ms |
|       CerasDeserialize | 61.708 ms | 28.058 ms | 1.5379 ms |
