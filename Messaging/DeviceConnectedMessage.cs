using CommunityToolkit.Mvvm.Messaging.Messages;
using Plugin.BLE.Abstractions.Contracts;

namespace SukaliCareApp.Messaging;
public class DeviceConnectedMessage : ValueChangedMessage<IDevice>
{
    public DeviceConnectedMessage(IDevice value) : base(value)
    {
    }
}
