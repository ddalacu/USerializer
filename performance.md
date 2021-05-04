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
|   USerializerSerialize |  9.534 ms |  2.646 ms | 0.1451 ms |
| USerializerDeserialize | 29.184 ms |  4.848 ms | 0.2658 ms |
|   MessagePackSerialize |  8.910 ms |  3.088 ms | 0.1692 ms |
| MessagePackDeserialize | 29.621 ms |  5.245 ms | 0.2875 ms |
|         CerasSerialize | 54.987 ms |  9.341 ms | 0.5120 ms |
|       CerasDeserialize | 56.490 ms | 17.183 ms | 0.9419 ms |
