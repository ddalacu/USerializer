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
|   USerializerSerialize |  8.398 ms |  2.031 ms | 0.1113 ms |
| USerializerDeserialize | 26.474 ms |  1.575 ms | 0.0863 ms |
|   MessagePackSerialize |  9.821 ms |  2.406 ms | 0.1319 ms |
| MessagePackDeserialize | 28.948 ms |  1.127 ms | 0.0618 ms |
|         CerasSerialize | 59.105 ms | 10.324 ms | 0.5659 ms |
|       CerasDeserialize | 56.379 ms |  3.270 ms | 0.1792 ms |
