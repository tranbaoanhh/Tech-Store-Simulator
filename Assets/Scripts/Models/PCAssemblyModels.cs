using System;
using System.Collections.Generic;
using UnityEngine;

public enum ComponentType
{
    CPU,
    Motherboard,
    RAM,
    GPU,
    Storage,
    PSU,
    Case
}

public enum CpuSocket
{
    LGA1700,
    AM4,
    AM5
}

public enum RamType
{
    DDR4,
    DDR5
}

[Serializable]
public class PCComponentData
{
    public string id;
    public string componentName;
    public ComponentType type;
    public int price;
    public int performanceScore;
    public int wattage;

    [Header("Thong so tuong thich (Chi dien theo loai linh kien)")]
    public CpuSocket cpuSocket;
    public RamType ramType;
    public int ramCapacityGB;
}

[Serializable]
public class CustomerOrder
{
    public string customerName;
    public int budget;
    public int requiredPerformanceScore;
    public int requiredRamGB;
    public string note;
}

public class AssemblyValidationResult
{
    public bool isValid;
    public int totalPrice;
    public int totalPerformance;
    public int totalPowerDraw;
    public int psuCapacity;
    public List<string> errors = new List<string>();
}