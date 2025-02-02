using Microsoft.AspNetCore.Components;

namespace BlazorMvvm;
public partial class ObservableComponent : BlazorMvvmComponentBase<IBlazorViewModel>
{
    private BlazorViewModel? _viewModel;
#pragma warning disable BL0007
    [Parameter]
    public BlazorViewModel? ViewModel
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
    private string[]? _propertyNames;
#pragma warning disable BL0007
    [Parameter]
    public string[]? PropertyNames
#pragma warning restore BL0007
    {
        get => _propertyNames;
        set
        {
            if (_propertyNames == value) return;
            _propertyNames = value;
            SetBoundPropertyNames(_propertyNames);
        }
    }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}