``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 10.77 ms |  4.943 ms | 0.271 ms |
| USerializerDeserialize | 33.13 ms | 15.114 ms | 0.828 ms |
|   MessagePackSerialize | 10.04 ms |  2.055 ms | 0.113 ms |
| MessagePackDeserialize | 33.39 ms |  9.603 ms | 0.526 ms |
|         CerasSerialize | 58.79 ms | 12.906 ms | 0.707 ms |
|       CerasDeserialize | 61.14 ms | 11.994 ms | 0.657 ms |
