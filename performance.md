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
|   USerializerSerialize |  7.280 ms | 0.0251 ms | 0.0014 ms |
| USerializerDeserialize | 22.996 ms | 4.4581 ms | 0.2444 ms |
|   MessagePackSerialize |  8.139 ms | 0.0452 ms | 0.0025 ms |
| MessagePackDeserialize | 24.218 ms | 2.5464 ms | 0.1396 ms |
|         CerasSerialize | 47.631 ms | 0.7031 ms | 0.0385 ms |
|       CerasDeserialize | 46.344 ms | 3.1216 ms | 0.1711 ms |
