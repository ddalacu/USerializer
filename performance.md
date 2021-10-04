``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.988 ms | 0.2942 ms | 0.0161 ms |
| USerializerDeserialize | 28.572 ms | 4.4121 ms | 0.2418 ms |
|   MessagePackSerialize |  9.939 ms | 8.7909 ms | 0.4819 ms |
| MessagePackDeserialize | 30.525 ms | 4.8068 ms | 0.2635 ms |
|         CerasSerialize | 57.712 ms | 5.8137 ms | 0.3187 ms |
|       CerasDeserialize | 54.636 ms | 1.8504 ms | 0.1014 ms |
