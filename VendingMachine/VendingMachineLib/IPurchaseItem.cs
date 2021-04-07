namespace VendingMachineLib
{
    public interface IPurchaseItem
    {
        void PurchaseItem(string userId, string itemId, int count);
    }
}
