``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.793 ms |  2.176 ms | 0.1193 ms |
| USerializerDeserialize | 27.456 ms | 10.702 ms | 0.5866 ms |
|   MessagePackSerialize |  9.414 ms | 10.239 ms | 0.5612 ms |
| MessagePackDeserialize | 27.662 ms | 18.536 ms | 1.0160 ms |
|         CerasSerialize | 50.478 ms | 22.653 ms | 1.2417 ms |
|       CerasDeserialize | 55.915 ms | 73.890 ms | 4.0502 ms |
