using BlazorMvvm.Tests.Components;
using BlazorMvvm.Tests.Services;
using BlazorMvvm.Tests.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMvvm.Tests.Tests
{
    [TestClass]
    public class ViewModelFactoryTests
    {
        [TestMethod]
        public void VerifyMultipleConstructorsResolution()
        {
            ServiceCollection services = new();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<IBlazorMvvmViewModelFactory, BlazorMvvmViewModelFactory>();
            services.AddScoped<BlazorMvvmScopedCache>();

            ServiceProvider sp = services.BuildServiceProvider();
            IBlazorMvvmViewModelFactory factory = sp.GetRequiredService<IBlazorMvvmViewModelFactory>();
            BlazorMvvmScopedCache cache = sp.GetRequiredService<BlazorMvvmScopedCache>();

            TestViewModelWithMultipleConstructors? vm = factory.GetViewModel(typeof(TestViewModelWithMultipleConstructors), sp, cache) as TestViewModelWithMultipleConstructors;

            Assert.IsNotNull(vm, "ViewModel should not be null");
            Assert.AreEqual("Attribute", vm.ConstructorUsed, "Should use constructor marked with [BlazorMvvmViewModelFactoryConstructor]");
            Assert.IsNotNull(vm.ServiceProvider, "ServiceProvider should be injected");
        }

        [TestMethod]
        public void VerifyAutomaticSetDataContext()
        {
            ServiceCollection services = new();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<IBlazorMvvmViewModelFactory, BlazorMvvmViewModelFactory>();
            services.AddScoped<BlazorMvvmScopedCache>();

            ServiceProvider sp = services.BuildServiceProvider();
            IBlazorMvvmViewModelFactory factory = sp.GetRequiredService<IBlazorMvvmViewModelFactory>();
            BlazorMvvmScopedCache cache = sp.GetRequiredService<BlazorMvvmScopedCache>();

            TestComponent component = new();
            component.SetDependencies(sp, factory, cache);

            component.RunOnInitialized();

            TestViewModelWithMultipleConstructors? vm = component.GetViewModel();
            Assert.IsNotNull(vm, "ViewModel should be automatically created and set");
            Assert.AreEqual("Attribute", vm.ConstructorUsed, "Should use correct constructor");
        }
    }
}
