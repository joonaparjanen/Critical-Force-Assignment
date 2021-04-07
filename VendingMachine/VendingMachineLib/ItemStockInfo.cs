namespace VendingMachineLib
{
    /// <summary>
    /// Contains seller information about the item
    /// </summary>
    public struct ItemStockInfo
    {
        public readonly IItem item;
        public int price;
        public int count;

        public ItemStockInfo(IItem item, int price, int count = 1 )
        {
            this.item = item;
            this.price = price;
            this.count = count;
        }
    }
}
