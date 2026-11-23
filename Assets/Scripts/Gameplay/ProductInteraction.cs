using UnityEngine;

namespace TechStoreSimulator
{
    public class ProductInteraction : MonoBehaviour
    {
        [SerializeField] string displayName = "Ban phim";
        [SerializeField] int price = 350000;
        [SerializeField] int initialStock = 5;
        public string DisplayName => displayName + " (" + price.ToString("N0") + " VND)";
        public string ProductId => displayName.Trim().ToLowerInvariant();

        void Start()
        {
            RegisterProduct();
        }

        public void Configure(string productName, int cost)
        {
            displayName = productName;
            price = cost;
            RegisterProduct();
        }

        void RegisterProduct()
        {
            if (GameManager.Instance == null || GameManager.Instance.Products == null) return;
            if (!GameManager.Instance.Products.TryGet(ProductId, out _))
                GameManager.Instance.Products.Register(new Product { id = ProductId, displayName = displayName, price = price, stock = initialStock });
        }

        public void TryBuy()
        {
            RegisterProduct();
            if (GameManager.Instance == null || !GameManager.Instance.Products.TryGet(ProductId, out Product product)) return;
            if (product.stock <= 0) { GameManager.Instance.SetStatus("San pham da het hang."); return; }
            if (!GameManager.Instance.TrySpend(product.price)) return;
            GameManager.Instance.Products.TryRemoveStock(ProductId, 1);
            gameObject.SetActive(false); // A physical display item is taken; shared stock remains authoritative.
        }
    }
}
