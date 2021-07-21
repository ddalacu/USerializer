``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.302
  [Host]   : .NET Core 5.0.8 (CoreCLR 5.0.821.31504, CoreFX 5.0.821.31504), X64 RyuJIT
  ShortRun : .NET Core 5.0.8 (CoreCLR 5.0.821.31504, CoreFX 5.0.821.31504), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.533 ms |  1.406 ms | 0.0771 ms |
| USerializerDeserialize | 25.232 ms | 11.274 ms | 0.6180 ms |
|   MessagePackSerialize |  8.894 ms |  2.916 ms | 0.1598 ms |
| MessagePackDeserialize | 27.181 ms |  8.238 ms | 0.4515 ms |
|         CerasSerialize | 51.822 ms | 20.975 ms | 1.1497 ms |
|       CerasDeserialize | 52.231 ms |  7.643 ms | 0.4190 ms |
