using BlazorMvvm.Tests.ViewModel;

namespace BlazorMvvm.Tests.Generator
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
    }
}
