using BlazorMvvm.Tests.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMvvm.Tests.Tests
{
    [TestClass]
    public class LazyLoadingTests
    {
        [TestMethod]
        public void VerifyLazyLoadingResolution()
        {
            ServiceCollection services = new();
            services.AddSingleton<IBlazorMvvmViewModelFactory, BlazorMvvmViewModelFactory>();
            services.AddScoped<BlazorMvvmScopedCache>();

            ServiceProvider sp = services.BuildServiceProvider();
            IBlazorMvvmViewModelFactory factory = sp.GetRequiredService<IBlazorMvvmViewModelFactory>();
            BlazorMvvmScopedCache cache = sp.GetRequiredService<BlazorMvvmScopedCache>();

            LazyViewModel? vm = factory.GetViewModel(typeof(LazyViewModel), sp, cache) as LazyViewModel;

            Assert.IsNotNull(vm, "LazyViewModel should be resolved automatically via generated ModuleInitializer");
            Assert.IsTrue(vm.IsInitialized, "ViewModel should be initialized");
        }
    }
}
