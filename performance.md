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
|   USerializerSerialize |  6.104 ms | 0.0201 ms | 0.0011 ms |
| USerializerDeserialize | 24.619 ms | 3.2345 ms | 0.1773 ms |
|   MessagePackSerialize |  8.292 ms | 0.0822 ms | 0.0045 ms |
| MessagePackDeserialize | 26.441 ms | 0.1363 ms | 0.0075 ms |
|         CerasSerialize | 47.640 ms | 1.7315 ms | 0.0949 ms |
|       CerasDeserialize | 47.904 ms | 1.3783 ms | 0.0756 ms |
