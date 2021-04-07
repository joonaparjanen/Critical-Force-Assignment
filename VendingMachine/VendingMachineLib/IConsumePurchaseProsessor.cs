namespace VendingMachineLib
{
    public interface IConsumePurchaseProsessor
    {
        /// <summary>
        /// Used for triggering purchase charge, and determining were that charge triggered or not.
        /// </summary>
        /// <param name="buyerId">Buyer to be charged</param>
        /// <param name="price">Charged amount of the purchase.</param>
        /// <returns>Should return TRUE if the purchase was charged, otherwise FALSE</returns>
        bool ConsumePurchase(string buyerId, int price);
    }
}
