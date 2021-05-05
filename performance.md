``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |   StdDev |
|----------------------- |----------:|----------:|---------:|
|   USerializerSerialize |  23.41 ms |  2.824 ms | 0.155 ms |
| USerializerDeserialize |  72.41 ms | 45.987 ms | 2.521 ms |
|   MessagePackSerialize |  21.47 ms |  1.757 ms | 0.096 ms |
| MessagePackDeserialize |  69.02 ms | 10.471 ms | 0.574 ms |
|         CerasSerialize | 131.18 ms | 18.195 ms | 0.997 ms |
|       CerasDeserialize | 131.47 ms | 28.968 ms | 1.588 ms |
