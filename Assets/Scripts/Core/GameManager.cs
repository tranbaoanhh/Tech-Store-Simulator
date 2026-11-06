using UnityEngine;

namespace TechStoreSimulator
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] int startingMoney = 10000000;
        public int Money { get; private set; }
        public int ItemsPurchased { get; private set; }
        string status = "Cham E vao san pham de mua.";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Money = startingMoney;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Money < amount) { status = "Khong du tien!"; return false; }
            Money -= amount;
            ItemsPurchased++;
            status = "Mua thanh cong!";
            return true;
        }

        void OnGUI()
        {
            GUI.Box(new Rect(15, 15, 360, 92), "TECH STORE SIMULATOR - PROTOTYPE");
            GUI.Label(new Rect(30, 42, 330, 22), "Tien: " + Money.ToString("N0") + " VND | Da mua: " + ItemsPurchased);
            GUI.Label(new Rect(30, 66, 330, 22), status);
        }
    }
}
