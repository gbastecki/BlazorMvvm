namespace BlazorMvvm.Tests.Tests
{
    [TestClass]
    public class BlazorMessengerTests
    {
        public TestContext TestContext { get; set; }
        [TestCleanup]
        public void Cleanup()
        {
            // Reset the default messenger for each test
            typeof(BlazorMessenger)
                .GetMethod("ResetDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(null, null);
            typeof(BlazorStrongMessenger)
                .GetMethod("ResetDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(null, null);
        }
        #region Basic Send/Receive Tests
        [TestMethod]
        public void SendReceive_BasicMessage_ReceivesMessage()
        {
            BlazorMessenger messenger = new();
            string? receivedValue = null;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receivedValue = m.Value);
            messenger.Send(new TestMessage { Value = "Hello" });
            Assert.AreEqual("Hello", receivedValue);
        }
        [TestMethod]
        public void SendReceive_MultipleRecipients_AllReceive()
        {
            BlazorMessenger messenger = new();
            List<string> receivedValues = new();
            object recipient1 = new();
            object recipient2 = new();
            object recipient3 = new();
            messenger.Register<TestMessage>(recipient1, (r, m) => receivedValues.Add($"1:{m.Value}"));
            messenger.Register<TestMessage>(recipient2, (r, m) => receivedValues.Add($"2:{m.Value}"));
            messenger.Register<TestMessage>(recipient3, (r, m) => receivedValues.Add($"3:{m.Value}"));
            messenger.Send(new TestMessage { Value = "Test" });
            Assert.HasCount(3, receivedValues);
            CollectionAssert.Contains(receivedValues, "1:Test");
            CollectionAssert.Contains(receivedValues, "2:Test");
            CollectionAssert.Contains(receivedValues, "3:Test");
        }
        [TestMethod]
        public void SendReceive_DifferentMessageTypes_OnlyMatchingReceives()
        {
            BlazorMessenger messenger = new();
            bool receivedTestMessage = false;
            bool receivedOtherMessage = false;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receivedTestMessage = true);
            messenger.Register<OtherMessage>(recipient, (r, m) => receivedOtherMessage = true);
            messenger.Send(new TestMessage { Value = "Test" });
            Assert.IsTrue(receivedTestMessage);
            Assert.IsFalse(receivedOtherMessage);
        }
        #endregion
        #region Channel Token Tests
        [TestMethod]
        public void Send_WithToken_OnlyMatchingChannelReceives()
        {
            BlazorMessenger messenger = new();
            string? channel1Received = null;
            string? channel2Received = null;
            object recipient1 = new();
            object recipient2 = new();
            messenger.Register<TestMessage, int>(recipient1, 1, (r, m) => channel1Received = m.Value);
            messenger.Register<TestMessage, int>(recipient2, 2, (r, m) => channel2Received = m.Value);
            messenger.Send(new TestMessage { Value = "ToChannel1" }, 1);
            Assert.AreEqual("ToChannel1", channel1Received);
            Assert.IsNull(channel2Received);
        }
        [TestMethod]
        public void Send_WithStringToken_ReceivesMessage()
        {
            BlazorMessenger messenger = new();
            string? receivedValue = null;
            object recipient = new();
            messenger.Register<TestMessage, string>(recipient, "channel-a", (r, m) => receivedValue = m.Value);
            messenger.Send(new TestMessage { Value = "Hello" }, "channel-a");
            Assert.AreEqual("Hello", receivedValue);
        }
        [TestMethod]
        public void Send_WithEnumAsIntToken_OnlyMatchingChannelReceives()
        {
            BlazorMessenger messenger = new();
            string? systemReceived = null;
            string? userReceived = null;
            object recipient1 = new();
            object recipient2 = new();
            messenger.Register<TestMessage, int>(recipient1, (int)TestChannel.System, (r, m) => systemReceived = m.Value);
            messenger.Register<TestMessage, int>(recipient2, (int)TestChannel.User, (r, m) => userReceived = m.Value);
            messenger.Send(new TestMessage { Value = "SystemMessage" }, (int)TestChannel.System);
            Assert.AreEqual("SystemMessage", systemReceived);
            Assert.IsNull(userReceived);
        }
        [TestMethod]
        public void Unregister_WithToken_NoLongerReceivesOnChannel()
        {
            BlazorMessenger messenger = new();
            int receiveCount = 0;
            object recipient = new();
            messenger.Register<TestMessage, string>(recipient, "channel-a", (r, m) => receiveCount++);
            messenger.Send(new TestMessage { Value = "1" }, "channel-a");
            messenger.Unregister<TestMessage, string>(recipient, "channel-a");
            messenger.Send(new TestMessage { Value = "2" }, "channel-a");
            Assert.AreEqual(1, receiveCount);
        }
        [TestMethod]
        public void Register_MultipleChannels_ReceivesFromMatchingOnly()
        {
            BlazorMessenger messenger = new();
            List<string> receivedChannels = new();
            object recipient = new();
            messenger.Register<TestMessage, string>(recipient, "channel-a", (r, m) => receivedChannels.Add($"A:{m.Value}"));
            messenger.Register<TestMessage, string>(recipient, "channel-b", (r, m) => receivedChannels.Add($"B:{m.Value}"));
            messenger.Send(new TestMessage { Value = "Test" }, "channel-a");
            messenger.Send(new TestMessage { Value = "Test" }, "channel-b");
            messenger.Send(new TestMessage { Value = "Test" }, "channel-c");
            Assert.HasCount(2, receivedChannels);
            CollectionAssert.Contains(receivedChannels, "A:Test");
            CollectionAssert.Contains(receivedChannels, "B:Test");
        }
        [TestMethod]
        public void Unregister_OneChannel_StillReceivesFromOther()
        {
            BlazorMessenger messenger = new();
            List<string> receivedChannels = new();
            object recipient = new();
            messenger.Register<TestMessage, string>(recipient, "channel-a", (r, m) => receivedChannels.Add("A"));
            messenger.Register<TestMessage, string>(recipient, "channel-b", (r, m) => receivedChannels.Add("B"));
            messenger.Unregister<TestMessage, string>(recipient, "channel-a");
            messenger.Send(new TestMessage { Value = "Test" }, "channel-a");
            messenger.Send(new TestMessage { Value = "Test" }, "channel-b");
            Assert.HasCount(1, receivedChannels);
            CollectionAssert.Contains(receivedChannels, "B");
        }
        [TestMethod]
        public void Send_DefaultChannel_DoesNotReceiveTokenedMessages()
        {
            BlazorMessenger messenger = new();
            string? defaultReceived = null;
            string? tokenedReceived = null;
            object recipient1 = new();
            object recipient2 = new();
            // Register on default (no token)
            messenger.Register<TestMessage>(recipient1, (r, m) => defaultReceived = m.Value);
            // Register on specific token
            messenger.Register<TestMessage, int>(recipient2, 42, (r, m) => tokenedReceived = m.Value);
            // Send to default channel
            messenger.Send(new TestMessage { Value = "Default" });
            // Send to tokened channel
            messenger.Send(new TestMessage { Value = "Tokened" }, 42);
            Assert.AreEqual("Default", defaultReceived);
            Assert.AreEqual("Tokened", tokenedReceived);
        }
        #endregion
        #region Unregister Tests
        [TestMethod]
        public void Unregister_SingleMessageType_NoLongerReceives()
        {
            BlazorMessenger messenger = new();
            int receiveCount = 0;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receiveCount++);
            messenger.Send(new TestMessage { Value = "1" });
            messenger.Unregister<TestMessage>(recipient);
            messenger.Send(new TestMessage { Value = "2" });
            Assert.AreEqual(1, receiveCount);
        }
        [TestMethod]
        public void UnregisterAll_MultipleMessageTypes_NoneReceive()
        {
            BlazorMessenger messenger = new();
            bool receivedTest = false;
            bool receivedOther = false;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receivedTest = true);
            messenger.Register<OtherMessage>(recipient, (r, m) => receivedOther = true);
            messenger.UnregisterAll(recipient);
            messenger.Send(new TestMessage { Value = "Test" });
            messenger.Send(new OtherMessage { Data = 42 });
            Assert.IsFalse(receivedTest);
            Assert.IsFalse(receivedOther);
        }
        #endregion
        #region IsRegistered Tests
        [TestMethod]
        public void IsRegistered_RegisteredRecipient_ReturnsTrue()
        {
            BlazorMessenger messenger = new();
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => { });
            Assert.IsTrue(messenger.IsRegistered<TestMessage>(recipient));
        }
        [TestMethod]
        public void IsRegistered_NotRegistered_ReturnsFalse()
        {
            BlazorMessenger messenger = new();
            object recipient = new();
            Assert.IsFalse(messenger.IsRegistered<TestMessage>(recipient));
        }
        [TestMethod]
        public void IsRegistered_DifferentMessageType_ReturnsFalse()
        {
            BlazorMessenger messenger = new();
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => { });
            Assert.IsTrue(messenger.IsRegistered<TestMessage>(recipient));
            Assert.IsFalse(messenger.IsRegistered<OtherMessage>(recipient));
        }
        #endregion
        #region Default Instance Tests
        [TestMethod]
        public void Default_ReturnsSharedInstance()
        {
            BlazorMessenger instance1 = BlazorMessenger.Default;
            BlazorMessenger instance2 = BlazorMessenger.Default;
            Assert.AreSame(instance1, instance2);
        }
        [TestMethod]
        public void StrongMessenger_Default_ReturnsSharedInstance()
        {
            BlazorStrongMessenger instance1 = BlazorStrongMessenger.Default;
            BlazorStrongMessenger instance2 = BlazorStrongMessenger.Default;
            Assert.AreSame(instance1, instance2);
        }
        #endregion
        #region Strong Messenger Tests
        [TestMethod]
        public void StrongMessenger_SendReceive_BasicMessage()
        {
            BlazorStrongMessenger messenger = new();
            string? receivedValue = null;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receivedValue = m.Value);
            messenger.Send(new TestMessage { Value = "StrongHello" });
            Assert.AreEqual("StrongHello", receivedValue);
        }
        [TestMethod]
        public void StrongMessenger_Unregister_NoLongerReceives()
        {
            BlazorStrongMessenger messenger = new();
            int receiveCount = 0;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receiveCount++);
            messenger.Send(new TestMessage { Value = "1" });
            messenger.Unregister<TestMessage>(recipient);
            messenger.Send(new TestMessage { Value = "2" });
            Assert.AreEqual(1, receiveCount);
        }
        [TestMethod]
        public void StrongMessenger_MultipleRecipients_AllReceive()
        {
            BlazorStrongMessenger messenger = new();
            List<string> receivedValues = new();
            object recipient1 = new();
            object recipient2 = new();
            object recipient3 = new();
            messenger.Register<TestMessage>(recipient1, (r, m) => receivedValues.Add($"1:{m.Value}"));
            messenger.Register<TestMessage>(recipient2, (r, m) => receivedValues.Add($"2:{m.Value}"));
            messenger.Register<TestMessage>(recipient3, (r, m) => receivedValues.Add($"3:{m.Value}"));
            messenger.Send(new TestMessage { Value = "Test" });
            Assert.HasCount(3, receivedValues);
            CollectionAssert.Contains(receivedValues, "1:Test");
            CollectionAssert.Contains(receivedValues, "2:Test");
            CollectionAssert.Contains(receivedValues, "3:Test");
        }
        [TestMethod]
        public void StrongMessenger_UnregisterAll_NoneReceive()
        {
            BlazorStrongMessenger messenger = new();
            bool receivedTest = false;
            bool receivedOther = false;
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => receivedTest = true);
            messenger.Register<OtherMessage>(recipient, (r, m) => receivedOther = true);
            messenger.UnregisterAll(recipient);
            messenger.Send(new TestMessage { Value = "Test" });
            messenger.Send(new OtherMessage { Data = 42 });
            Assert.IsFalse(receivedTest);
            Assert.IsFalse(receivedOther);
        }
        [TestMethod]
        public void StrongMessenger_IsRegistered_ReturnsTrue()
        {
            BlazorStrongMessenger messenger = new();
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => { });
            Assert.IsTrue(messenger.IsRegistered<TestMessage>(recipient));
        }
        [TestMethod]
        public void StrongMessenger_IsRegistered_AfterUnregister_ReturnsFalse()
        {
            BlazorStrongMessenger messenger = new();
            object recipient = new();
            messenger.Register<TestMessage>(recipient, (r, m) => { });
            messenger.Unregister<TestMessage>(recipient);
            Assert.IsFalse(messenger.IsRegistered<TestMessage>(recipient));
        }
        [TestMethod]
        public void StrongMessenger_WithChannelToken_OnlyMatchingReceives()
        {
            BlazorStrongMessenger messenger = new();
            string? channel1Received = null;
            string? channel2Received = null;
            object recipient1 = new();
            object recipient2 = new();
            messenger.Register<TestMessage, int>(recipient1, 1, (r, m) => channel1Received = m.Value);
            messenger.Register<TestMessage, int>(recipient2, 2, (r, m) => channel2Received = m.Value);
            messenger.Send(new TestMessage { Value = "ToChannel1" }, 1);
            Assert.AreEqual("ToChannel1", channel1Received);
            Assert.IsNull(channel2Received);
        }
        [TestMethod]
        public void StrongMessenger_RecipientInterface_ReceivesMessage()
        {
            BlazorStrongMessenger messenger = new();
            TestRecipient recipient = new();
            messenger.Register(recipient);
            messenger.Send(new TestMessage { Value = "StrongRecipient" });
            Assert.AreEqual("StrongRecipient", recipient.LastReceivedValue);
        }
        #endregion
        #region IBlazorRecipient Tests
        [TestMethod]
        public void RecipientInterface_ReceivesMessage()
        {
            BlazorMessenger messenger = new();
            TestRecipient recipient = new();
            messenger.Register(recipient);
            messenger.Send(new TestMessage { Value = "ReceivedViaInterface" });
            Assert.AreEqual("ReceivedViaInterface", recipient.LastReceivedValue);
        }
        #endregion
        #region ValueChangedMessage Tests
        [TestMethod]
        public void ValueChangedMessage_ContainsValue()
        {
            BlazorMessenger messenger = new();
            int? receivedValue = null;
            object recipient = new();
            messenger.Register<ValueChangedMessage<int>>(recipient, (r, m) => receivedValue = m.Value);
            messenger.Send(new ValueChangedMessage<int>(42));
            Assert.AreEqual(42, receivedValue);
        }
        #endregion
        #region RequestMessage Tests
        [TestMethod]
        public void RequestMessage_ReceivesReply()
        {
            BlazorMessenger messenger = new();
            object recipient = new();
            messenger.Register<UserRequestMessage>(recipient, (r, m) =>
            {
                m.Reply("John Doe");
            });
            UserRequestMessage request = messenger.Send(new UserRequestMessage());
            Assert.IsTrue(request.HasReceivedResponse);
            Assert.AreEqual("John Doe", request.Response);
        }
        [TestMethod]
        public void RequestMessage_ImplicitConversion()
        {
            BlazorMessenger messenger = new();
            object recipient = new();
            messenger.Register<UserRequestMessage>(recipient, (r, m) => m.Reply("Jane Doe"));
            string response = messenger.Send(new UserRequestMessage());
            Assert.AreEqual("Jane Doe", response);
        }
        [TestMethod]
        public void RequestMessage_NoReply_ThrowsOnAccess()
        {
            UserRequestMessage request = new();
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = request.Response);
        }
        #endregion
        #region Test Message Classes
        private class TestMessage
        {
            public string Value { get; set; } = "";
        }
        private class OtherMessage
        {
            public int Data { get; set; }
        }
        private class TestRecipient : IBlazorRecipient<TestMessage>
        {
            public string? LastReceivedValue { get; private set; }
            public void Receive(TestMessage message)
            {
                LastReceivedValue = message.Value;
            }
        }
        private class UserRequestMessage : RequestMessage<string>
        {
        }
        private enum TestChannel
        {
            System,
            User,
            Debug
        }
        #endregion
    }
}