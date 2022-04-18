``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.407
  [Host]   : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT
  ShortRun : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  7.749 ms |  0.0664 ms | 0.0036 ms |
| USerializerDeserialize | 22.638 ms |  0.9080 ms | 0.0498 ms |
|   MessagePackSerialize |  7.754 ms | 10.9520 ms | 0.6003 ms |
| MessagePackDeserialize | 23.433 ms |  0.2985 ms | 0.0164 ms |
|         CerasSerialize | 49.355 ms |  2.1187 ms | 0.1161 ms |
|       CerasDeserialize | 45.211 ms |  0.4090 ms | 0.0224 ms |
