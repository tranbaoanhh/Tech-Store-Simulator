using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TechStoreSimulator;

public class PCAssemblyUI : MonoBehaviour
{
    public AssemblyController controller;

    private readonly List<CustomerOrder> _orderQueue = new List<CustomerOrder>();
    private readonly List<PCComponentData> _cpuCatalog = new List<PCComponentData>();
    private readonly List<PCComponentData> _mbCatalog = new List<PCComponentData>();
    private readonly List<PCComponentData> _ramCatalog = new List<PCComponentData>();
    private readonly List<PCComponentData> _gpuCatalog = new List<PCComponentData>();
    private readonly List<PCComponentData> _ssdCatalog = new List<PCComponentData>();

    private PCComponentData _selectedCase;
    private PCComponentData _selectedPsu;

    private int _orderIndex;
    private int _cpuIndex;
    private int _mbIndex;
    private int _ramIndex;
    private int _gpuIndex;
    private int _ssdIndex;

    private Text _orderReqText;
    private Text _cpuLabel;
    private Text _mbLabel;
    private Text _ramLabel;
    private Text _gpuLabel;
    private Text _ssdLabel;
    private Text _statsText;
    private Text _resultText;

    private void Start()
    {
        if (controller == null)
            controller = FindFirstObjectByType<AssemblyController>();

        InitOrders();
        InitCatalog();
        BuildFullUI();
        ApplyOrder(_orderIndex, syncOnly: true);
    }

    private void InitOrders()
    {
        _orderQueue.Clear();
        _orderQueue.Add(new CustomerOrder
        {
            customerName = "Le Hoang (Streamer)",
            budget = 1200,
            requiredPerformanceScore = 1600,
            requiredRamGB = 16,
            note = "PC Gaming 2K muot ma, dung socket LGA1700, du 16GB RAM"
        });
        _orderQueue.Add(new CustomerOrder
        {
            customerName = "Tran Minh (Sinh vien)",
            budget = 900,
            requiredPerformanceScore = 1500,
            requiredRamGB = 16,
            note = "May hoc tap va choi game nhe duoi $900"
        });
        _orderQueue.Add(new CustomerOrder
        {
            customerName = "Nguyen An (Editor)",
            budget = 1450,
            requiredPerformanceScore = 1900,
            requiredRamGB = 16,
            note = "Dung video 4K, hieu nang cao va tuong thich 100%"
        });
    }

    private void InitCatalog()
    {
        _selectedCase = new PCComponentData
        {
            id = "case_01",
            componentName = "NZXT H5 Flow Case",
            type = ComponentType.Case,
            price = 90,
            wattage = 0
        };

        _selectedPsu = new PCComponentData
        {
            id = "psu_01",
            componentName = "Corsair RM750e (750W)",
            type = ComponentType.PSU,
            price = 100,
            wattage = 750
        };

        _cpuCatalog.Add(new PCComponentData
        {
            id = "cpu_01",
            componentName = "Intel Core i5-13400F [LGA1700 | 600 perf | 65W] - $200",
            type = ComponentType.CPU,
            price = 200,
            performanceScore = 600,
            wattage = 65,
            cpuSocket = CpuSocket.LGA1700
        });
        _cpuCatalog.Add(new PCComponentData
        {
            id = "cpu_02",
            componentName = "AMD Ryzen 5 7600 [AM5 | 680 perf | 65W] - $230",
            type = ComponentType.CPU,
            price = 230,
            performanceScore = 680,
            wattage = 65,
            cpuSocket = CpuSocket.AM5
        });

        _mbCatalog.Add(new PCComponentData
        {
            id = "mb_01",
            componentName = "MSI B760M Pro [LGA1700 | DDR4 | 100 perf | 30W] - $140",
            type = ComponentType.Motherboard,
            price = 140,
            performanceScore = 100,
            wattage = 30,
            cpuSocket = CpuSocket.LGA1700,
            ramType = RamType.DDR4
        });
        _mbCatalog.Add(new PCComponentData
        {
            id = "mb_02",
            componentName = "ASUS B650M [AM5 | DDR5 | 120 perf | 35W] - $160",
            type = ComponentType.Motherboard,
            price = 160,
            performanceScore = 120,
            wattage = 35,
            cpuSocket = CpuSocket.AM5,
            ramType = RamType.DDR5
        });

        _ramCatalog.Add(new PCComponentData
        {
            id = "ram_01",
            componentName = "Kingston 16GB DDR4 [16GB | 150 perf | 10W] - $50",
            type = ComponentType.RAM,
            price = 50,
            performanceScore = 150,
            wattage = 10,
            ramType = RamType.DDR4,
            ramCapacityGB = 16
        });
        _ramCatalog.Add(new PCComponentData
        {
            id = "ram_02",
            componentName = "Corsair 16GB DDR5 [16GB | 180 perf | 12W] - $80",
            type = ComponentType.RAM,
            price = 80,
            performanceScore = 180,
            wattage = 12,
            ramType = RamType.DDR5,
            ramCapacityGB = 16
        });

        _gpuCatalog.Add(new PCComponentData
        {
            id = "gpu_01",
            componentName = "NVIDIA RTX 4060 Ti [800 perf | 160W] - $400",
            type = ComponentType.GPU,
            price = 400,
            performanceScore = 800,
            wattage = 160
        });
        _gpuCatalog.Add(new PCComponentData
        {
            id = "gpu_02",
            componentName = "RTX 4070 Ti Super [1200 perf | 285W] - $800",
            type = ComponentType.GPU,
            price = 800,
            performanceScore = 1200,
            wattage = 285
        });

        _ssdCatalog.Add(new PCComponentData
        {
            id = "ssd_01",
            componentName = "WD Black 1TB NVMe [100 perf | 5W] - $80",
            type = ComponentType.Storage,
            price = 80,
            performanceScore = 100,
            wattage = 5
        });
        _ssdCatalog.Add(new PCComponentData
        {
            id = "ssd_02",
            componentName = "Samsung 990 Pro 2TB [140 perf | 7W] - $150",
            type = ComponentType.Storage,
            price = 150,
            performanceScore = 140,
            wattage = 7
        });
    }

    private void ApplyOrder(int index, bool syncOnly)
    {
        if (controller == null || _orderQueue.Count == 0) return;

        _orderIndex = ((index % _orderQueue.Count) + _orderQueue.Count) % _orderQueue.Count;
        CustomerOrder src = _orderQueue[_orderIndex];

        controller.currentOrder = new CustomerOrder
        {
            customerName = src.customerName,
            budget = src.budget,
            requiredPerformanceScore = src.requiredPerformanceScore,
            requiredRamGB = src.requiredRamGB,
            note = src.note
        };

        _orderReqText.text =
            $"<b>Khach hang ({_orderIndex + 1}/{_orderQueue.Count}):</b> {src.customerName}\n" +
            $"• Ngan sach toi da: <b>${src.budget}</b>\n" +
            $"• Hieu nang toi thieu: <b>{src.requiredPerformanceScore}</b>\n" +
            $"• Dung luong RAM: <b>{src.requiredRamGB} GB</b>\n" +
            $"• Ghi chu: <i>{src.note}</i>";

        if (syncOnly)
        {
            SyncComponentsToController();
        }
    }

    private void SyncComponentsToController()
    {
        if (controller == null) return;

        controller.installedComponents = new List<PCComponentData>
        {
            _selectedCase,
            _cpuCatalog[_cpuIndex],
            _mbCatalog[_mbIndex],
            _ramCatalog[_ramIndex],
            _gpuCatalog[_gpuIndex],
            _ssdCatalog[_ssdIndex],
            _selectedPsu
        };

        _cpuLabel.text = "<b>CPU:</b> " + _cpuCatalog[_cpuIndex].componentName;
        _mbLabel.text = "<b>Mainboard:</b> " + _mbCatalog[_mbIndex].componentName;
        _ramLabel.text = "<b>RAM:</b> " + _ramCatalog[_ramIndex].componentName;
        _gpuLabel.text = "<b>GPU:</b> " + _gpuCatalog[_gpuIndex].componentName;
        _ssdLabel.text = "<b>SSD:</b> " + _ssdCatalog[_ssdIndex].componentName;

        PreviewStats();
    }

    private void PreviewStats()
    {
        if (controller == null || controller.currentOrder == null) return;

        AssemblyService service = new AssemblyService();
        AssemblyValidationResult res = service.ValidateBuild(controller.installedComponents, controller.currentOrder);

        string budgetColor = res.totalPrice <= controller.currentOrder.budget ? "#4ADE80" : "#F87171";
        string perfColor = res.totalPerformance >= controller.currentOrder.requiredPerformanceScore ? "#4ADE80" : "#FBBF24";
        string powerColor = res.totalPowerDraw <= res.psuCapacity ? "#4ADE80" : "#F87171";

        _statsText.text =
            $"<b>Tong chi phi:</b> <color={budgetColor}><b>${res.totalPrice}</b></color> / ${controller.currentOrder.budget}\n" +
            $"<b>Hieu nang:</b> <color={perfColor}><b>{res.totalPerformance}</b></color> / {controller.currentOrder.requiredPerformanceScore}\n" +
            $"<b>Cong suat:</b> <color={powerColor}><b>{res.totalPowerDraw}W</b></color> / {res.psuCapacity}W";
    }

    private void ValidateAndCompleteOrder()
    {
        if (controller == null) return;

        SyncComponentsToController();
        controller.CheckCurrentBuild();

        AssemblyService service = new AssemblyService();
        AssemblyValidationResult res = service.ValidateBuild(controller.installedComponents, controller.currentOrder);

        if (res.isValid)
        {
            _resultText.text =
                "<color=#4ADE80><b>[HOAN THANH DON HANG]</b></color>\n" +
                "Linh kien tuong thich day du, dat hieu nang va nam trong ngan sach.";
        }
        else
        {
            string errText = $"<color=#F87171><b>[LOI CAU HINH - {res.errors.Count} VAN DE]</b></color>\n";
            foreach (string err in res.errors)
            {
                errText += "• " + err + "\n";
            }
            _resultText.text = errText;
        }
    }

    private void BuildFullUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        GameObject canvasObj = new GameObject("PCAssemblyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Nen tong
        RectTransform root = CreatePanel(canvasObj.transform, Vector2.zero, Vector2.one, new Color(0.09f, 0.12f, 0.18f, 1f));

        // Thanh tieu de tren cung
        RectTransform topBar = CreatePanel(root, new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.97f), new Color(0.15f, 0.23f, 0.35f, 1f));
        CreateText(topBar, font, 28, TextAnchor.MiddleLeft, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f),
            "<b>[TV4] HE THONG LAP RAP PC THEO YEU CAU</b>  <size=20>(Case: NZXT H5 Flow + PSU: Corsair RM750e 750W)</size>");

        // COT TRAI: Chon linh kien CPU, Mainboard, RAM, GPU, SSD
        RectTransform leftCol = CreatePanel(root, new Vector2(0.02f, 0.04f), new Vector2(0.58f, 0.88f), new Color(0.13f, 0.17f, 0.24f, 1f));
        CreateText(leftCol, font, 25, TextAnchor.MiddleLeft, new Vector2(0.03f, 0.91f), new Vector2(0.97f, 0.98f), "<b>CHON LINH KIEN (CPU / MAINBOARD / RAM / GPU / SSD)</b>");

        _cpuLabel = CreatePartCard(leftCol, font, 0.73f, () => { _cpuIndex = (_cpuIndex + 1) % _cpuCatalog.Count; SyncComponentsToController(); });
        _mbLabel = CreatePartCard(leftCol, font, 0.55f, () => { _mbIndex = (_mbIndex + 1) % _mbCatalog.Count; SyncComponentsToController(); });
        _ramLabel = CreatePartCard(leftCol, font, 0.37f, () => { _ramIndex = (_ramIndex + 1) % _ramCatalog.Count; SyncComponentsToController(); });
        _gpuLabel = CreatePartCard(leftCol, font, 0.19f, () => { _gpuIndex = (_gpuIndex + 1) % _gpuCatalog.Count; SyncComponentsToController(); });
        _ssdLabel = CreatePartCard(leftCol, font, 0.01f, () => { _ssdIndex = (_ssdIndex + 1) % _ssdCatalog.Count; SyncComponentsToController(); });

        // COT PHAI: Yeu cau don hang + Thong so + Ket qua + Nut thao tac & Demo
        RectTransform rightCol = CreatePanel(root, new Vector2(0.60f, 0.04f), new Vector2(0.98f, 0.88f), new Color(0.13f, 0.17f, 0.24f, 1f));

        // Box 1: Yeu cau khach hang + Nut doi don
        RectTransform reqBox = CreatePanel(rightCol, new Vector2(0.03f, 0.68f), new Vector2(0.97f, 0.97f), new Color(0.18f, 0.24f, 0.34f, 1f));
        CreateText(reqBox, font, 22, TextAnchor.MiddleLeft, new Vector2(0.03f, 0.78f), new Vector2(0.66f, 0.96f), "<b>YEU CAU DON HANG KHACH</b>");
        CreateButton(reqBox, font, "DOI DON KHACH >>", new Color(0.26f, 0.35f, 0.50f, 1f),
            new Vector2(0.67f, 0.79f), new Vector2(0.97f, 0.95f), () =>
            {
                ApplyOrder(_orderIndex + 1, syncOnly: true);
                _resultText.text = "Da nap don khach hang moi. Hay chon linh kien va bam <b>KIEM TRA & HOAN THANH DON</b>.";
            });
        _orderReqText = CreateText(reqBox, font, 20, TextAnchor.UpperLeft, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.75f), "");

        // Box 2: Thong so tong hop
        RectTransform statBox = CreatePanel(rightCol, new Vector2(0.03f, 0.49f), new Vector2(0.97f, 0.66f), new Color(0.18f, 0.24f, 0.34f, 1f));
        _statsText = CreateText(statBox, font, 21, TextAnchor.MiddleLeft, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f), "");

        // Box 3: Ket qua kiem tra / Loi cau hinh
        RectTransform resBox = CreatePanel(rightCol, new Vector2(0.03f, 0.23f), new Vector2(0.97f, 0.47f), new Color(0.08f, 0.10f, 0.14f, 1f));
        _resultText = CreateText(resBox, font, 20, TextAnchor.UpperLeft, new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.94f),
            "Chon linh kien ben trai va bam <b>KIEM TRA & HOAN THANH DON</b>...");

        // Nut kiem tra & hoan thanh don
        CreateButton(rightCol, font, "KIEM TRA & HOAN THANH DON", new Color(0.15f, 0.60f, 0.35f, 1f),
            new Vector2(0.03f, 0.125f), new Vector2(0.97f, 0.21f), ValidateAndCompleteOrder);

        // 2 Nut Demo: Hop le & Loi
        CreateButton(rightCol, font, "DEMO 1: CAU HINH HOP LE", new Color(0.20f, 0.45f, 0.78f, 1f),
            new Vector2(0.03f, 0.02f), new Vector2(0.49f, 0.105f), () =>
            {
                _cpuIndex = 0; _mbIndex = 0; _ramIndex = 0; _gpuIndex = 0; _ssdIndex = 0;
                ApplyOrder(0, syncOnly: false);
                ValidateAndCompleteOrder();
            });

        CreateButton(rightCol, font, "DEMO 2: CAU HINH LOI", new Color(0.75f, 0.25f, 0.25f, 1f),
            new Vector2(0.51f, 0.02f), new Vector2(0.97f, 0.105f), () =>
            {
                _cpuIndex = 0; _mbIndex = 1; _ramIndex = 1; _gpuIndex = 1; _ssdIndex = 0;
                ApplyOrder(1, syncOnly: false);
                ValidateAndCompleteOrder();
            });
    }

    private Text CreatePartCard(Transform parent, Font font, float yMin, UnityEngine.Events.UnityAction onSwitch)
    {
        RectTransform card = CreatePanel(parent, new Vector2(0.03f, yMin), new Vector2(0.97f, yMin + 0.16f), new Color(0.19f, 0.25f, 0.35f, 1f));
        Text label = CreateText(card, font, 20, TextAnchor.MiddleLeft, new Vector2(0.03f, 0.06f), new Vector2(0.79f, 0.94f), "");
        CreateButton(card, font, "Doi >>", new Color(0.30f, 0.38f, 0.52f, 1f), new Vector2(0.81f, 0.18f), new Vector2(0.97f, 0.82f), onSwitch);
        return label;
    }

    private RectTransform CreatePanel(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject obj = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = color;
        return rt;
    }

    private Text CreateText(Transform parent, Font font, int fontSize, TextAnchor align, Vector2 anchorMin, Vector2 anchorMax, string content)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text txt = obj.GetComponent<Text>();
        txt.font = font;
        txt.fontSize = fontSize;
        txt.alignment = align;
        txt.color = Color.white;
        txt.supportRichText = true;
        txt.text = content;
        return txt;
    }

    private void CreateButton(Transform parent, Font font, string label, Color bgColor, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = CreatePanel(parent, anchorMin, anchorMax, bgColor);
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        CreateText(rt, font, 19, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, $"<b>{label}</b>");
    }
}
