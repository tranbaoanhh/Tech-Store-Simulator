using UnityEngine;

namespace TechStoreSimulator
{
    /// <summary>Prototype sales HUD; can be replaced by Canvas UI without changing sales logic.</summary>
    public class SalesUI : MonoBehaviour
    {
        void OnGUI()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null || manager.Finance == null) return;

            GUI.Box(new Rect(15, 115, 260, 68), "Ban hang");
            GUI.Label(new Rect(30, 140, 235, 20), "Doanh thu: " + manager.Finance.Revenue.ToString("N0") + " VND");
            GUI.Label(new Rect(30, 160, 235, 20), "Don thanh cong: " + manager.Finance.CompletedSales);

            int productCount = 0;
            foreach (Product product in manager.Products.Products) productCount++;
            GUI.Box(new Rect(15, 190, 260, 35 + productCount * 20), "Ton kho");
            int row = 0;
            foreach (Product product in manager.Products.Products)
            {
                GUI.Label(new Rect(30, 215 + row * 20, 235, 20), product.displayName + ": " + product.stock);
                row++;
            }
        }
    }
}
