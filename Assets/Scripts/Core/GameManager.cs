using System.Collections;
using UnityEngine;

namespace TechStoreSimulator
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] int startingMoney = 10000000;
        [SerializeField, Min(1)] int demoCustomerCount = 5;
        [SerializeField, Min(0f)] float timeBetweenCustomers = 3f;
        public int Money { get; private set; }
        public int ItemsPurchased { get; private set; }
        public ProductService Products { get; private set; }
        public FinanceService Finance { get; private set; }
        public SalesService Sales { get; private set; }
        public string Status { get; private set; } = "Cham E vao san pham de mua.";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Money = startingMoney;
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

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Money < amount) { Status = "Khong du tien!"; return false; }
            Money -= amount;
            ItemsPurchased++;
            Status = "Mua thanh cong!";
            return true;
        }

        public void RecordStoreSale(int amount)
        {
            if (amount <= 0 || Money > int.MaxValue - amount) return;
            Money += amount;
        }

        public void SetStatus(string message)
        {
            Status = message;
        }

        public bool TrySell(string productId, int quantity, out Order order, out string error)
        {
            if (Sales == null) { order = null; error = "Dich vu ban hang chua san sang."; Status = error; return false; }
            bool success = Sales.TrySell(productId, quantity, out order, out error);
            Status = error;
            if (success) RecordStoreSale(order.total);
            return success;
        }

        public bool TrySell(string productId, int quantity, int customerBudget, out Order order, out string error)
        {
            if (Sales == null) { order = null; error = "Dich vu ban hang chua san sang."; Status = error; return false; }
            bool success = Sales.TrySell(productId, quantity, customerBudget, out order, out error);
            Status = error;
            if (success) RecordStoreSale(order.total);
            return success;
        }

        void OnGUI()
        {
            GUI.Box(new Rect(15, 15, 360, 92), "TECH STORE SIMULATOR - PROTOTYPE");
            GUI.Label(new Rect(30, 42, 330, 22), "Tien: " + Money.ToString("N0") + " VND | Da mua: " + ItemsPurchased);
            GUI.Label(new Rect(30, 66, 330, 22), Status);
        }
    }
}
