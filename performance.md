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
|   USerializerSerialize |  9.432 ms | 1.1900 ms | 0.0652 ms |
| USerializerDeserialize | 31.392 ms | 2.6743 ms | 0.1466 ms |
|   MessagePackSerialize |  9.363 ms | 0.3351 ms | 0.0184 ms |
| MessagePackDeserialize | 30.130 ms | 3.3554 ms | 0.1839 ms |
|         CerasSerialize | 57.865 ms | 1.6609 ms | 0.0910 ms |
|       CerasDeserialize | 56.195 ms | 7.2109 ms | 0.3953 ms |
