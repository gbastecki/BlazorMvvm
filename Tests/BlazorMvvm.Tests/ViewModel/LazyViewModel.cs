namespace BlazorMvvm.Tests.ViewModel
{
    [BlazorMvvmViewModel]
    public class LazyViewModel : BlazorViewModel
    {
        public bool IsInitialized { get; private set; }

        public LazyViewModel()
        {
            IsInitialized = true;
        }
    }
}
