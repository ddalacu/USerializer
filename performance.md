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
|   USerializerSerialize |  7.763 ms | 0.0229 ms | 0.0013 ms |
| USerializerDeserialize | 23.831 ms | 3.4240 ms | 0.1877 ms |
|   MessagePackSerialize |  8.013 ms | 0.0488 ms | 0.0027 ms |
| MessagePackDeserialize | 25.441 ms | 2.1308 ms | 0.1168 ms |
|         CerasSerialize | 47.786 ms | 1.4233 ms | 0.0780 ms |
|       CerasDeserialize | 46.929 ms | 2.0167 ms | 0.1105 ms |
