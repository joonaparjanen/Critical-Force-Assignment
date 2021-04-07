namespace VendingMachineLib
{
    using System;

    public class PurchaseFailedException : Exception
    {
        public PurchaseFailedException()
        {
        }

        public PurchaseFailedException(string message)
            : base(message)
        {
        }

        public PurchaseFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
