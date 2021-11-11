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
|   USerializerSerialize |  8.399 ms | 0.2574 ms | 0.0141 ms |
| USerializerDeserialize | 28.613 ms | 4.4369 ms | 0.2432 ms |
|   MessagePackSerialize |  9.409 ms | 0.1079 ms | 0.0059 ms |
| MessagePackDeserialize | 31.703 ms | 0.6845 ms | 0.0375 ms |
|         CerasSerialize | 56.951 ms | 3.4236 ms | 0.1877 ms |
|       CerasDeserialize | 55.939 ms | 0.6956 ms | 0.0381 ms |
