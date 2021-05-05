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
|   USerializerSerialize |  9.677 ms |  1.813 ms | 0.0994 ms |
| USerializerDeserialize | 29.971 ms | 15.060 ms | 0.8255 ms |
|   MessagePackSerialize |  9.162 ms |  1.578 ms | 0.0865 ms |
| MessagePackDeserialize | 30.780 ms |  4.021 ms | 0.2204 ms |
|         CerasSerialize | 54.630 ms | 12.951 ms | 0.7099 ms |
|       CerasDeserialize | 55.110 ms | 32.183 ms | 1.7640 ms |
