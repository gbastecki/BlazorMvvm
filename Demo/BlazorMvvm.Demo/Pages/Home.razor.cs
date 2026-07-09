using BlazorMvvm.Demo.JsHandlers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages
{
    public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
    {
        [Inject] SvgHandler SvgHandler { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IBlazorMessenger Messenger { get; set; }

        ObservablePartViewModel ObservablePartViewModel;
        SharedObservableViewModel SharedObservableViewModel;
        ButtonExampleViewModel ButtonExampleViewModel;
        SvgWrapperViewModel SvgWrapperViewModel;
        DebounceExampleViewModel DebounceExampleViewModel;

        MessengerSenderViewModel MessengerSender;
        MessengerReceiverViewModel MessengerReceiver;

        protected override void OnInitialized()
        {
            ObservablePartViewModel = new();
            SharedObservableViewModel = new();
            ButtonExampleViewModel = new();
            SvgWrapperViewModel = new(SvgHandler);
            DebounceExampleViewModel = new();

            MessengerSender = new MessengerSenderViewModel(Messenger);
            MessengerReceiver = new MessengerReceiverViewModel(Messenger);

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("scrollSync.init");
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override void OnDispose()
        {
            DebounceExampleViewModel?.Dispose();
            base.OnDispose();
        }
    }
}
