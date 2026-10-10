# 级联渲染性能数据（CssCascadeBenchmarks / InnerTextBenchmarks）

规则分桶索引（关键选择器分桶 + 文档级样式集合缓存）改造前 / 后的对比数据。

- 运行日期：2026-10-10
- 目标框架：net10.0
- 构建配置：Release
- 测试框架：BenchmarkDotNet 0.15.8（DefaultJob + MemoryDiagnoser，InnerText 为 MediumRunJob）
- 运行环境：Windows 10.0.26200, x64

## 一、CssCascadeBenchmarks

| Method | 改造前 | 改造后 | 加速比 | 分配（前 → 后） |
|---|---:|---:|---:|---|
| ComputedStyle | 9.192 ms | 5.076 ms | 1.81x | 2.64 MB → 1.43 MB |
| RenderTree | 77.188 ms | 53.524 ms | 1.44x | 18.4 MB → 19 MB |
| ParseInlineDeclarations | 1.316 ms | 1.358 ms | 1.00x | 1.78 MB → 1.78 MB |

改造后明细：

| Method                  | Mean      | Error     | StdDev    | Gen0      | Gen1     | Gen2    | Allocated |
|------------------------ |----------:|----------:|----------:|----------:|---------:|--------:|----------:|
| ComputedStyle           |  5.076 ms | 0.0636 ms | 0.0595 ms |  171.8750 |        - |       - |   1.43 MB |
| RenderTree              | 53.524 ms | 1.0666 ms | 1.2283 ms | 2400.0000 | 600.0000 | 100.0000 |     19 MB |
| ParseInlineDeclarations |  1.358 ms | 0.0253 ms | 0.0236 ms |  220.7031 |   5.8594 |       - |   1.78 MB |

- `ComputedStyle`：对 25 个采样元素执行 `getComputedStyle` 的合计耗时与分配。
- `RenderTree`：`IWindow.Render()` 构建完整渲染树的耗时与分配。
- `ParseInlineDeclarations`：解析 200 条内联样式声明（10 组 × 20 模板）的合计耗时与分配。
- `ParseInlineDeclarations` 与解析路径无关，差异在噪声范围内。

## 二、InnerTextBenchmarks

| Method | 改造前 | 改造后 | 加速比 |
|---|---:|---:|---:|
| GetInnerText_RealWorld_Length | 125,872.3 us | 119,352.4 us | 1.05x |
| GetInnerText_SelectorHeavy_Length | 11,351,408.5 us | 1,631,595.5 us | 6.96x |
| Experimental_RealWorld_Length | 717.9 us | 640.2 us | — |
| Experimental_SelectorHeavy_Length | 24,462.0 us | 15,691.9 us | — |

改造后明细：

| Method                            | Mean           | Error        | StdDev       | Ratio  | RatioSD | Allocated    |
|---------------------------------- |---------------:|-------------:|-------------:|-------:|--------:|-------------:|
| GetInnerText_RealWorld_Length     |   119,352.4 us |  2,605.62 us |  3,736.90 us |  1.001 |    0.04 |  44113.46 KB |
| GetInnerText_SelectorHeavy_Length | 1,631,595.5 us | 24,501.75 us | 35,914.35 us | 13.683 |    0.51 | 405509.84 KB |
| Experimental_RealWorld_Length     |       640.2 us |     12.64 us |     18.93 us |  0.005 |    0.00 |    596.27 KB |
| Experimental_SelectorHeavy_Length |    15,691.9 us |    469.66 us |    702.97 us |  0.132 |    0.01 |      8394 KB |

- `Experimental_*` 是夹具里的简化算法，只用于对照量级，不覆盖完整 innerText 规范，不能等价替换。
- 与简化算法的差距由 90.3x 降到 13.7x（选择器密集场景）/ 由 175x 降到 186x（真实场景，无变化）。

## 三、结论与后续

- 选择器密集场景的收益来自剪枝本身：候选规则从 513 条降到平均 15 条。
- `ComputedStyle` / `RenderTree` / `InnerText_RealWorld` 的剩余开销主要不在规则匹配，而在每个元素的 `PrepareComputedDeclarations(...).Compute(...)`（逐属性计算全部声明）。这部分本次未改动。
- 后续若仍需要，可评估「按 innerText 实际用到的属性短路计算」，但那会改变计算语义，需单独确认。

## 四、图例

- `Mean`：所有测量值的算术平均。
- `Error`：99.9% 置信区间的一半。
- `StdDev`：所有测量值的标准差。
- `Ratio`：相对同一组基线方法的比值分布均值。
- `Gen0` / `Gen1` / `Gen2`：每 1000 次操作触发第 0 / 1 / 2 代 GC 的次数。
- `Allocated`：单次操作分配的内存（仅托管内存，含子调用，1 KB = 1024 B）。
