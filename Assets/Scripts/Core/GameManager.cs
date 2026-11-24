using System;
using System.Collections;
using UnityEngine;

namespace TechStoreSimulator
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Prototype balance; FinanceService records store sales separately.
        [SerializeField, Min(0)] int startingMoney = 10000000;
        [SerializeField] bool showPrototypeHUD = true;
        [SerializeField, Min(1)] int demoCustomerCount = 5;
        [SerializeField, Min(0f)] float timeBetweenCustomers = 3f;

        public int Money { get; private set; }
        public int ItemsPurchased { get; private set; }
        public string Status => status;
        public ProductService Products { get; private set; }
        public FinanceService Finance { get; private set; }
        public SalesService Sales { get; private set; }

        public event Action<int> MoneyChanged;
        public event Action<int> ItemsPurchasedChanged;
        public event Action<string> StatusChanged;

        const string InitialStatus = "Cham E vao san pham de mua.";
        string status = InitialStatus;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyState(Mathf.Max(0, startingMoney), 0, InitialStatus);
            Products = new ProductService();
            Finance = new FinanceService();
            Sales = new SalesService(Products, Finance);
        }

        void Start()
        {
            // Keep existing scenes playable even if they were not regenerated with the demo builder.
            if (FindObjectOfType<SalesUI>() == null) gameObject.AddComponent<SalesUI>();
            StartCoroutine(RunDemoCustomers());
        }

        IEnumerator RunDemoCustomers()
        {
            int created = FindObjectOfType<CustomerAI>() != null ? 1 : 0;
            while (created < demoCustomerCount)
            {
                yield return new WaitUntil(() => FindObjectOfType<CustomerAI>() == null);
                if (created > 0) yield return new WaitForSeconds(timeBetweenCustomers);
                GameObject customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                customer.name = "Demo Customer " + (created + 1);
                customer.transform.position = new Vector3(2.5f, 1f, -4f);
                customer.AddComponent<CustomerAI>();
                created++;
            }
        }

        void OnValidate()
        {
            startingMoney = Mathf.Max(0, startingMoney);
            demoCustomerCount = Mathf.Max(1, demoCustomerCount);
            timeBetweenCustomers = Mathf.Max(0f, timeBetweenCustomers);
        }

        void OnDestroy()
        {
            // Destroying a duplicate must not clear the active singleton.
            if (Instance == this) Instance = null;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                ApplyState(Money, ItemsPurchased, "Gia mua phai lon hon 0!");
                return false;
            }
            if (Money < amount)
            {
                ApplyState(Money, ItemsPurchased, "Khong du tien!");
                return false;
            }
            if (ItemsPurchased == int.MaxValue)
            {
                ApplyState(Money, ItemsPurchased, "Da dat gioi han so luot mua!");
                return false;
            }

            ApplyState(Money - amount, ItemsPurchased + 1, "Mua thanh cong!");
            return true;
        }

        /// <summary>Restores a validated balance and purchase count without doing file I/O.</summary>
        public bool TryRestoreState(int money, int itemsPurchased)
        {
            if (money < 0 || itemsPurchased < 0)
            {
                ApplyState(Money, ItemsPurchased, "Du lieu khoi phuc khong hop le!");
                return false;
            }

            ApplyState(money, itemsPurchased, "Da khoi phuc tien va so luot mua.");
            return true;
        }

        void ApplyState(int money, int itemsPurchased, string newStatus)
        {
            bool moneyChanged = Money != money;
            bool purchasesChanged = ItemsPurchased != itemsPurchased;
            bool statusChanged = status != newStatus;

            // Update all values before notifying listeners so they read a consistent state.
            Money = money;
            ItemsPurchased = itemsPurchased;
            status = newStatus;

            if (moneyChanged) MoneyChanged?.Invoke(money);
            if (purchasesChanged) ItemsPurchasedChanged?.Invoke(itemsPurchased);
            if (statusChanged) StatusChanged?.Invoke(newStatus);
        }

        public void RecordStoreSale(int amount)
        {
            if (amount <= 0 || Money > int.MaxValue - amount) return;
            ApplyState(Money + amount, ItemsPurchased, status);
        }

        public void SetStatus(string message)
        {
            ApplyState(Money, ItemsPurchased, message);
        }

        public bool TrySell(string productId, int quantity, out Order order, out string error)
        {
            if (Sales == null) { order = null; error = "Dich vu ban hang chua san sang."; SetStatus(error); return false; }
            bool success = Sales.TrySell(productId, quantity, out order, out error);
            SetStatus(error);
            if (success)
            {
                RecordStoreSale(order.total);
                RefreshProductDisplays(productId);
            }
            return success;
        }

        public bool TrySell(string productId, int quantity, int customerBudget, out Order order, out string error)
        {
            if (Sales == null) { order = null; error = "Dich vu ban hang chua san sang."; SetStatus(error); return false; }
            bool success = Sales.TrySell(productId, quantity, customerBudget, out order, out error);
            SetStatus(error);
            if (success)
            {
                RecordStoreSale(order.total);
                RefreshProductDisplays(productId);
            }
            return success;
        }

        void RefreshProductDisplays(string productId)
        {
            foreach (ProductInteraction display in FindObjectsOfType<ProductInteraction>())
                if (display.ProductId == productId) display.RefreshVisibility();
        }

        void OnGUI()
        {
            if (!showPrototypeHUD || Instance != this) return;
            GUI.Box(new Rect(15, 15, 360, 92), "TECH STORE SIMULATOR - PROTOTYPE");
            GUI.Label(new Rect(30, 42, 330, 22), "Tien: " + Money.ToString("N0") + " VND | Da mua: " + ItemsPurchased);
            GUI.Label(new Rect(30, 66, 330, 22), status);
        }
    }
}
