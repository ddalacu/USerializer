``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize | 10.122 ms |  0.8423 ms | 0.0462 ms |
| USerializerDeserialize | 29.734 ms |  5.9433 ms | 0.3258 ms |
|   MessagePackSerialize |  9.561 ms |  6.6320 ms | 0.3635 ms |
| MessagePackDeserialize | 29.379 ms |  3.9497 ms | 0.2165 ms |
|         CerasSerialize | 55.281 ms |  6.4357 ms | 0.3528 ms |
|       CerasDeserialize | 56.417 ms | 11.9577 ms | 0.6554 ms |
