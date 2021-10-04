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
|   USerializerSerialize |  5.869 ms | 0.0159 ms | 0.0009 ms |
| USerializerDeserialize | 24.327 ms | 0.0527 ms | 0.0029 ms |
|   MessagePackSerialize |  8.015 ms | 0.5874 ms | 0.0322 ms |
| MessagePackDeserialize | 26.230 ms | 1.6658 ms | 0.0913 ms |
|         CerasSerialize | 47.895 ms | 1.0870 ms | 0.0596 ms |
|       CerasDeserialize | 48.192 ms | 3.7823 ms | 0.2073 ms |
