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
|   USerializerSerialize |  9.709 ms | 0.8107 ms | 0.0444 ms |
| USerializerDeserialize | 30.491 ms | 3.5116 ms | 0.1925 ms |
|   MessagePackSerialize |  9.378 ms | 0.0703 ms | 0.0039 ms |
| MessagePackDeserialize | 30.077 ms | 0.9081 ms | 0.0498 ms |
|         CerasSerialize | 59.456 ms | 1.1417 ms | 0.0626 ms |
|       CerasDeserialize | 54.357 ms | 2.1160 ms | 0.1160 ms |
