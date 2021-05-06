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
|   USerializerSerialize |  9.970 ms | 0.2290 ms | 0.0126 ms |
| USerializerDeserialize | 30.816 ms | 5.6774 ms | 0.3112 ms |
|   MessagePackSerialize |  9.582 ms | 0.0713 ms | 0.0039 ms |
| MessagePackDeserialize | 30.342 ms | 1.1017 ms | 0.0604 ms |
|         CerasSerialize | 56.622 ms | 1.3542 ms | 0.0742 ms |
|       CerasDeserialize | 54.063 ms | 0.6899 ms | 0.0378 ms |
