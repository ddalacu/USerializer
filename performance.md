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
|   USerializerSerialize |  5.568 ms | 0.0295 ms | 0.0016 ms |
| USerializerDeserialize | 21.606 ms | 1.3718 ms | 0.0752 ms |
|   MessagePackSerialize |  7.107 ms | 0.0366 ms | 0.0020 ms |
| MessagePackDeserialize | 23.325 ms | 1.7770 ms | 0.0974 ms |
|         CerasSerialize | 45.270 ms | 1.7706 ms | 0.0971 ms |
|       CerasDeserialize | 42.978 ms | 1.7163 ms | 0.0941 ms |
