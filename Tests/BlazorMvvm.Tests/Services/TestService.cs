namespace BlazorMvvm.Tests.Services
{
    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "TestValue";
    }
}
