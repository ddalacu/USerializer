``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  8.113 ms |  0.7164 ms | 0.0393 ms |
| USerializerDeserialize | 28.039 ms |  2.0027 ms | 0.1098 ms |
|   MessagePackSerialize |  9.425 ms |  6.9360 ms | 0.3802 ms |
| MessagePackDeserialize | 30.987 ms |  6.8075 ms | 0.3731 ms |
|         CerasSerialize | 61.073 ms | 59.8331 ms | 3.2797 ms |
|       CerasDeserialize | 56.191 ms | 19.0022 ms | 1.0416 ms |
