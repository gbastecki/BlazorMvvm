using BlazorMvvm;
using BlazorMvvm.Demo.JsHandlers;
using Microsoft.AspNetCore.Components;

namespace BlazorMvvm.Demo.Pages
{
    public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
    {
        [Inject] SvgHandler SvgHandler { get; set; }
        HomeViewModel ViewModel;
        ObservablePartViewModel ObservablePartViewModel;
        SharedObservableViewModel SharedObservableViewModel;
        ButtonExampleViewModel ButtonExampleViewModel;
        SvgWrapperViewModel SvgWrapperViewModel;

        protected override void OnInitialized()
        {
            ViewModel = new();
            ObservablePartViewModel = new();
            SharedObservableViewModel = new();
            ButtonExampleViewModel = new();
            SvgWrapperViewModel = new(SvgHandler);
            SetDataContext(ViewModel);
            base.OnInitialized();
        }
    }
}
