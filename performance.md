``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.400
  [Host]   : .NET Core 5.0.9 (CoreCLR 5.0.921.35908, CoreFX 5.0.921.35908), X64 RyuJIT
  ShortRun : .NET Core 5.0.9 (CoreCLR 5.0.921.35908, CoreFX 5.0.921.35908), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.617 ms | 0.0509 ms | 0.0028 ms |
| USerializerDeserialize | 28.457 ms | 4.3448 ms | 0.2382 ms |
|   MessagePackSerialize |  9.563 ms | 1.3964 ms | 0.0765 ms |
| MessagePackDeserialize | 30.340 ms | 4.8918 ms | 0.2681 ms |
|         CerasSerialize | 57.826 ms | 2.3704 ms | 0.1299 ms |
|       CerasDeserialize | 54.577 ms | 2.3031 ms | 0.1262 ms |
