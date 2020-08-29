``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.401
  [Host]     : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT


```
|                  Method |           Mean |        Error |       StdDev |    Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------ |---------------:|-------------:|-------------:|---------:|-------:|------:|----------:|
| InvokeMarkdownProcessor |       239.9 ns |      2.33 ns |      1.94 ns |   0.1450 |      - |     - |     608 B |
|          InvokePipeline | 1,959,246.0 ns | 39,108.35 ns | 56,088.03 ns | 175.7813 | 3.9063 |     - |  761570 B |
