``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.056 ms | 0.0592 ms | 0.0032 ms |
| USerializerDeserialize | 28.755 ms | 4.6896 ms | 0.2571 ms |
|   MessagePackSerialize |  9.967 ms | 1.2790 ms | 0.0701 ms |
| MessagePackDeserialize | 30.888 ms | 4.8045 ms | 0.2634 ms |
|         CerasSerialize | 57.118 ms | 0.5532 ms | 0.0303 ms |
|       CerasDeserialize | 60.224 ms | 9.0106 ms | 0.4939 ms |
