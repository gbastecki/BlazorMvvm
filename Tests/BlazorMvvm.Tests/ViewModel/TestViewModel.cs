namespace BlazorMvvm.Tests.ViewModel
{
    public partial class TestViewModel : BlazorViewModel
    {
        [BlazorObservableProperty]
        private int _counter;

        [BlazorObservableProperty]
        private int m_counter2;

        [BlazorObservableProperty]
        private int counter3;

        [BlazorObservableProperty(Name = "CustomPropertyName")]
        private string? _customField;

        [BlazorObservableProperty]
        private string? _name;

        [BlazorCommand]
        private void IncrementCounter()
        {
            Counter++;
        }

        [BlazorCommand]
        private void UpdateName(string newName)
        {
            Name = newName;
        }

        [BlazorCommand(CanExecute = nameof(CanIncrement))]
        private void IncrementWithCheck()
        {
            Counter++;
        }

        [BlazorCommand(nameof(CanIncrement))]
        private void IncrementWithCheckCtor()
        {
            Counter++;
        }

        private bool CanIncrement()
        {
            return Counter < 10;
        }

        [BlazorCommand(AllowConcurrentExecutions = true)]
        private async Task AsyncOperation()
        {
            await Task.Delay(10);
            Counter++;
        }

        [BlazorCommand(nameof(CanExecuteAsync), AllowConcurrentExecutions = false)]
        private async Task AsyncOperationWithCheck()
        {
            await Task.Delay(10);
            Counter++;
        }
        private static Task<bool> CanExecuteAsync() => Task.FromResult(true);

        [BlazorCommand]
        private async ValueTask AsyncValueTaskOperation()
        {
            await Task.Delay(10);
            Counter++;
        }

        [BlazorCommand(CanExecute = nameof(CanExecuteSync))]
        private async Task AsyncWithSyncCheck()
        {
            await Task.Delay(10);
            Counter++;
        }
        private static bool CanExecuteSync() => true;

        [BlazorCommand(CanExecute = nameof(CanExecuteValueTask))]
        private async Task AsyncWithValueTaskCheck()
        {
            await Task.Delay(10);
            Counter++;
        }
        private static async ValueTask<bool> CanExecuteValueTask()
        {
            await Task.Delay(1);
            return true;
        }

        [BlazorCommand]
        private void Sum(int a, int b)
        {
            Counter = a + b;
        }

        [BlazorObservableProperty]
        private bool _isLoading;

        [BlazorObservableProperty]
        private int _loadingChangedCount;

        [BlazorCommand(OnIsExecutingChangedCallback = nameof(OnLoadingChanged))]
        private async Task LongRunningOperation()
        {
            await Task.Delay(50);
            Counter++;
        }

        private void OnLoadingChanged(bool isExecuting)
        {
            IsLoading = isExecuting;
            LoadingChangedCount++;
        }

        [BlazorCommand(OnIsExecutingChangedCallback = nameof(OnLoadingChanged), AllowConcurrentExecutions = false)]
        private async Task LongRunningWithConcurrencyCheck()
        {
            await Task.Delay(50);
            Counter++;
        }

        [BlazorObservableProperty]
        private int _autoRefreshCount;

        [BlazorCommand(autoRefreshOnIsExecutingChanged: true)]
        private async Task AutoRefreshOperation()
        {
            await Task.Delay(50);
            Counter++;
        }

        [BlazorObservableProperty]
        private int _combinedCallbackCount;

        [BlazorCommand(autoRefreshOnIsExecutingChanged: true, OnIsExecutingChangedCallback = nameof(OnCombinedCallback))]
        private async Task CombinedCallbackOperation()
        {
            await Task.Delay(50);
            Counter++;
        }

        private void OnCombinedCallback(bool isExecuting)
        {
            CombinedCallbackCount++;
        }

        [BlazorCommand(ContinueOnCapturedContext = false)]
        private async Task GenerateWithConfigureAwaitFalse()
        {
            await Task.Delay(10);
        }
    }
}
