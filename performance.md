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
|   USerializerSerialize |  7.121 ms | 0.0417 ms | 0.0023 ms |
| USerializerDeserialize | 24.362 ms | 0.1905 ms | 0.0104 ms |
|   MessagePackSerialize |  8.139 ms | 0.0360 ms | 0.0020 ms |
| MessagePackDeserialize | 26.216 ms | 3.6661 ms | 0.2010 ms |
|         CerasSerialize | 47.721 ms | 1.9660 ms | 0.1078 ms |
|       CerasDeserialize | 45.767 ms | 4.9975 ms | 0.2739 ms |
