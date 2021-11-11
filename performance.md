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
|   USerializerSerialize |  7.618 ms | 0.0705 ms | 0.0039 ms |
| USerializerDeserialize | 24.886 ms | 3.6273 ms | 0.1988 ms |
|   MessagePackSerialize |  7.905 ms | 0.1838 ms | 0.0101 ms |
| MessagePackDeserialize | 26.369 ms | 0.3149 ms | 0.0173 ms |
|         CerasSerialize | 49.310 ms | 1.9527 ms | 0.1070 ms |
|       CerasDeserialize | 47.964 ms | 1.0813 ms | 0.0593 ms |
