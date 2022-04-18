``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.407
  [Host]   : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT
  ShortRun : .NET Core 5.0.16 (CoreCLR 5.0.1622.16705, CoreFX 5.0.1622.16705), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  7.569 ms | 0.0460 ms | 0.0025 ms |
| USerializerDeserialize | 20.795 ms | 2.9748 ms | 0.1631 ms |
|   MessagePackSerialize |  7.973 ms | 0.0648 ms | 0.0036 ms |
| MessagePackDeserialize | 26.256 ms | 0.0844 ms | 0.0046 ms |
|         CerasSerialize | 44.419 ms | 1.2920 ms | 0.0708 ms |
|       CerasDeserialize | 40.846 ms | 3.8990 ms | 0.2137 ms |
