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
|   USerializerSerialize |  5.946 ms | 0.0340 ms | 0.0019 ms |
| USerializerDeserialize | 23.892 ms | 3.5570 ms | 0.1950 ms |
|   MessagePackSerialize |  8.087 ms | 0.0134 ms | 0.0007 ms |
| MessagePackDeserialize | 25.071 ms | 3.3604 ms | 0.1842 ms |
|         CerasSerialize | 47.364 ms | 4.2980 ms | 0.2356 ms |
|       CerasDeserialize | 45.924 ms | 6.2344 ms | 0.3417 ms |
