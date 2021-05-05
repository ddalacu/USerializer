``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.050 ms | 0.0844 ms | 0.0046 ms |
| USerializerDeserialize | 26.120 ms | 3.2857 ms | 0.1801 ms |
|   MessagePackSerialize |  7.891 ms | 0.2715 ms | 0.0149 ms |
| MessagePackDeserialize | 25.805 ms | 1.6311 ms | 0.0894 ms |
|         CerasSerialize | 48.350 ms | 1.4095 ms | 0.0773 ms |
|       CerasDeserialize | 46.397 ms | 2.2271 ms | 0.1221 ms |
