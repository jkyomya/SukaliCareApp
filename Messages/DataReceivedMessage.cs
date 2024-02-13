using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SukaliCareApp.Messages;

public class DataReceivedMessage : ValueChangedMessage<string>
{
    public DataReceivedMessage(string value) : base(value)
    {
    }
}
