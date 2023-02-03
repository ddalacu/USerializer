``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 22.04
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.408
  [Host]   : .NET Core 5.0.17 (CoreCLR 5.0.1722.21314, CoreFX 5.0.1722.21314), X64 RyuJIT
  ShortRun : .NET Core 5.0.17 (CoreCLR 5.0.1722.21314, CoreFX 5.0.1722.21314), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.845 ms | 0.1809 ms | 0.0099 ms |
| USerializerDeserialize | 19.346 ms | 6.6184 ms | 0.3628 ms |
|   MessagePackSerialize |  6.882 ms | 0.0140 ms | 0.0008 ms |
| MessagePackDeserialize | 23.018 ms | 5.2296 ms | 0.2867 ms |
|         CerasSerialize | 46.663 ms | 1.7155 ms | 0.0940 ms |
|       CerasDeserialize | 44.486 ms | 5.6493 ms | 0.3097 ms |
