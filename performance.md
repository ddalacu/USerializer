``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 22.04
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=7.0.102
  [Host]   : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT
  ShortRun : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.478 ms | 0.1011 ms | 0.0055 ms |
| USerializerDeserialize | 23.157 ms | 2.5028 ms | 0.1372 ms |
|   MessagePackSerialize |  6.332 ms | 0.0135 ms | 0.0007 ms |
| MessagePackDeserialize | 21.167 ms | 6.5606 ms | 0.3596 ms |
|         CerasSerialize | 47.747 ms | 4.6799 ms | 0.2565 ms |
|       CerasDeserialize | 43.002 ms | 4.5968 ms | 0.2520 ms |
