``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  9.009 ms |  1.3428 ms | 0.0736 ms |
| USerializerDeserialize | 32.182 ms | 20.6346 ms | 1.1311 ms |
|   MessagePackSerialize |  9.552 ms |  0.7288 ms | 0.0399 ms |
| MessagePackDeserialize | 29.673 ms | 13.6525 ms | 0.7483 ms |
|         CerasSerialize | 55.983 ms | 33.8209 ms | 1.8538 ms |
|       CerasDeserialize | 56.320 ms |  8.6065 ms | 0.4718 ms |
