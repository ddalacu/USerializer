``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  5.837 ms | 0.0194 ms | 0.0011 ms |
| USerializerDeserialize | 23.871 ms | 4.3390 ms | 0.2378 ms |
|   MessagePackSerialize |  8.135 ms | 0.0094 ms | 0.0005 ms |
| MessagePackDeserialize | 25.684 ms | 3.9811 ms | 0.2182 ms |
|         CerasSerialize | 50.212 ms | 1.0786 ms | 0.0591 ms |
|       CerasDeserialize | 45.617 ms | 2.4300 ms | 0.1332 ms |
