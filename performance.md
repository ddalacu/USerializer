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
|   USerializerSerialize |  8.033 ms | 1.2486 ms | 0.0684 ms |
| USerializerDeserialize | 26.857 ms | 2.8761 ms | 0.1577 ms |
|   MessagePackSerialize | 10.034 ms | 8.8757 ms | 0.4865 ms |
| MessagePackDeserialize | 30.144 ms | 1.4213 ms | 0.0779 ms |
|         CerasSerialize | 59.150 ms | 1.1351 ms | 0.0622 ms |
|       CerasDeserialize | 56.548 ms | 0.2836 ms | 0.0155 ms |
