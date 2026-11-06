using UnityEngine;

namespace TechStoreSimulator
{
    public class ProductInteraction : MonoBehaviour
    {
        [SerializeField] string displayName = "Ban phim";
        [SerializeField] int price = 350000;
        public string DisplayName => displayName + " (" + price.ToString("N0") + " VND)";

        public void Configure(string productName, int cost)
        {
            displayName = productName;
            price = cost;
        }

        public void TryBuy()
        {
            if (GameManager.Instance != null && GameManager.Instance.TrySpend(price))
                gameObject.SetActive(false); // MVP: muon ban duoc nhieu lan, can he thong ton kho
        }
    }
}
