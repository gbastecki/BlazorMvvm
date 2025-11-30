using BlazorMvvm.Tests.ViewModel;

namespace BlazorMvvm.Tests.Components
{
    public class TestComponent : BlazorMvvmComponentBase<TestViewModelWithMultipleConstructors>
    {
        public void SetDependencies(IServiceProvider sp, IBlazorMvvmViewModelFactory factory, BlazorMvvmScopedCache cache)
        {
            this.ServiceProvider = sp;
            this.ViewModelFactory = factory;
            this.ScopedCache = cache;
        }

        public void RunOnInitialized()
        {
            base.OnInitialized();
        }

        public TestViewModelWithMultipleConstructors? GetViewModel()
        {
            return this.BaseViewModel;
        }

        protected override void InvokeRefresh()
        {
        }
    }
}
