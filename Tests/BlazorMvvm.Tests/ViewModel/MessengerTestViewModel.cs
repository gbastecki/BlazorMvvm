namespace BlazorMvvm.Tests.ViewModel
{
    /// <summary>
    /// Test ViewModel using BlazorMessenger attribute with multiple IBlazorRecipient implementations.
    /// </summary>
    [BlazorMessenger]
    public partial class MessengerTestViewModel : BlazorViewModel, 
        IBlazorRecipient<CounterChangedMessage>, 
        IBlazorRecipient<NameChangedMessage>
    {
        [BlazorObservableProperty]
        private int _counter;

        [BlazorObservableProperty]
        private string? _name;

        [BlazorObservableProperty]
        private int _messageCount;

        public void Receive(CounterChangedMessage message)
        {
            Counter = message.Value;
            MessageCount++;
        }

        public void Receive(NameChangedMessage message)
        {
            Name = message.Value;
            MessageCount++;
        }
    }

    public class CounterChangedMessage : ValueChangedMessage<int>
    {
        public CounterChangedMessage(int value) : base(value) { }
    }

    public class NameChangedMessage : ValueChangedMessage<string>
    {
        public NameChangedMessage(string value) : base(value) { }
    }
}
