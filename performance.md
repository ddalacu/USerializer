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
|   USerializerSerialize | 10.505 ms | 0.7429 ms | 0.0407 ms |
| USerializerDeserialize | 32.692 ms | 3.5221 ms | 0.1931 ms |
|   MessagePackSerialize |  9.826 ms | 2.2439 ms | 0.1230 ms |
| MessagePackDeserialize | 31.353 ms | 1.2545 ms | 0.0688 ms |
|         CerasSerialize | 57.837 ms | 0.6437 ms | 0.0353 ms |
|       CerasDeserialize | 56.526 ms | 3.6206 ms | 0.1985 ms |
