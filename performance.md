``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.203
  [Host]   : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  ShortRun : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.614 ms |  2.469 ms | 0.1354 ms |
| USerializerDeserialize | 22.378 ms |  7.475 ms | 0.4097 ms |
|   MessagePackSerialize |  8.044 ms |  1.860 ms | 0.1020 ms |
| MessagePackDeserialize | 24.806 ms |  1.060 ms | 0.0581 ms |
|         CerasSerialize | 48.502 ms | 25.661 ms | 1.4066 ms |
|       CerasDeserialize | 44.360 ms | 19.772 ms | 1.0837 ms |
