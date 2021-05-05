``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize | 10.949 ms |  9.001 ms | 0.4934 ms |
| USerializerDeserialize | 31.115 ms | 19.563 ms | 1.0723 ms |
|   MessagePackSerialize |  9.454 ms |  3.805 ms | 0.2086 ms |
| MessagePackDeserialize | 30.891 ms |  8.960 ms | 0.4911 ms |
|         CerasSerialize | 56.497 ms |  9.399 ms | 0.5152 ms |
|       CerasDeserialize | 55.631 ms |  6.411 ms | 0.3514 ms |
