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
    }
}
