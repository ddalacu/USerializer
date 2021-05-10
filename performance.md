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
|   USerializerSerialize |  7.522 ms | 1.7708 ms | 0.0971 ms |
| USerializerDeserialize | 26.314 ms | 6.6300 ms | 0.3634 ms |
|   MessagePackSerialize |  9.243 ms | 2.8760 ms | 0.1576 ms |
| MessagePackDeserialize | 30.661 ms | 2.5634 ms | 0.1405 ms |
|         CerasSerialize | 56.765 ms | 0.1510 ms | 0.0083 ms |
|       CerasDeserialize | 54.623 ms | 0.8571 ms | 0.0470 ms |
