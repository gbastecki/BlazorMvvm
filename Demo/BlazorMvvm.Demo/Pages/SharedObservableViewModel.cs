using BlazorMvvm;

namespace BlazorMvvm.Demo.Pages
{
    public class SharedObservableViewModel : BlazorViewModel
    {
        private int _counter1;
        public int Counter1
        {
            get => _counter1;
            set
            {
                if (_counter1 == value) return;
                _counter1 = value;
                base.OnPropertyChanged();
            }
        }

        private int _counter2;
        public int Counter2
        {
            get => _counter2;
            set
            {
                if (_counter2 == value) return;
                _counter2 = value;
                base.OnPropertyChanged();
            }
        }

        private int _counter3;
        public int Counter3
        {
            get => _counter3;
            set
            {
                if (_counter3 == value) return;
                _counter3 = value;
                base.OnPropertyChanged();
            }
        }

        public IBlazorRelayCommand<bool> IncreaseCounterCommand1;
        public IBlazorRelayCommand<bool> IncreaseCounterCommand2;
        public IBlazorRelayCommand<bool> IncreaseCounterCommand3;
        public SharedObservableViewModel()
        {
            IncreaseCounterCommand1 = new BlazorRelayCommand<bool>(IncreaseCounter1);
            IncreaseCounterCommand2 = new BlazorRelayCommand<bool>(IncreaseCounter2);
            IncreaseCounterCommand3 = new BlazorRelayCommand<bool>(IncreaseCounter3);
        }

        internal void IncreaseCounter1(bool withRefreshing)
        {
            if (withRefreshing) Counter1++;
            else _counter1++;
        }
        internal void IncreaseCounter2(bool withRefreshing)
        {
            if (withRefreshing) Counter2++;
            else _counter2++;
        }
        internal void IncreaseCounter3(bool withRefreshing)
        {
            if (withRefreshing) Counter3++;
            else _counter3++;
        }
    }
}
