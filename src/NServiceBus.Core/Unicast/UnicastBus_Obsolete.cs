#pragma warning disable 1591
// ReSharper disable UnusedParameter.Global
namespace NServiceBus.Unicast
{
    using System;

    public partial class UnicastBus
    {
        [ObsoleteEx(RemoveInVersion = "6", TreatAsErrorFromVersion = "5", Message = "InMemory.Raise has been removed from the core.")]
        public void Raise<T>(Action<T> messageConstructor)
        {
            ThrowInMemoryException();
        }

        [ObsoleteEx(RemoveInVersion = "6", TreatAsErrorFromVersion = "5", Message = "InMemory.Raise has been removed from the core.")]
        public void Raise<T>(T @event)
        {
            ThrowInMemoryException();
        }

        static void ThrowInMemoryException()
        {
            throw new Exception("InMemory.Raise has been removed from the core.");
        }

        [ObsoleteEx(RemoveInVersion = "6", TreatAsErrorFromVersion = "5", Message = "InMemory has been removed from the core.")]
        public IInMemoryOperations InMemory
        {
            get
            {
                ThrowInMemoryException();
                return null;
            }
        }
    }
}
