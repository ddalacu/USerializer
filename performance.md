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
|   USerializerSerialize | 10.249 ms |  2.939 ms | 0.1611 ms |
| USerializerDeserialize | 31.758 ms |  7.074 ms | 0.3878 ms |
|   MessagePackSerialize |  9.764 ms |  2.166 ms | 0.1187 ms |
| MessagePackDeserialize | 31.585 ms |  4.936 ms | 0.2706 ms |
|         CerasSerialize | 57.809 ms | 18.142 ms | 0.9944 ms |
|       CerasDeserialize | 58.821 ms |  6.750 ms | 0.3700 ms |
