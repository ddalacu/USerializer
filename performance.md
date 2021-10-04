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
|   USerializerSerialize |  5.965 ms | 0.1089 ms | 0.0060 ms |
| USerializerDeserialize | 23.998 ms | 2.3025 ms | 0.1262 ms |
|   MessagePackSerialize |  8.247 ms | 0.0543 ms | 0.0030 ms |
| MessagePackDeserialize | 26.046 ms | 2.4335 ms | 0.1334 ms |
|         CerasSerialize | 48.213 ms | 2.4674 ms | 0.1352 ms |
|       CerasDeserialize | 47.538 ms | 3.2937 ms | 0.1805 ms |
