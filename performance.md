``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.852 ms |  4.293 ms | 0.2353 ms |
| USerializerDeserialize | 27.392 ms | 13.276 ms | 0.7277 ms |
|   MessagePackSerialize |  9.775 ms |  7.674 ms | 0.4206 ms |
| MessagePackDeserialize | 31.270 ms |  9.493 ms | 0.5203 ms |
|         CerasSerialize | 56.169 ms | 22.056 ms | 1.2090 ms |
|       CerasDeserialize | 54.384 ms | 15.880 ms | 0.8704 ms |
