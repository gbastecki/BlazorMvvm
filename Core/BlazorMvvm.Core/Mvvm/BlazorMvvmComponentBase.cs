using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorMvvm;

public abstract class BlazorMvvmComponentBase<TViewModel> : ComponentBase, IAsyncDisposable where TViewModel : IBlazorViewModel
{
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    protected IBlazorMvvmViewModelFactory? ViewModelFactory { get; set; } = default!;
    protected BlazorMvvmScopedCache? ScopedCache { get; set; } = default!;

    protected TViewModel? BaseViewModel;
    protected HashSet<string>? PropertyNamesHashset;
    private bool _shouldRefresh;
    private TaskCompletionSource? _renderTcs;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ViewModelFactory = ServiceProvider.GetService(typeof(IBlazorMvvmViewModelFactory)) as IBlazorMvvmViewModelFactory;
        ScopedCache = ServiceProvider.GetService(typeof(BlazorMvvmScopedCache)) as BlazorMvvmScopedCache;

        if (BaseViewModel == null && ViewModelFactory != null && ScopedCache != null)
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
            this.BaseViewModel.OnTriggerRefreshAsync -= TriggerRefreshAsync;
        }
        _renderTcs?.TrySetCanceled();
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
            this.BaseViewModel.OnTriggerRefreshAsync -= TriggerRefreshAsync;
        }
        this.BaseViewModel = ViewModel;
        if (this.BaseViewModel != null)
        {
            this.BaseViewModel.OnTriggerRefreshAsync += TriggerRefreshAsync;
        }
        InvokeRefresh();
    }
    protected void SetBoundPropertyNames(params string[]? PropertyNames)
    {
        this.PropertyNamesHashset = PropertyNames != null && PropertyNames.Length > 0 ? new(PropertyNames) : null;
    }

    private Task TriggerRefreshAsync(string? propertyName)
    {
        if (propertyName != null && PropertyNamesHashset != null)
        {
            if (!PropertyNamesHashset.Contains(propertyName)) return Task.CompletedTask;
        }
        return InvokeRefreshAsync();
    }

    protected virtual Task InvokeRefreshAsync()
    {
        _shouldRefresh = true;
        _renderTcs ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = _renderTcs.Task;
        _ = base.InvokeAsync(StateHasChanged);
        return task;
    }

    protected virtual void InvokeRefresh()
    {
        _ = InvokeRefreshAsync();
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRefresh) return false;
        _shouldRefresh = false;
        return base.ShouldRender();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        var tcs = _renderTcs;
        if (tcs != null)
        {
            _renderTcs = null;
            tcs.TrySetResult();
        }
    }
}