using System.Collections.Generic;
using System.Linq;

namespace TechStoreSimulator
{
    public class AssemblyService
    {
        public AssemblyValidationResult ValidateBuild(List<PCComponentData> installedParts, CustomerOrder order)
        {
            var result = new AssemblyValidationResult();

            if (installedParts == null || installedParts.Count == 0)
            {
                result.isValid = false;
                result.errors.Add("Chua lap bat ky linh kien nao vao thung may.");
                return result;
            }

            var cpu = installedParts.FirstOrDefault(p => p.type == ComponentType.CPU);
            var mobo = installedParts.FirstOrDefault(p => p.type == ComponentType.Motherboard);
            var gpu = installedParts.FirstOrDefault(p => p.type == ComponentType.GPU);
            var storage = installedParts.FirstOrDefault(p => p.type == ComponentType.Storage);
            var psu = installedParts.FirstOrDefault(p => p.type == ComponentType.PSU);
            var pcCase = installedParts.FirstOrDefault(p => p.type == ComponentType.Case);
            var rams = installedParts.Where(p => p.type == ComponentType.RAM).ToList();

            if (pcCase == null) result.errors.Add("Thieu Vo may (Case).");
            if (mobo == null) result.errors.Add("Thieu Bo mach chu (Motherboard).");
            if (cpu == null) result.errors.Add("Thieu Vi xu ly (CPU).");
            if (rams.Count == 0) result.errors.Add("Thieu Bo nho trong (RAM).");
            if (gpu == null) result.errors.Add("Thieu Card do hoa (GPU).");
            if (storage == null) result.errors.Add("Thieu O cung (Storage).");
            if (psu == null) result.errors.Add("Thieu Bo nguon (PSU).");

            if (cpu != null && mobo != null && cpu.cpuSocket != mobo.cpuSocket)
            {
                result.errors.Add($"Lech Socket: CPU ({cpu.cpuSocket}) khong lap vua Mainboard ({mobo.cpuSocket}).");
            }

            if (mobo != null && rams.Count > 0)
            {
                foreach (var ram in rams)
                {
                    if (ram.ramType != mobo.ramType)
                    {
                        result.errors.Add($"Sai chuan RAM: Thanh {ram.componentName} ({ram.ramType}) khong tuong thich Mainboard ({mobo.ramType}).");
                    }
                }
            }

            result.totalPrice = installedParts.Sum(p => p.price);
            result.totalPerformance = installedParts.Sum(p => p.performanceScore);
            int totalRamGB = rams.Sum(r => r.ramCapacityGB);

            result.psuCapacity = psu != null ? psu.wattage : 0;
            result.totalPowerDraw = installedParts
                .Where(p => p.type != ComponentType.PSU)
                .Sum(p => p.wattage);

            if (psu != null && result.totalPowerDraw > result.psuCapacity)
            {
                result.errors.Add($"Qua tai nguon: He thong tieu thu {result.totalPowerDraw}W nhung PSU chi dap ung {result.psuCapacity}W.");
            }

            if (order != null)
            {
                if (result.totalPrice > order.budget)
                {
                    result.errors.Add($"Vuot ngan sach: Tong chi phi ${result.totalPrice} vuot muc toi da ${order.budget} cua khach.");
                }
                if (result.totalPerformance < order.requiredPerformanceScore)
                {
                    result.errors.Add($"Chua dat hieu nang: Diem cau hinh ({result.totalPerformance}) thap hon yeu cau ({order.requiredPerformanceScore}).");
                }
                if (totalRamGB < order.requiredRamGB)
                {
                    result.errors.Add($"Thieu dung luong RAM: Hien co {totalRamGB}GB, khach yeu cau toi thieu {order.requiredRamGB}GB.");
                }
            }

            result.isValid = result.errors.Count == 0;
            return result;
        }
    }
}
