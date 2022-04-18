``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.407
  [Host]   : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT
  ShortRun : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  9.224 ms | 0.4532 ms | 0.0248 ms |
| USerializerDeserialize | 24.894 ms | 1.7214 ms | 0.0944 ms |
|   MessagePackSerialize |  9.422 ms | 0.5197 ms | 0.0285 ms |
| MessagePackDeserialize | 29.073 ms | 6.0710 ms | 0.3328 ms |
|         CerasSerialize | 58.520 ms | 0.8890 ms | 0.0487 ms |
|       CerasDeserialize | 53.473 ms | 4.0570 ms | 0.2224 ms |
