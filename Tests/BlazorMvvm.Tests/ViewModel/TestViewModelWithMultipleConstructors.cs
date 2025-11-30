namespace BlazorMvvm.Tests.ViewModel
{
    [BlazorMvvmViewModel]
    public class TestViewModelWithMultipleConstructors : BlazorViewModel
    {
        public string ConstructorUsed { get; private set; } = string.Empty;
        public IServiceProvider? ServiceProvider { get; private set; }
        public Services.ITestService? TestService { get; private set; }

        public TestViewModelWithMultipleConstructors()
        {
            ConstructorUsed = "Default";
        }

        public TestViewModelWithMultipleConstructors(IServiceProvider serviceProvider)
        {
            ConstructorUsed = "ServiceProvider";
            ServiceProvider = serviceProvider;
        }

        [BlazorMvvmViewModelFactoryConstructor]
        public TestViewModelWithMultipleConstructors(IServiceProvider serviceProvider, Services.ITestService testService)
        {
            ConstructorUsed = "Attribute";
            ServiceProvider = serviceProvider;
            TestService = testService;
        }

        public void OnDispose() { }
    }
}
