using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Parameters for affecting the tool's performance
/// </summary>
public class SubstancePerformanceParams
{
    public bool ProcessorUsageAllMenuToggle = true, ProcessorUsageHalfMenuToggle, ProcessorUsageOneMenuToggle, ProcessorUsageUnsupportedMenuToggle; // toggles for GUI
    public bool ProceduralCacheSizeNoLimitMenuToggle = true, ProceduralCacheSizeHeavyMenuToggle, ProceduralCacheSizeMediumMenuToggle, ProceduralCacheSizeTinyMenuToggle, ProceduralCacheSizeNoneMenuToggle;
    public enum MySubstanceProcessorUsage { Unsupported, One, Half, All }; // # of cores for processing textures
    public MySubstanceProcessorUsage mySubstanceProcessorUsage;
    public enum MyProceduralCacheSize { Medium = 0, Heavy = 1, None = 2, NoLimit = 3, Tiny = 4 }; // how much memory dedicated to the procedural material
    public MyProceduralCacheSize myProceduralCacheSize;

}
