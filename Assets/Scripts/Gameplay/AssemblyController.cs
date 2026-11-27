using System.Collections.Generic;
using UnityEngine;
using TechStoreSimulator;

public class AssemblyController : MonoBehaviour
{
    [Header("Don dat hang hien tai")]
    public CustomerOrder currentOrder;

    [Header("Danh sach linh kien dang lap trong may")]
    public List<PCComponentData> installedComponents = new List<PCComponentData>();

    private readonly AssemblyService _assemblyService = new AssemblyService();

    private void Start()
    {
        if (currentOrder == null || string.IsNullOrEmpty(currentOrder.customerName))
        {
            SetupValidDemoOrder();
        }
    }

    [ContextMenu("Test: Kiem tra cau hinh hien tai")]
    public void CheckCurrentBuild()
    {
        AssemblyValidationResult result = _assemblyService.ValidateBuild(installedComponents, currentOrder);

        Debug.Log($"=== KIEM TRA DON HANG CUA: {currentOrder.customerName} ===");
        Debug.Log($"Tong tien: ${result.totalPrice} / Ngan sach: ${currentOrder.budget} | Hieu nang: {result.totalPerformance}/{currentOrder.requiredPerformanceScore} | Cong suat: {result.totalPowerDraw}W/{result.psuCapacity}W");

        if (result.isValid)
        {
            Debug.Log("<color=green><b>[THANH CONG]</b> May hoat dong tot va dat moi yeu cau cua khach.</color>");
        }
        else
        {
            Debug.LogWarning($"<color=red><b>[LOI LAP RAP]</b> Phat hien {result.errors.Count} van de:</color>");
            foreach (string err in result.errors)
            {
                Debug.LogError(" - " + err);
            }
        }
    }

    [ContextMenu("Demo 1: Nap cau hinh HOP LE va chay thu")]
    public void RunValidBuildDemo()
    {
        SetupValidDemoOrder();
        CheckCurrentBuild();
    }

    [ContextMenu("Demo 2: Nap cau hinh LOI (Sai Socket & Vuot ngan sach)")]
    public void RunInvalidBuildDemo()
    {
        currentOrder = new CustomerOrder
        {
            customerName = "Tran Minh (Sinh vien)",
            budget = 900,
            requiredPerformanceScore = 1500,
            requiredRamGB = 16,
            note = "Can may hoc tap va choi game nhe duoi $900"
        };

        installedComponents = new List<PCComponentData>
        {
            new PCComponentData { id = "case_01", componentName = "Vo Case NZXT H5", type = ComponentType.Case, price = 80, wattage = 0 },
            new PCComponentData { id = "cpu_01", componentName = "Intel Core i5-13400F", type = ComponentType.CPU, price = 200, performanceScore = 600, wattage = 65, cpuSocket = CpuSocket.LGA1700 },
            new PCComponentData { id = "mb_01", componentName = "ASUS B650M (AM5)", type = ComponentType.Motherboard, price = 150, performanceScore = 100, wattage = 30, cpuSocket = CpuSocket.AM5, ramType = RamType.DDR5 },
            new PCComponentData { id = "ram_01", componentName = "Corsair 16GB DDR5", type = ComponentType.RAM, price = 70, performanceScore = 150, wattage = 10, ramType = RamType.DDR5, ramCapacityGB = 16 },
            new PCComponentData { id = "gpu_01", componentName = "RTX 4070", type = ComponentType.GPU, price = 550, performanceScore = 900, wattage = 200 },
            new PCComponentData { id = "ssd_01", componentName = "Samsung 1TB NVMe", type = ComponentType.Storage, price = 80, performanceScore = 100, wattage = 5 },
            new PCComponentData { id = "psu_01", componentName = "Cooler Master 650W", type = ComponentType.PSU, price = 70, wattage = 650 }
        };

        CheckCurrentBuild();
    }

    private void SetupValidDemoOrder()
    {
        currentOrder = new CustomerOrder
        {
            customerName = "Le Hoang (Streamer)",
            budget = 1200,
            requiredPerformanceScore = 1600,
            requiredRamGB = 16,
            note = "PC Gaming 2K muot ma"
        };

        installedComponents = new List<PCComponentData>
        {
            new PCComponentData { id = "case_01", componentName = "Vo Case Lian Li", type = ComponentType.Case, price = 90, wattage = 0 },
            new PCComponentData { id = "cpu_01", componentName = "Intel Core i5-13400F", type = ComponentType.CPU, price = 200, performanceScore = 600, wattage = 65, cpuSocket = CpuSocket.LGA1700 },
            new PCComponentData { id = "mb_01", componentName = "MSI B760M Pro (LGA1700)", type = ComponentType.Motherboard, price = 140, performanceScore = 100, wattage = 30, cpuSocket = CpuSocket.LGA1700, ramType = RamType.DDR4 },
            new PCComponentData { id = "ram_01", componentName = "Kingston Fury 16GB DDR4", type = ComponentType.RAM, price = 50, performanceScore = 150, wattage = 10, ramType = RamType.DDR4, ramCapacityGB = 16 },
            new PCComponentData { id = "gpu_01", componentName = "NVIDIA RTX 4060 Ti", type = ComponentType.GPU, price = 400, performanceScore = 800, wattage = 160 },
            new PCComponentData { id = "ssd_01", componentName = "WD Black 1TB NVMe", type = ComponentType.Storage, price = 80, performanceScore = 100, wattage = 5 },
            new PCComponentData { id = "psu_01", componentName = "Corsair RM750e (750W)", type = ComponentType.PSU, price = 100, wattage = 750 }
        };
    }
}
