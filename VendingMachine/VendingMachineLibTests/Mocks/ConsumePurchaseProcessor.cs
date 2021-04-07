using System;
using System.Collections.Generic;
using VendingMachineLib;

namespace VendingMachineLibTests.Mocks
{
    public class ConsumePurchaseProcessor : IConsumePurchaseProsessor
    {
        private Dictionary<string, int> userBalances;

        public ConsumePurchaseProcessor(Dictionary<string, int> userBalances)
        {
            this.userBalances = userBalances;
        }

        bool IConsumePurchaseProsessor.ConsumePurchase(string buyerId, int price)
        {
            if(userBalances.TryGetValue(buyerId,out int balance) && balance >= price)
            {
                userBalances[buyerId] -= price;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetUserBalance(string userId)
        {
            return userBalances.TryGetValue(userId, out int balance) ? balance : 0;
        }
    }
}
