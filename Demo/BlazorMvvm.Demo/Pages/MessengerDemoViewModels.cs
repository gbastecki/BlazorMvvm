using System;
using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages
{
    public partial class MessengerSenderViewModel : BlazorViewModel
    {
        private readonly IBlazorMessenger _messenger;

        [BlazorObservableProperty]
        private int _counter;

        [BlazorObservableProperty]
        private bool _isSending;

        public MessengerSenderViewModel(IBlazorMessenger messenger)
        {
            _messenger = messenger;
        }

        [BlazorCommand(OnIsExecutingChangedCallback = nameof(OnIsSendingChanged))]
        private async Task SendMessage()
        {
            Counter++;
            _messenger.Send(new CounterUpdateMessage(Counter));
            await Task.Delay(200);
        }

        private void OnIsSendingChanged(bool isExecuting)
        {
            IsSending = isExecuting;
        }
    }

    [BlazorMessenger]
    public partial class MessengerReceiverViewModel : BlazorViewModel, IBlazorRecipient<CounterUpdateMessage>, IDisposable
    {
        private readonly IBlazorMessenger _messenger;

        [BlazorObservableProperty]
        private int _receivedCounter;

        [BlazorObservableProperty]
        private int _messageCount;

        [BlazorObservableProperty]
        private string _lastMessage = "";

        public MessengerReceiverViewModel(IBlazorMessenger messenger)
        {
            _messenger = messenger;
            RegisterMessenger(_messenger);
        }

        public void Dispose()
        {
            UnregisterMessenger(_messenger);
        }

        public void Receive(CounterUpdateMessage message)
        {
            ReceivedCounter = message.Value;
            MessageCount++;
            LastMessage = $"Received: {message.Value} at {DateTime.Now:HH:mm:ss}";
        }
    }

    public class CounterUpdateMessage : ValueChangedMessage<int>
    {
        public CounterUpdateMessage(int value) : base(value) { }
    }
}
