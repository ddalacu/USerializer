``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |    Error |    StdDev |
|----------------------- |----------:|---------:|----------:|
|   USerializerSerialize |  7.382 ms | 1.129 ms | 0.0619 ms |
| USerializerDeserialize | 24.845 ms | 2.943 ms | 0.1613 ms |
|   MessagePackSerialize |  9.327 ms | 3.477 ms | 0.1906 ms |
| MessagePackDeserialize | 27.903 ms | 3.541 ms | 0.1941 ms |
|         CerasSerialize | 53.049 ms | 9.224 ms | 0.5056 ms |
|       CerasDeserialize | 52.798 ms | 9.638 ms | 0.5283 ms |
