using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SukaliCareApp.Messaging;

public class DataReceivedMessage : ValueChangedMessage<CgmReading>
{
    public DataReceivedMessage(CgmReading value) : base(value)
    {
    }
}
