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
|   USerializerSerialize |  7.174 ms |  0.1268 ms | 0.0070 ms |
| USerializerDeserialize | 22.627 ms |  2.2888 ms | 0.1255 ms |
|   MessagePackSerialize |  8.035 ms |  0.0911 ms | 0.0050 ms |
| MessagePackDeserialize | 22.964 ms |  2.7898 ms | 0.1529 ms |
|         CerasSerialize | 43.258 ms |  2.4400 ms | 0.1337 ms |
|       CerasDeserialize | 41.112 ms | 15.5086 ms | 0.8501 ms |
