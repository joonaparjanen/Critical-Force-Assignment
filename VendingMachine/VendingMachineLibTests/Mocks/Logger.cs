using System;
using VendingMachineLib;

namespace VendingMachineLibTests.Mocks
{
    public class Logger : ILogger
    {
        void ILogger.Debug(string message)
        {
            Console.WriteLine("DEBUG: " + message);
        }

        void ILogger.Error(Exception exception)
        {
            Console.WriteLine("ERROR: " + exception.ToString());
        }

        void ILogger.Message(string message)
        {
            Console.WriteLine("MESSAGE: " + message);
        }
    }
}
