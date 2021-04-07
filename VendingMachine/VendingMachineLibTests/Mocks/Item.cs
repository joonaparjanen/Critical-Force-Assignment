using VendingMachineLib;

namespace VendingMachineLibTests.Mocks
{
    internal struct Item : IItem
    {
        public string id { get; private set; }

        public Item(string id)
        {
            this.id = id;
        }
    }
}
