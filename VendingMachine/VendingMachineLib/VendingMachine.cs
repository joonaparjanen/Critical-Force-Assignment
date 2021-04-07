using System;
using System.Collections.Generic;
using System.Linq;

namespace VendingMachineLib
{
    public class VendingMachine : IPurchaseItem
    {
        private ILogger logger;
        private IConsumePurchaseProsessor consumePurchaseProsessor;
        private IReceiveItemsProsessor receiveItemProsessor;
        private readonly Dictionary<string, ItemStockInfo> catalogue;

        public VendingMachine(ILogger logger, IConsumePurchaseProsessor consumePurchaseProsessor, IReceiveItemsProsessor receiveItemProsessor)
        {
            this.logger = logger;
            this.consumePurchaseProsessor = consumePurchaseProsessor;
            this.receiveItemProsessor = receiveItemProsessor;
            catalogue = new Dictionary<string, ItemStockInfo>();
        }

        /// <returns>Copy of all ItemStoreInfos in Vending machine catalogue</returns>
        public ItemStockInfo[] GetCatalogueItems()
        {
            return catalogue.Values.OrderBy(item => item.item.id).ToArray();
        }


        /// <summary>
        /// Adds Items to the catalogue, if the item's count is zero or less, it is discarded.
        /// </summary>
        public void AddItems(IEnumerable<ItemStockInfo> itemStockInfos)
        {
            foreach (ItemStockInfo p in itemStockInfos)
            {
                if (p.count <= 0) continue;

                if (catalogue.TryGetValue(p.item.id, out ItemStockInfo itemInfo))
                {
                    itemInfo.count += p.count;
                    catalogue[p.item.id] = itemInfo;
                }
                else
                {
                    catalogue[p.item.id] = p;
                }
            }
            logger.Debug($"Items added to VendingMachine[{GetHashCode()}]");
        }

        /// <summary>
        /// Removes items in catalogue
        /// </summary>
        public void RemoveItems()
        {
            catalogue.Clear();
            logger.Debug($"Items removed from VendingMachine[{GetHashCode()}]");
        }

        /// <summary>
        /// Removes items from catalogue and replaces them with new items.
        /// <![CDATA[This method is here only to match assigment criteria.]]>
        /// </summary>
        public void ReplaceItems(IEnumerable<ItemStockInfo> itemStockInfos)
        {
            RemoveItems();
            AddItems(itemStockInfos);
        }


        /// <summary>
        /// Function for getting ItemStockInfo
        /// </summary>
        /// <returns>TRUE if ItemStockInfo exist, otherwise FALSE</returns>
        public bool TryGetItemStockInfo(string itemId, out ItemStockInfo itemStockInfo)
        {
            var exists = catalogue.TryGetValue(itemId, out var itemInfo);
            itemStockInfo = itemInfo;
            return exists;
        }

        /// <summary>
        /// Handles VendingMachine purchase.
        /// <para/>
        /// <br>throws ArgumentException if count is less than 1</br>
        /// <br>throws PurchaseFailedException if purchase fails otherwise</br>
        /// </summary>
        /// <param name="buyerId">Buyer that have triggered the purchase</param>
        /// <param name="itemId">ItemId that the buyer tries to buy</param>
        /// <param name="count">Count of the item user wants to buy</param>
        public void PurchaseItem(string buyerId, string itemId, int count)
        {
            logger.Debug($"Try Purchase: {count} items[{itemId}] from VendingMachine[{GetHashCode()}] to buyer: {buyerId}");

            if (count <= 0) 
                error(new ArgumentException($"You are trying to purchase {count} items, you have to buy atleast 1 item."));
            else if (!TryGetItemStockInfo(itemId, out var itemInfo)) 
                error(new PurchaseFailedException("Item does not exist in catalogue"));
            else if (itemInfo.count < count) 
                error(new PurchaseFailedException("Vending machine doesn't have enough items to buy."));
            else if (!consumePurchaseProsessor.ConsumePurchase(buyerId, itemInfo.price * count)) 
                error(new PurchaseFailedException("Failed to consume purchase"));

            RemoveItems(itemId, count);

            receiveItemProsessor.ReceiveItems(buyerId, itemId, count);
            logger.Message($"Purchase completed: {count} items[{itemId}] was purchased from VendingMachine[{GetHashCode()}] and added to buyer: {buyerId}");
        }


        /// <summary>
        /// Removes specified amount of items with itemId, Throws exception if ItemStockCount is not sufficient.
        /// </summary>
        private void RemoveItems(string itemId, int count)
        {
            if (!TryGetItemStockInfo(itemId, out var itemInfo)) throw new Exception("Item does not exist in catalogue");
            else if (itemInfo.count < count) throw new Exception("Vending machine doesn't have enough items to remove.");

            itemInfo.count -= count;
            if (itemInfo.count <= 0) catalogue.Remove(itemId);
            else catalogue[itemId] = itemInfo;

            logger.Debug($"Remove items from VendingMachine[{GetHashCode()}]");
        }

        /// <summary>
        /// Feedforward errors to logger and throw that same exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private void error(Exception exception)
        {
            logger.Error(exception);
            throw exception;
        }
    }
}
