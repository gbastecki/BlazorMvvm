using BlazorMvvm.Tests.ViewModel;

namespace BlazorMvvm.Tests.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void PropertyGenerationTest()
        {
            TestViewModel vm = new()
            {
                Counter = 10,
                Counter2 = 5,
                Counter3 = 15
            };
            Assert.AreEqual(10, vm.Counter);
            Assert.AreEqual(5, vm.Counter2);
            Assert.AreEqual(15, vm.Counter3);

            bool notified = false;
            bool notified2 = false;
            bool notified3 = false;
            vm.OnTriggerRefresh += (prop) =>
            {
                if (prop == nameof(vm.Counter)) notified = true;
                if (prop == nameof(vm.Counter2)) notified2 = true;
                if (prop == nameof(vm.Counter3)) notified3 = true;
            };

            vm.Counter = 20;
            Assert.IsTrue(notified);
            Assert.AreEqual(20, vm.Counter);

            vm.Counter2 = 12;
            Assert.IsTrue(notified2);
            Assert.AreEqual(12, vm.Counter2);

            vm.Counter3 = 2;
            Assert.IsTrue(notified3);
            Assert.AreEqual(2, vm.Counter3);
        }

        [TestMethod]
        public void CustomPropertyNameTest()
        {
            TestViewModel vm = new()
            {
                CustomPropertyName = "Test Value"
            };

            bool notified = false;
            vm.OnTriggerRefresh += (prop) =>
            {
                if (prop == nameof(vm.CustomPropertyName)) notified = true;
            };
            
            vm.CustomPropertyName = "New Value";
            Assert.IsTrue(notified);
            Assert.AreEqual("New Value", vm.CustomPropertyName);
        }

        [TestMethod]
        public void CommandGenerationTest()
        {
            TestViewModel vm = new();

            Assert.IsNotNull(vm.IncrementCounterCommand);
            Assert.IsNotNull(vm.UpdateNameCommand);

            vm.IncrementCounterCommand.Execute();
            Assert.AreEqual(1, vm.Counter);

            vm.UpdateNameCommand.Execute("BlazorMvvm");
            Assert.AreEqual("BlazorMvvm", vm.Name);
        }

        [TestMethod]
        public void CommandWithCanExecuteTest()
        {
            TestViewModel vm = new()
            {
                Counter = 10
            };
            Assert.IsFalse(vm.IncrementWithCheckCommand.CanExecute());

            vm.Counter = 5;
            Assert.IsTrue(vm.IncrementWithCheckCommand.CanExecute());
            vm.IncrementWithCheckCommand.Execute();
            Assert.AreEqual(6, vm.Counter);
        }

        [TestMethod]
        public void CommandWithCanExecuteCtorTest()
        {
            TestViewModel vm = new()
            {
                Counter = 10
            };
            Assert.IsFalse(vm.IncrementWithCheckCtorCommand.CanExecute());

            vm.Counter = 5;
            Assert.IsTrue(vm.IncrementWithCheckCtorCommand.CanExecute());
            vm.IncrementWithCheckCtorCommand.Execute();
            Assert.AreEqual(6, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncCommandTest()
        {
            TestViewModel vm = new();
            vm.AsyncOperationCommand.Execute();
            await Task.Delay(50, TestContext.CancellationToken);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncCommandWithCheckTest()
        {
            TestViewModel vm = new();
            vm.AsyncOperationWithCheckCommand.Execute();
            await Task.Delay(50, TestContext.CancellationToken);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncValueTaskOperationTest()
        {
            TestViewModel vm = new();
            vm.AsyncValueTaskOperationCommand.Execute();
            await Task.Delay(50, TestContext.CancellationToken);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncWithSyncCheckTest()
        {
            TestViewModel vm = new();
            Assert.IsTrue(await vm.AsyncWithSyncCheckCommand.CanExecute());
            vm.AsyncWithSyncCheckCommand.Execute();
            await Task.Delay(50, TestContext.CancellationToken);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncWithValueTaskCheckTest()
        {
            TestViewModel vm = new();
            Assert.IsTrue(await vm.AsyncWithValueTaskCheckCommand.CanExecute());
            vm.AsyncWithValueTaskCheckCommand.Execute();
            await Task.Delay(50, TestContext.CancellationToken);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public void MultipleParamsCommandTest()
        {
            TestViewModel vm = new();
            vm.SumCommand.Execute((5, 3));
            Assert.AreEqual(8, vm.Counter);
        }

        [TestMethod]
        public async Task OnIsExecutingChangedCallbackTest()
        {
            TestViewModel vm = new();
            Assert.IsFalse(vm.IsLoading);
            Assert.AreEqual(0, vm.LoadingChangedCount);

            vm.LongRunningOperationCommand.Execute();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsTrue(vm.IsLoading);
            Assert.AreEqual(1, vm.LoadingChangedCount);

            await Task.Delay(100, TestContext.CancellationToken);

            Assert.IsFalse(vm.IsLoading);
            Assert.AreEqual(2, vm.LoadingChangedCount);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task OnIsExecutingChangedCallbackWithConcurrencyTest()
        {
            TestViewModel vm = new();

            vm.LongRunningWithConcurrencyCheckCommand.Execute();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsTrue(vm.IsLoading);

            await Task.Delay(100, TestContext.CancellationToken);

            Assert.IsFalse(vm.IsLoading);
        }

        [TestMethod]
        public void MessengerRegistration_GeneratesRegisterMethod()
        {
            MessengerTestViewModel vm = new();
            BlazorMessenger messenger = new();

            vm.RegisterMessenger(messenger);

            Assert.IsTrue(messenger.IsRegistered<CounterChangedMessage>(vm));
            Assert.IsTrue(messenger.IsRegistered<NameChangedMessage>(vm));
        }

        [TestMethod]
        public void MessengerRegistration_ReceivesMessages()
        {
            MessengerTestViewModel vm = new();
            BlazorMessenger messenger = new();
            vm.RegisterMessenger(messenger);

            messenger.Send(new CounterChangedMessage(42));
            messenger.Send(new NameChangedMessage("Test"));

            Assert.AreEqual(42, vm.Counter);
            Assert.AreEqual("Test", vm.Name);
            Assert.AreEqual(2, vm.MessageCount);
        }

        [TestMethod]
        public void MessengerRegistration_UnregisterStopsMessages()
        {
            MessengerTestViewModel vm = new();
            BlazorMessenger messenger = new();
            vm.RegisterMessenger(messenger);

            vm.UnregisterMessenger(messenger);
            messenger.Send(new CounterChangedMessage(42));

            Assert.AreEqual(0, vm.Counter);
            Assert.AreEqual(0, vm.MessageCount);
        }

        [TestMethod]
        public async Task AutoRefreshOnIsExecutingChanged_TriggersPropertyChanged()
        {
            TestViewModel vm = new();
            int propertyChangedCount = 0;
            vm.OnTriggerRefresh += _ => propertyChangedCount++;

            vm.AutoRefreshOperationCommand.Execute();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(1, propertyChangedCount);

            await Task.Delay(100, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(2, propertyChangedCount);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AutoRefreshWithCallback_BothAreInvoked()
        {
            TestViewModel vm = new();
            int propertyChangedCount = 0;
            vm.OnTriggerRefresh += _ => propertyChangedCount++;

            Assert.AreEqual(0, vm.CombinedCallbackCount);

            vm.CombinedCallbackOperationCommand.Execute();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(1, propertyChangedCount);
            Assert.AreEqual(1, vm.CombinedCallbackCount);

            await Task.Delay(100, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(2, propertyChangedCount);
            Assert.AreEqual(2, vm.CombinedCallbackCount);
            Assert.AreEqual(1, vm.Counter);
        }
    }
}
