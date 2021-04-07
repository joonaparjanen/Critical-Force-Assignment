using System;
using System.Collections.Generic;
using VendingMachineLib;

namespace VendingMachineLibTests.Mocks
{
    public class ReceiveItemsProsessor : IReceiveItemsProsessor
    {
        /// <summary>
        /// UserId --> ItemId --> ItemCount
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> userInventoryCollection = new Dictionary<string, Dictionary<string, int>>();

        void IReceiveItemsProsessor.ReceiveItems(string userId, string itemId, int count)
        {
            if (userInventoryCollection.TryGetValue(userId, out var inventory))
            {
                if(inventory.ContainsKey(itemId))
                {
                    inventory[itemId] += count;
                }
                else
                {
                    inventory[itemId] = count;
                }
                userInventoryCollection[userId] = inventory;
            }
            else
            {
                userInventoryCollection[userId] = new Dictionary<string, int> {
                    {itemId,count}
                };
            }
        }

        public int GetUserItemCount(string userId, string itemId)
        {
            if (userInventoryCollection.TryGetValue(userId, out var inventory) 
                && inventory.TryGetValue(itemId, out int count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }
    }
}
