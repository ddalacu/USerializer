``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.403
  [Host]   : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT
  ShortRun : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.280 ms | 0.4201 ms | 0.0230 ms |
| USerializerDeserialize | 23.952 ms | 0.9031 ms | 0.0495 ms |
|   MessagePackSerialize |  7.416 ms | 4.0403 ms | 0.2215 ms |
| MessagePackDeserialize | 23.350 ms | 0.5785 ms | 0.0317 ms |
|         CerasSerialize | 42.740 ms | 2.4500 ms | 0.1343 ms |
|       CerasDeserialize | 43.256 ms | 2.8202 ms | 0.1546 ms |
