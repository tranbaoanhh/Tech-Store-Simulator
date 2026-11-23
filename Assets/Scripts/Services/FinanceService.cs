namespace TechStoreSimulator
{
    /// <summary>Records store income from completed customer sales.</summary>
    public class FinanceService
    {
        public int Revenue { get; private set; }
        public int CompletedSales { get; private set; }

        public bool TryRecordSale(int amount)
        {
            if (amount <= 0 || Revenue > int.MaxValue - amount) return false;
            Revenue += amount;
            CompletedSales++;
            return true;
        }
    }
}
