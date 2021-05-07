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
|   USerializerSerialize |  7.507 ms |  2.093 ms | 0.1147 ms |
| USerializerDeserialize | 26.168 ms | 27.991 ms | 1.5343 ms |
|   MessagePackSerialize |  9.053 ms |  2.834 ms | 0.1553 ms |
| MessagePackDeserialize | 30.106 ms |  6.615 ms | 0.3626 ms |
|         CerasSerialize | 55.766 ms | 33.469 ms | 1.8345 ms |
|       CerasDeserialize | 56.385 ms | 28.746 ms | 1.5757 ms |
