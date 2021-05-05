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
|   USerializerSerialize |  9.877 ms | 1.5964 ms | 0.0875 ms |
| USerializerDeserialize | 30.007 ms | 9.8475 ms | 0.5398 ms |
|   MessagePackSerialize |  9.499 ms | 8.0338 ms | 0.4404 ms |
| MessagePackDeserialize | 29.739 ms | 0.3997 ms | 0.0219 ms |
|         CerasSerialize | 55.742 ms | 1.6160 ms | 0.0886 ms |
|       CerasDeserialize | 55.578 ms | 6.2395 ms | 0.3420 ms |
