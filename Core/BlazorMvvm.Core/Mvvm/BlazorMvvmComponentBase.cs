using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorMvvm;
public abstract class BlazorMvvmComponentBase<TViewModel> : ComponentBase, IAsyncDisposable where TViewModel : IBlazorViewModel
{
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    [Inject] protected IBlazorMvvmViewModelFactory ViewModelFactory { get; set; } = default!;
    [Inject] protected BlazorMvvmScopedCache ScopedCache { get; set; } = default!;

    protected TViewModel? BaseViewModel;
    protected HashSet<string>? PropertyNamesHashset;
    private bool _shouldRefresh;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (BaseViewModel == null)
        {
            var vm = ViewModelFactory.GetViewModel(typeof(TViewModel), ServiceProvider, ScopedCache);
            if (vm != null)
            {
                var viewModel = (TViewModel)vm;
                if (viewModel != null)
                {
                    SetDataContext(viewModel);
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        OnDispose();
        await OnDisposeAsync();
        GC.SuppressFinalize(this);
    }
    protected virtual void OnDispose()
    {
        if (this.BaseViewModel != null)
        {
            this.BaseViewModel.OnTriggerRefresh -= TriggerRefresh;
        }
    }
    protected virtual async ValueTask OnDisposeAsync()
    {
        await ValueTask.CompletedTask;
    }

    protected void SetDataContext(TViewModel? ViewModel)
    {
        if (ReferenceEquals(this.BaseViewModel, ViewModel)) return;
        if (this.BaseViewModel != null)
        {
            this.BaseViewModel.OnTriggerRefresh -= TriggerRefresh;
        }
        this.BaseViewModel = ViewModel;
        if (this.BaseViewModel != null)
        {
            this.BaseViewModel.OnTriggerRefresh += TriggerRefresh;
        }
        InvokeRefresh();
    }
    protected void SetBoundPropertyNames(params string[]? PropertyNames)
    {
        this.PropertyNamesHashset = PropertyNames != null && PropertyNames.Length > 0 ? new(PropertyNames) : null;
    }

    private void TriggerRefresh(string? propertyName)
    {
        if (propertyName != null && PropertyNamesHashset != null)
        {
            if (!PropertyNamesHashset.Contains(propertyName)) return;
        }
        InvokeRefresh();
    }

    protected virtual void InvokeRefresh()
    {
        _shouldRefresh = true;
        base.InvokeAsync(StateHasChanged);
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRefresh) return false;
        _shouldRefresh = false;
        return base.ShouldRender();
    }
}