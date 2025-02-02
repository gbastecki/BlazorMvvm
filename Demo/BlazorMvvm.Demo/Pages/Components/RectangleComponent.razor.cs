using Microsoft.AspNetCore.Components;

namespace BlazorMvvm.Demo.Pages.Components
{
    public partial class RectangleComponent : BlazorMvvmComponentBase<RectangleViewModel>
    {
        private RectangleViewModel _viewModel;
#pragma warning disable BL0007
        [Parameter]
        public RectangleViewModel ViewModel
#pragma warning restore BL0007
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;
                _viewModel = value;
                SetDataContext(_viewModel);
            }
        }
    }
}
