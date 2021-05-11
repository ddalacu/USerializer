``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.779 ms | 0.3539 ms | 0.0194 ms |
| USerializerDeserialize | 26.566 ms | 3.1543 ms | 0.1729 ms |
|   MessagePackSerialize |  9.363 ms | 1.8857 ms | 0.1034 ms |
| MessagePackDeserialize | 29.783 ms | 7.9416 ms | 0.4353 ms |
|         CerasSerialize | 58.578 ms | 5.4527 ms | 0.2989 ms |
|       CerasDeserialize | 53.706 ms | 3.2960 ms | 0.1807 ms |
