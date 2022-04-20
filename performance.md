``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.407
  [Host]   : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT
  ShortRun : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.453 ms | 0.0318 ms | 0.0017 ms |
| USerializerDeserialize | 22.437 ms | 1.2548 ms | 0.0688 ms |
|   MessagePackSerialize |  8.044 ms | 0.3226 ms | 0.0177 ms |
| MessagePackDeserialize | 26.130 ms | 4.3904 ms | 0.2407 ms |
|         CerasSerialize | 50.364 ms | 0.7334 ms | 0.0402 ms |
|       CerasDeserialize | 48.120 ms | 1.0208 ms | 0.0560 ms |
