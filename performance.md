``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.403
  [Host]   : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT
  ShortRun : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  9.000 ms | 0.0823 ms | 0.0045 ms |
| USerializerDeserialize | 27.718 ms | 4.1975 ms | 0.2301 ms |
|   MessagePackSerialize | 10.263 ms | 6.2658 ms | 0.3434 ms |
| MessagePackDeserialize | 32.983 ms | 8.0020 ms | 0.4386 ms |
|         CerasSerialize | 57.494 ms | 0.4289 ms | 0.0235 ms |
|       CerasDeserialize | 56.507 ms | 0.3282 ms | 0.0180 ms |
