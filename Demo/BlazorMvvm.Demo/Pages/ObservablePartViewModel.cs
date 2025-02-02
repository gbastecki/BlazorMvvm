using BlazorMvvm;

namespace BlazorMvvm.Demo.Pages
{
    public class ObservablePartViewModel : BlazorViewModel
    {
        private int _counter;
        public int Counter
        {
            get => _counter;
            set => Set(ref _counter, value);
        }

        public IBlazorRelayCommand<bool> IncreaseCounterCommand;
        public ObservablePartViewModel()
        {
            IncreaseCounterCommand = new BlazorRelayCommand<bool>(IncreaseCounter);
        }

        private void IncreaseCounter(bool withRefreshing)
        {
            if (withRefreshing) Counter++;
            else _counter++;
        }
    }
}
