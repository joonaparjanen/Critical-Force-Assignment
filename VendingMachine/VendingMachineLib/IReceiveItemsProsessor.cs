using System;
using System.Collections.Generic;
using System.Text;

namespace VendingMachineLib
{
    public interface IReceiveItemsProsessor
    {
        /// <summary>
        /// Triggers when purchase is triggered and the product should be given to buyer.
        /// </summary>
        /// <param name="itemId">ItemId that was bought.</param>
        void ReceiveItems(string userId, string itemId, int count);
    }
}
