using BlazorMvvm;
using BlazorMvvm.Demo.JsHandlers;
using Microsoft.AspNetCore.Components;

namespace BlazorMvvm.Demo.Pages
{
    public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
    {
        [Inject] SvgHandler SvgHandler { get; set; }
        ObservablePartViewModel ObservablePartViewModel;
        SharedObservableViewModel SharedObservableViewModel;
        ButtonExampleViewModel ButtonExampleViewModel;
        SvgWrapperViewModel SvgWrapperViewModel;

        protected override void OnInitialized()
        {
            ObservablePartViewModel = new();
            SharedObservableViewModel = new();
            ButtonExampleViewModel = new();
            SvgWrapperViewModel = new(SvgHandler);
            base.OnInitialized();
        }
    }
}
