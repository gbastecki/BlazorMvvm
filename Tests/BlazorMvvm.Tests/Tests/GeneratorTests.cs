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
            await vm.AsyncOperationCommand.ExecuteAsync();
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncCommandWithCheckTest()
        {
            TestViewModel vm = new();
            await vm.AsyncOperationWithCheckCommand.ExecuteAsync();
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncValueTaskOperationTest()
        {
            TestViewModel vm = new();
            await vm.AsyncValueTaskOperationCommand.ExecuteAsync();
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncWithSyncCheckTest()
        {
            TestViewModel vm = new();
            Assert.IsTrue(await vm.AsyncWithSyncCheckCommand.CanExecute());
            await vm.AsyncWithSyncCheckCommand.ExecuteAsync();
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task AsyncWithValueTaskCheckTest()
        {
            TestViewModel vm = new();
            Assert.IsTrue(await vm.AsyncWithValueTaskCheckCommand.CanExecute());
            await vm.AsyncWithValueTaskCheckCommand.ExecuteAsync();
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

            Task executionTask = vm.LongRunningOperationCommand.ExecuteAsync();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsTrue(vm.IsLoading);
            Assert.AreEqual(1, vm.LoadingChangedCount);

            await executionTask;

            Assert.IsFalse(vm.IsLoading);
            Assert.AreEqual(2, vm.LoadingChangedCount);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public async Task OnIsExecutingChangedCallbackWithConcurrencyTest()
        {
            TestViewModel vm = new();

            Task executionTask = vm.LongRunningWithConcurrencyCheckCommand.ExecuteAsync();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsTrue(vm.IsLoading);

            await executionTask;

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

            Task executionTask = vm.AutoRefreshOperationCommand.ExecuteAsync();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(1, propertyChangedCount);

            await executionTask;

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

            Task executionTask = vm.CombinedCallbackOperationCommand.ExecuteAsync();
            await Task.Delay(10, TestContext.CancellationToken);

            Assert.IsGreaterThanOrEqualTo(1, propertyChangedCount);
            Assert.AreEqual(1, vm.CombinedCallbackCount);

            await executionTask;

            Assert.IsGreaterThanOrEqualTo(2, propertyChangedCount);
            Assert.AreEqual(2, vm.CombinedCallbackCount);
            Assert.AreEqual(1, vm.Counter);
        }

        [TestMethod]
        public void CollisionViewModelTest()
        {
            var vmA = new BlazorMvvm.Tests.ViewModel.Collision.NamespaceA.CollisionViewModel();
            var vmB = new BlazorMvvm.Tests.ViewModel.Collision.NamespaceB.CollisionViewModel();

            vmA.ValueA = "Test A";
            vmB.ValueB = "Test B";

            Assert.AreEqual("Test A", vmA.ValueA);
            Assert.AreEqual("Test B", vmB.ValueB);
        }

        [TestMethod]
        public async Task ContinueOnCapturedContextTest()
        {
            var oldContext = SynchronizationContext.Current;
            try
            {
                var testContext = new TestSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(testContext);

                bool executed = false;
                var command = new BlazorAsyncCommand(async () =>
                {
                    await Task.Delay(10);
                    executed = true;
                });

                // Test with ContinueOnCapturedContext = true (default)
                await command.ExecuteAsync();
                Assert.IsTrue(executed);
                Assert.IsTrue(testContext.WasPostedTo);

                // Test with ContinueOnCapturedContext = false
                testContext.Reset();
                command.ContinueOnCapturedContext = false;
                await command.ExecuteAsync();
                Assert.IsFalse(testContext.WasPostedTo);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            public bool WasPostedTo { get; private set; }

            public void Reset() => WasPostedTo = false;

            public override void Post(SendOrPostCallback d, object? state)
            {
                WasPostedTo = true;
                base.Post(d, state);
            }
        }

        [TestMethod]
        public void GeneratedCommandContinueOnCapturedContextTest()
        {
            TestViewModel vm = new();
            Assert.IsNotNull(vm.GenerateWithConfigureAwaitFalseCommand);
            Assert.IsFalse(vm.GenerateWithConfigureAwaitFalseCommand.ContinueOnCapturedContext);
        }

        [TestMethod]
        public async Task OnPropertyChangedAsync_AwaitsSubscribers()
        {
            TestViewModel vm = new();
            bool taskCompleted = false;

            vm.OnTriggerRefreshAsync += async (prop) =>
            {
                await Task.Delay(50);
                taskCompleted = true;
            };

            await vm.OnPropertyChangedAsync("Counter");
            Assert.IsTrue(taskCompleted);
        }

        [TestMethod]
        public async Task OnPropertyChangedAsync_ContinueOnCapturedContext()
        {
            TestViewModel vm = new();
            var oldContext = SynchronizationContext.Current;
            try
            {
                var testContext = new TestSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(testContext);

                vm.OnTriggerRefreshAsync += async (prop) =>
                {
                    await Task.Delay(10);
                };

                // Default is true (should capture context)
                await vm.OnPropertyChangedAsync("Counter");
                Assert.IsTrue(testContext.WasPostedTo);

                // If false, should not capture context
                testContext.Reset();
                await vm.OnPropertyChangedAsync("Counter", continueOnCapturedContext: false);
                Assert.IsFalse(testContext.WasPostedTo);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        [TestMethod]
        public async Task DebounceRefresh_PropertySpecific_DebouncesAndResets()
        {
            TestViewModel vm = new();
            int notificationCount = 0;
            vm.OnTriggerRefresh += (prop) =>
            {
                if (prop == "Counter")
                {
                    notificationCount++;
                }
            };

            // Call debounce twice in rapid succession
            vm.DebounceRefresh(50, "Counter");
            await Task.Delay(20);
            vm.DebounceRefresh(50, "Counter"); // should reset the timer

            // Wait 20ms: total 40ms since start, shouldn't have fired yet
            await Task.Delay(20);
            Assert.AreEqual(0, notificationCount);

            // Wait another 40ms: total 80ms since start, 60ms since second call (which had 50ms delay), should have fired once
            await Task.Delay(40);
            Assert.AreEqual(1, notificationCount);
        }

        [TestMethod]
        public async Task DebounceRefresh_FullRefresh_CancelsPropertySpecific()
        {
            TestViewModel vm = new();
            int propertyCount = 0;
            int fullCount = 0;

            vm.OnTriggerRefresh += (prop) =>
            {
                if (prop == "Counter") propertyCount++;
                else if (prop == null) fullCount++;
            };

            // Schedule property debounce
            vm.DebounceRefresh(50, "Counter");

            // Schedule full refresh debounce immediately
            vm.DebounceRefresh(30, null);

            // Wait 70ms to let both timers expire
            await Task.Delay(70);

            // Property debounce should have been cancelled by the full refresh
            Assert.AreEqual(0, propertyCount);
            Assert.AreEqual(1, fullCount);
        }

        [TestMethod]
        public async Task DebounceRefresh_CancelAllDebounces_CancelsAll()
        {
            TestViewModel vm = new();
            int count = 0;
            vm.OnTriggerRefresh += (_) => count++;

            vm.DebounceRefresh(50, "Counter");
            vm.CancelAllDebounces();

            await Task.Delay(70);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task AsyncCommand_ConcurrentExecutions_IsExecutingTrackedCorrectly()
        {
            var tcs1 = new TaskCompletionSource<bool>();
            var tcs2 = new TaskCompletionSource<bool>();
            int executeCount = 0;

            var cmd = new BlazorAsyncCommand(async () =>
            {
                var count = Interlocked.Increment(ref executeCount);
                if (count == 1) await tcs1.Task;
                else if (count == 2) await tcs2.Task;
            }, allowConcurrentExecutions: true);

            var t1 = cmd.ExecuteAsync();
            var t2 = cmd.ExecuteAsync();

            // Both are executing
            Assert.IsTrue(cmd.IsExecuting);

            // Complete first execution
            tcs1.SetResult(true);
            await t1;

            // Should still be executing because second task is still running
            Assert.IsTrue(cmd.IsExecuting);

            // Complete second execution
            tcs2.SetResult(true);
            await t2;

            // Finally false
            Assert.IsFalse(cmd.IsExecuting);
        }
    }
}
