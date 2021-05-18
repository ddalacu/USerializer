``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.203
  [Host]   : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  ShortRun : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.952 ms | 0.0293 ms | 0.0016 ms |
| USerializerDeserialize | 26.714 ms | 3.6795 ms | 0.2017 ms |
|   MessagePackSerialize |  9.542 ms | 0.1627 ms | 0.0089 ms |
| MessagePackDeserialize | 30.370 ms | 1.3679 ms | 0.0750 ms |
|         CerasSerialize | 58.973 ms | 0.4212 ms | 0.0231 ms |
|       CerasDeserialize | 55.603 ms | 0.6157 ms | 0.0337 ms |
