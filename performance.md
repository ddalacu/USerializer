``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.636 ms |  1.486 ms | 0.0815 ms |
| USerializerDeserialize | 25.083 ms |  6.972 ms | 0.3821 ms |
|   MessagePackSerialize |  8.973 ms |  2.069 ms | 0.1134 ms |
| MessagePackDeserialize | 27.932 ms |  3.456 ms | 0.1894 ms |
|         CerasSerialize | 55.576 ms | 10.097 ms | 0.5534 ms |
|       CerasDeserialize | 50.303 ms |  2.842 ms | 0.1558 ms |
