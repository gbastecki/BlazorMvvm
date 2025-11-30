namespace BlazorMvvm.Demo.Pages
{
    [BlazorMvvmViewModel(ViewModelLifetime.Singleton)]
    public class HomeViewModel : BlazorViewModel
    {
        private int _counter;
        public int Counter
        {
            get => _counter;
            set
            {
                if (_counter == value) return;
                _counter = value;
                base.OnPropertyChanged();
            }
        }

        public IBlazorRelayCommand<bool> IncreaseCounterCommand;
        public HomeViewModel()
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
