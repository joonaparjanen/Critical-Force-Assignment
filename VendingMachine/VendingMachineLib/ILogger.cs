using System; 
namespace VendingMachineLib
{
    public interface ILogger
    {
        void Error(Exception exception);
        void Debug(string message);
        void Message(string message);
    }
}
