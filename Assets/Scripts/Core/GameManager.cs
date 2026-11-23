using System;
using UnityEngine;

namespace TechStoreSimulator
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        // Prototype balance only; this is not the future FinanceService.
        [SerializeField, Min(0)] int startingMoney = 10000000;
        [SerializeField] bool showPrototypeHUD = true;
        public int Money { get; private set; }
        public int ItemsPurchased { get; private set; }
        public string Status => status;

        // Subscribe in OnEnable and unsubscribe in OnDisable. Read the properties
        // after subscribing for the initial UI state; events report changes only.
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
        }

        void OnValidate()
        {
            startingMoney = Mathf.Max(0, startingMoney);
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

        /// <summary>
        /// Integration point for a future save service. Negative values are rejected
        /// without changing the balance or purchase count. Does not read or write files.
        /// </summary>
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

            // Commit all values before notifying listeners so they can read a
            // consistent state, including when restoring a saved session.
            Money = money;
            ItemsPurchased = itemsPurchased;
            status = newStatus;

            if (moneyChanged) MoneyChanged?.Invoke(money);
            if (purchasesChanged) ItemsPurchasedChanged?.Invoke(itemsPurchased);
            if (statusChanged) StatusChanged?.Invoke(newStatus);
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
