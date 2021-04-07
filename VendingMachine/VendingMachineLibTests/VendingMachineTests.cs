using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using VendingMachineLib;
using VendingMachineLibTests.Mocks;

namespace VendingMachineLibTests
{
    public class VendingMachineTests
    {
        private ConsumePurchaseProcessor consumePurchaseProcessor;
        private ReceiveItemsProsessor receiveItemsProsessor;
        private VendingMachine vendingMachine;
     
        private Dictionary<string, int> initialUserBalances { 
            get {
                return new Dictionary<string, int>
                {
                    { "user_with_0", 0 },
                    { "user_with_1", 1 },
                    { "user_with_10", 10 },
                    { "user_with_100", 100 },
                    { "user_with_1000", 1000 }
                };
            }
        }
        private ItemStockInfo[] initialVendingMachineItems
        {
            get
            {
                return new ItemStockInfo[]{
                    new ItemStockInfo(new Item("weapon_1"), 124, 1),
                    new ItemStockInfo(new Item("weapon_2"), 32, 1),
                    new ItemStockInfo(new Item("weapon_3"), 124, 17),
                    new ItemStockInfo(new Item("food_12"), 7, 1),
                    new ItemStockInfo(new Item("food_244"), 14, 5),
                    new ItemStockInfo(new Item("drink_343"), 3, 5)
                }.OrderBy(item => item.item.id).ToArray();  
            }
        }

        [SetUp]
        public void Setup()
        {
            consumePurchaseProcessor = new ConsumePurchaseProcessor(initialUserBalances);
            receiveItemsProsessor = new ReceiveItemsProsessor();

            var builder = new ContainerBuilder();
            builder.RegisterType<VendingMachine>();
            builder.RegisterType<Logger>().As<ILogger>();
            builder.RegisterInstance(consumePurchaseProcessor).As<IConsumePurchaseProsessor>();
            builder.RegisterInstance(receiveItemsProsessor).As<IReceiveItemsProsessor>();

            var container = builder.Build();

            vendingMachine = container.Resolve<VendingMachine>();

            vendingMachine.AddItems(new List<ItemStockInfo>(initialVendingMachineItems));
        }

        [Test]
        public void Test_AddItems()
        {
            vendingMachine.RemoveItems();

            var a = new ItemStockInfo[]{
                new ItemStockInfo(new Item("w_1"), 124, 1),
                new ItemStockInfo(new Item("w_2"), 32, 1),
            };

            var b = new ItemStockInfo[]{
                new ItemStockInfo(new Item("f_12"), 7, 0),
                new ItemStockInfo(new Item("f_24"), 14, 5),
                new ItemStockInfo(new Item("d_12"), 3, 5)
            };

            var c = new ItemStockInfo[]{
                new ItemStockInfo(new Item("as_142"), 117, 0),
                new ItemStockInfo(new Item("g_242"), 1124, 5),
                new ItemStockInfo(new Item("hg_342"), 344, 5)
            };

            vendingMachine.AddItems(a);
            vendingMachine.AddItems(b);
            vendingMachine.AddItems(c);


            var expectedItems = new List<ItemStockInfo>();
            expectedItems.AddRange(a);
            expectedItems.AddRange(b);
            expectedItems.AddRange(c);
            expectedItems = expectedItems
                .Where(item => item.count > 0)
                .OrderBy(item => item.item.id).ToList();

            var items = vendingMachine.GetCatalogueItems();
            Assert.AreEqual(expectedItems.ToArray(), items);
        }

        [Test]
        public void Test_GetItemsInCatalogue()
        {
            // order both lists by itemId; there should not be duplicates becase values are derieved from Dictionary
            var items = vendingMachine.GetCatalogueItems();
            var initialItems = initialVendingMachineItems.Where(item => item.count>0).OrderBy(item => item.item.id).ToArray();

            Assert.AreEqual(items, initialItems);
        }


        [Test]
        public void Test_ItemStockCount()
        {
            Assert.AreEqual(0, GetItemStockCount("unknown_id"));
            Assert.AreEqual(1, GetItemStockCount("weapon_1"));
            Assert.AreEqual(1, GetItemStockCount("weapon_2"));
            Assert.AreEqual(17, GetItemStockCount("weapon_3"));
            Assert.AreEqual(1, GetItemStockCount("food_12"));
            Assert.AreEqual(5, GetItemStockCount("food_244"));
            Assert.AreEqual(5, GetItemStockCount("drink_343"));
        }

        [Test]
        public void Test_PurchaceItem1()
        {
            string userId = "user_with_1000";
            string itemId = "unknown_item_id";
            int count = 1;

            ItemStockInfo itemInfo = new ItemStockInfo();
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // unknown item
        }

        [Test]
        public void Test_PurchaceItem2()
        {
            string userId = "user_with_10";
            string itemId = "food_244";
            int count = 1;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("food_244"), 14, 5); // copied from initialVendingMachineItems
            int userBalance = 10;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // too low balance in buyer
        }

        [Test]
        public void Test_PurchaceItem3()
        {
            string userId = "user_with_100";
            string itemId = "food_244";
            int count = 2;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("food_244"), 14, 5); // copied from initialVendingMachineItems
            int userBalance = 100;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(true, purchaseWasSuccessful);
        }

        [Test]
        public void Test_PurchaceItem4()
        {
            string userId = "unknown_user";
            string itemId = "food_244";
            int count = 1;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("food_244"), 14, 5); // copied from initialVendingMachineItems
            int userBalance = 0;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // unknown user
        }


        [Test]
        public void Test_PurchaceItem5()
        {
            string userId = "user_with_1000";
            string itemId = "weapon_1";
            int count = 8;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("weapon_1"), 124, 1); // copied from initialVendingMachineItems
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // not enough items
        }

        [Test]
        public void Test_PurchaceItem6()
        {
            string userId = "user_with_1000";
            string itemId = "weapon_1";
            int count = 9;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("weapon_1"), 124, 1); // copied from initialVendingMachineItems
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful);  // not enough items & not enough balance
        }

        [Test]
        public void Test_PurchaceItem7()
        {
            string userId = "user_with_1000";
            string itemId = "weapon_3";
            int count = 8;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("weapon_3"), 124, 17); // copied from initialVendingMachineItems
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(true, purchaseWasSuccessful);
        }

        [Test]
        public void Test_PurchaceItem8()
        {
            string userId = "user_with_1000";
            string itemId = "weapon_3";
            int count = 0;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("weapon_3"), 124, 17); // copied from initialVendingMachineItems
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // cannot purchase zero items
        }



        [Test]
        public void Test_PurchaceItem9()
        {
            string userId = "user_with_1000";
            string itemId = "weapon_3";
            int count = -1;

            ItemStockInfo itemInfo = new ItemStockInfo(new Item("weapon_3"), 124, 17); // copied from initialVendingMachineItems
            int userBalance = 1000;
            int userItemCount = 0;

            var purchaseWasSuccessful = PurchaceItemTester((userId, itemId, count), (itemInfo, userBalance, userItemCount));
            Assert.AreEqual(false, purchaseWasSuccessful); // cannot purchase negative amount of items
        }

        [Test]
        public void Test_RemoveItems()
        {
            vendingMachine.RemoveItems();
            var items = vendingMachine.GetCatalogueItems();
            Assert.AreEqual(0, items.Length);
        }



        [Test]
        public void Test_ReplaceItems()
        {
            var itemsToReplace = new ItemStockInfo[]{
                new ItemStockInfo(new Item("c_12"), 7, 0),
                new ItemStockInfo(new Item("b_24"), 14, 5),
                new ItemStockInfo(new Item("a_12"), 3, 5)
            };

            vendingMachine.ReplaceItems(itemsToReplace);

            var shouldMatch = itemsToReplace 
                .Where(isi => isi.count>0)
                .OrderBy(isi => isi.item.id).ToArray();

            Assert.AreEqual(shouldMatch, vendingMachine.GetCatalogueItems());
        }


        private int GetItemStockCount(string itemId)
        {
            return vendingMachine.TryGetItemStockInfo(itemId, out var itemInfo) ? itemInfo.count : 0;
        }

        /// <returns>TRUE if purchase was successful, otherwise FALSE</returns>
        private bool PurchaceItemTester((string userId, string itemId, int count) input, (ItemStockInfo itemStockInfo, int userBalance, int userItemCount) current)
        {
            var (userId, itemId, count) = input;
            var (itemStockInfo, userBalance, userItemCount) = current;

            bool oneOrMoreItems = count >= 1; 
            bool enoughMoney = count * itemStockInfo.price <= userBalance;
            bool enoughItemsInStock = count <= itemStockInfo.count;
            bool purchaseWillHappen = oneOrMoreItems && enoughItemsInStock && enoughMoney;

            if (purchaseWillHappen)
            {
                Assert.DoesNotThrow(() => vendingMachine.PurchaseItem(userId, itemId, count));

                // items were removed from vendingmachine stock & item stock balance is not negative: 
                var itemStockBalance = GetItemStockCount(itemId);
                Assert.AreEqual(itemStockInfo.count - count, itemStockBalance);
                if (itemStockBalance < 0) Assert.Fail("Vending machine item stock balance is negative.");

                // User did lose money && balance is not negative (this tests that the mock works properly)
                var balance = consumePurchaseProcessor.GetUserBalance(userId);
                Assert.AreEqual(userBalance - itemStockInfo.price * count, balance);
                if (balance < 0) Assert.Fail("User balance was negative, fix mock of ConsumePurchaseProsessor");

                // user did get more items
                Assert.AreEqual(userItemCount + count, receiveItemsProsessor.GetUserItemCount(userId, itemId));
            }
            else
            {
                if (oneOrMoreItems)
                {
                    Assert.Throws<PurchaseFailedException>(() => vendingMachine.PurchaseItem(userId, itemId, count));
                }
                else
                {
                    Assert.Throws<ArgumentException>(() => vendingMachine.PurchaseItem(userId, itemId, count));
                }

                // vending machine items did not change
                Assert.AreEqual(initialVendingMachineItems, vendingMachine.GetCatalogueItems());

                // User did not lose money (this tests that the mock works properly)
                Assert.AreEqual(userBalance, consumePurchaseProcessor.GetUserBalance(userId));

                // user did not get any items
                Assert.AreEqual(0, receiveItemsProsessor.GetUserItemCount(userId, itemId));
            }
            return purchaseWillHappen;
        }
    }
}