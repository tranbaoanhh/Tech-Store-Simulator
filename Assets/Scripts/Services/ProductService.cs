using System.Collections.Generic;

namespace TechStoreSimulator
{
    /// <summary>Single source of truth for products and store stock.</summary>
    public class ProductService
    {
        readonly Dictionary<string, Product> products = new Dictionary<string, Product>();

        public IEnumerable<Product> Products => products.Values;

        public void Register(Product product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.id)) return;
            product.id = product.id.Trim();
            if (product.price < 0) product.price = 0;
            if (product.stock < 0) product.stock = 0;
            products[product.id] = product;
        }

        public bool TryGet(string id, out Product product)
        {
            return products.TryGetValue(id ?? string.Empty, out product);
        }

        public bool HasStock(string id, int quantity)
        {
            return quantity > 0 && TryGet(id, out Product product) && product.stock >= quantity;
        }

        public bool TryRemoveStock(string id, int quantity)
        {
            if (!HasStock(id, quantity)) return false;
            products[id].stock -= quantity;
            return true;
        }
    }
}
