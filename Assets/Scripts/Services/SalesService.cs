using System;

namespace TechStoreSimulator
{
    public class SalesService
    {
        readonly ProductService products;
        readonly FinanceService finance;

        public SalesService(ProductService products, FinanceService finance)
        {
            this.products = products ?? throw new ArgumentNullException(nameof(products));
            this.finance = finance ?? throw new ArgumentNullException(nameof(finance));
        }

        public bool TrySell(string productId, int quantity, out Order order, out string error)
        {
            order = null;
            if (quantity <= 0) { error = "So luong mua phai lon hon 0."; return false; }
            if (!products.TryGet(productId, out Product product)) { error = "Khong tim thay san pham."; return false; }
            if (product.price <= 0) { error = "Gia san pham khong hop le."; return false; }
            if (product.stock < quantity) { error = "San pham da het hang."; return false; }
            if ((long)product.price * quantity > int.MaxValue) { error = "Tong tien vuot gioi han."; return false; }

            int total = product.price * quantity;
            if (!products.TryRemoveStock(productId, quantity)) { error = "Khong the cap nhat ton kho."; return false; }
            if (!finance.TryRecordSale(total))
            {
                // Restore stock if finance rejects the transaction.
                product.stock += quantity;
                error = "Khong the ghi nhan doanh thu.";
                return false;
            }

            order = new Order { id = Guid.NewGuid().ToString("N"), total = total, completed = true };
            error = "Giao dich thanh cong.";
            return true;
        }
    }
}
