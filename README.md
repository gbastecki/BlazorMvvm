# BlazorMvvm
![https://github.com/github/docs/actions/workflows/main.yml](https://github.com/gbastecki/BlazorMvvm/actions/workflows/build.yml/badge.svg)
[![GitHub](https://img.shields.io/github/license/gbastecki/BlazorMvvm?color=0000a4&style=plastic)](https://github.com/gbastecki/BlazorMvvm/blob/main/LICENSE)
[![NuGet version](https://img.shields.io/nuget/v/gbastecki.BlazorMvvm?color=6a00d5&label=nuget%20version&logo=nuget&style=plastic)](https://www.nuget.org/packages/gbastecki.BlazorMvvm/)
[![NuGet downloads](https://img.shields.io/nuget/dt/gbastecki.BlazorMvvm?color=6a00d5&label=nuget%20downloads&logo=nuget&style=plastic)](https://www.nuget.org/packages/gbastecki.BlazorMvvm/)

A lightweight, AOT-compatible MVVM toolkit for Blazor with powerful source generation capabilities.

[NuGet Package](https://www.nuget.org/packages/gbastecki.BlazorMvvm) | [Live Demo](https://gbastecki.github.io/BlazorMvvm/)

---

## Table of Contents

- [Package requirements](#package-requirements)
- [Quick Start](#quick-start)
- [Core Features](#core-features)
  - [Observable Properties](#observable-properties)
  - [Commands](#commands)
  - [ObservableComponent](#observablecomponent)
  - [BlazorMessenger](#blazormessenger)
  - [ViewModelFactory](#viewmodelfactory)
- [Key Highlights](#key-highlights)
- [Command Attribute Quick Reference](#command-attribute-quick-reference)
- [License](#license)
- [Third-Party Libraries](#third-party-libraries)
- [See it in action](#see-it-in-action)

---

## Package requirements
This package uses [`Microsoft.AspNetCore.Components.Web`](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.Web/) package. The required version depends on your target .NET version:

| .NET Version | Required Package |
|--------------|------------------|
| .NET 10.0 | `Microsoft.AspNetCore.Components.Web` ≥ 10.0.0 |
| .NET 9.0 | `Microsoft.AspNetCore.Components.Web` ≥ 9.0.0 |
| .NET 8.0 | `Microsoft.AspNetCore.Components.Web` ≥ 8.0.0 |
| .NET 7.0 | `Microsoft.AspNetCore.Components.Web` ≥ 7.0.0 |
| .NET 6.0 | `Microsoft.AspNetCore.Components.Web` ≥ 6.0.0 |

---

## Quick Start

### 1. Install the package
```shell
dotnet add package gbastecki.BlazorMvvm
```

### 2. Add package reference

```xml
<PackageReference Include="gbastecki.BlazorMvvm" Version="*" />
```

### 3. Register ViewModelFactory (optional, for ViewModel DI)
```csharp
// Program.cs
builder.Services.UseBlazorMvvmViewModelFactory();
```

### 4. Create a ViewModel
```csharp
[BlazorMvvmViewModel(ViewModelLifetime.Transient)]
public partial class CounterViewModel : BlazorViewModel
{
    [BlazorObservableProperty]
    private int _count;

    [BlazorCommand]
    private void Increment() => Count++;
}
```

### 5. Create a Component
```razor
@inherits BlazorMvvmComponentBase<CounterViewModel>
@page "/counter"

<h1>Count: @BaseViewModel.Count</h1>
<button @onclick="BaseViewModel.IncrementCommand.Execute">+1</button>
```

---

## Core Features

### Observable Properties

Convert private fields into observable properties with automatic change notification.

#### Source Generation (Recommended)
```csharp
public partial class MyViewModel : BlazorViewModel
{
    [BlazorObservableProperty]
    private string _name;
    
    [BlazorObservableProperty]
    private int _counter;
    
    [BlazorObservableProperty(Name = "CustomName")]
    private string _internalField;
}
```

**Generates:**
```csharp
public string Name
{
    get => _name;
    set => Set(ref _name, value);
}

public int Counter
{
    get => _counter;
    set => Set(ref _counter, value);
}

public string CustomName
{
    get => _internalField;
    set => Set(ref _internalField, value);
}
```

#### Manual Implementation
```csharp
public class MyViewModel : BlazorViewModel
{
    private int _counter;
    public int Counter
    {
        get => _counter;
        set => Set(ref _counter, value); // Automatically notifies UI
        
        // or use manual setter:
        //set
        //{
        //    if (_counter == value) return;
        //    _counter = value;
        //    OnPropertyChanged(nameof(Counter));
        //}
    }
}
```

---

### Commands

Bind UI actions to ViewModel methods with built-in execution state management.

#### Synchronous Commands
```csharp
[BlazorCommand]
private void Save() => SaveData();

// Generated: public IBlazorCommand SaveCommand { get; }
```

#### Async Commands
```csharp
[BlazorCommand]
private async Task LoadDataAsync()
{
    await Task.Delay(1000);
}

// Generated: public IBlazorAsyncCommand LoadDataAsyncCommand { get; }
```

#### Commands with Parameters
```csharp
// Single parameter
[BlazorCommand]
private void UpdateName(string name) => Name = name;
// Generated: public IBlazorRelayCommand<string> UpdateNameCommand { get; }

// Multiple parameters (uses tuple)
[BlazorCommand]
private void Add(int a, int b) => Total = a + b;
// Generated: public IBlazorRelayCommand<(int, int)> AddCommand { get; }
// Usage: AddCommand.Execute((5, 10))
```

#### CanExecute Logic
```csharp
[BlazorCommand(CanExecute = nameof(CanSave))]
private void Save() => SaveData();

private bool CanSave() => !string.IsNullOrEmpty(Name);
```

#### Concurrency Control
```csharp
[BlazorCommand(AllowConcurrentExecutions = false)]
private async Task SubmitAsync()
{
    // Prevents multiple concurrent executions
}
```

#### Auto-Refresh on IsExecuting Changed
Automatically trigger UI refresh when `IsExecuting` state changes

```csharp
[BlazorCommand(autoRefreshOnIsExecutingChanged: true)]
private async Task LoadDataAsync()
{
    await Task.Delay(5000);
}
```

```razor
<button @onclick="ViewModel.LoadDataAsyncCommand.Execute"
        disabled="@ViewModel.LoadDataAsyncCommand.IsExecuting">
    @if (ViewModel.LoadDataAsyncCommand.IsExecuting)
    {
        <span>Loading...</span>
    }
    else
    {
        <span>Loaded Data</span>
    }
</button>
```

#### Custom Callback on IsExecuting Changed
```csharp
[BlazorObservableProperty]
private bool _isLoading;

[BlazorCommand(OnIsExecutingChangedCallback = nameof(OnLoadingChanged))]
private async Task SaveAsync() => await SaveDataAsync();

private void OnLoadingChanged(bool isExecuting)
{
    IsLoading = isExecuting;
    // Additional logic here
}
```

#### Combined: Auto-Refresh + Custom Callback
```csharp
[BlazorCommand(autoRefreshOnIsExecutingChanged: true, OnIsExecutingChangedCallback = nameof(OnSaving))]
private async Task SaveAsync() => await SaveDataAsync();

private void OnSaving(bool isExecuting)
{
    // Custom logic runs AFTER OnPropertyChanged() is called
    _logger.Log($"Saving state: {isExecuting}");
}
```

---

### ObservableComponent

Optimize UI updates by isolating which parts of your component re-render.

#### Full Update Mode
Re-renders when ANY property on the ViewModel changes:
```razor
<ObservableComponent ViewModel="MyViewModel">
    <p>Name: @MyViewModel.Name</p>
    <p>Count: @MyViewModel.Count</p>
</ObservableComponent>
```

#### Selective Update Mode
Re-renders ONLY when specified properties change:
```razor
<!-- Only updates when Counter1 changes -->
<ObservableComponent ViewModel="SharedVM" 
    PropertyNames="[nameof(SharedVM.Counter1)]">
    <p>Counter1: @SharedVM.Counter1</p>
</ObservableComponent>

<!-- Only updates when Counter2 or Counter3 changes -->
<ObservableComponent ViewModel="SharedVM" 
    PropertyNames="[nameof(SharedVM.Counter2), nameof(SharedVM.Counter3)]">
    <p>Counter2: @SharedVM.Counter2</p>
    <p>Counter3: @SharedVM.Counter3</p>
</ObservableComponent>
```

---

### BlazorMessenger

Cross-ViewModel communication with weak reference support for automatic cleanup.

#### Messenger Setup

Choose between Dependency Injection or static singleton access:

```csharp
// Option 1: Dependency Injection (Recommended)
// Program.cs
builder.Services.AddSingleton<IBlazorMessenger, BlazorMessenger>();

// ViewModel - Inject via constructor
public class MyViewModel : BlazorViewModel
{
    private readonly IBlazorMessenger _messenger;
    
    public MyViewModel(IBlazorMessenger messenger)
    {
        _messenger = messenger;
    }
}

// Option 2: Static Singleton
// No registration needed - access directly
BlazorMessenger.Default.Send(new MyMessage());
```

#### Define Messages
```csharp
// Simple value message
public class CounterChangedMessage : ValueChangedMessage<int>
{
    public CounterChangedMessage(int value) : base(value) { }
}

// Request/Response message
public class GetUserRequest : RequestMessage<User> { }
```

#### Send Messages
```csharp
// Via injected messenger
_messenger.Send(new CounterChangedMessage(42));

// Via static default instance
BlazorMessenger.Default.Send(new CounterChangedMessage(42));
```

#### Complete Receiver Pattern (Recommended)

Register in constructor, unregister in Dispose to prevent memory leaks:

```csharp
[BlazorMessenger]
public partial class ReceiverViewModel : BlazorViewModel,
    IBlazorRecipient<CounterChangedMessage>, IDisposable
{
    private readonly IBlazorMessenger _messenger;
    
    [BlazorObservableProperty]
    private int _counter;

    // Register in constructor - starts listening
    public ReceiverViewModel(IBlazorMessenger messenger)
    {
        _messenger = messenger;
        RegisterMessenger(_messenger); // Generated method
    }

    // Unregister in Dispose - stops listening, prevents leaks
    public void Dispose()
    {
        UnregisterMessenger(_messenger); // Generated method
    }

    public void Receive(CounterChangedMessage message)
    {
        Counter = message.Value;
    }
}
```

#### Manual Registration (Without Source Generation)
```csharp
public class ReceiverViewModel : BlazorViewModel,
    IBlazorRecipient<CounterChangedMessage>, IDisposable
{
    private readonly IBlazorMessenger _messenger;

    public ReceiverViewModel(IBlazorMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<CounterChangedMessage>(this);
    }

    public void Dispose()
    {
        _messenger.Unregister<CounterChangedMessage>(this);
    }

    public void Receive(CounterChangedMessage message) { }
}
```

#### Strong vs Weak Reference Messenger

| Feature | `BlazorMessenger` | `BlazorStrongMessenger` |
|---------|-------------------|------------------------|
| Reference Type | Weak | Strong |
| GC Behavior | Recipients auto-collected | Recipients kept alive |
| Memory Leaks | Forgiving if you forget Dispose | Must unregister manually |
| Use Case | Most scenarios (default) | Guaranteed delivery |

```csharp
// Use weak messenger (default) - safer for most cases
builder.Services.AddSingleton<IBlazorMessenger, BlazorMessenger>();

// Use strong messenger - guaranteed delivery, explicit lifecycle
builder.Services.AddSingleton<IBlazorMessenger, BlazorStrongMessenger>();
```

#### Channel Tokens

Isolate communication using channel tokens (any `IEquatable<T>` type):

##### String Channels
```csharp
// Register on specific channel
messenger.Register<MyMessage, string>(this, "channel-a",
    (r, m) => ((MyReceiver)r).OnMessage(m));

// Send to that channel only
messenger.Send(new MyMessage(), "channel-a");

// Unregister from channel
messenger.Unregister<MyMessage, string>(this, "channel-a");
```

##### Enum Channels (via int)
```csharp
// Note: Enums don't implement IEquatable<T>, so cast to int
public enum NotificationChannel { System = 0, User = 1, Debug = 2 }

// Register on channel
messenger.Register<LogMessage, int>(this, (int)NotificationChannel.Debug,
    (r, m) => ((LogReceiver)r).OnLog(m));

// Send to channel
messenger.Send(new LogMessage("msg"), (int)NotificationChannel.Debug);
```

#### Request/Response Pattern
```csharp
// Register handler
messenger.Register<GetUserRequest>(this, (r, m) =>
{
    m.Reply(new User("John"));
});

// Send and get response
User user = messenger.Send(new GetUserRequest());
```

---

### ViewModelFactory

Automatic dependency injection for ViewModels with configurable lifetimes.

#### Setup
```csharp
// Program.cs
builder.Services.UseBlazorMvvmViewModelFactory();
```

#### Register ViewModels
```csharp
[BlazorMvvmViewModel(ViewModelLifetime.Singleton)]
public partial class AppViewModel : BlazorViewModel { }

[BlazorMvvmViewModel(ViewModelLifetime.Scoped)]
public partial class PageViewModel : BlazorViewModel { }

[BlazorMvvmViewModel(ViewModelLifetime.Transient)]
public partial class DialogViewModel : BlazorViewModel { }
```

#### Constructor Injection
```csharp
[BlazorMvvmViewModel(ViewModelLifetime.Scoped)]
public partial class HomeViewModel : BlazorViewModel
{
    private readonly IApiService _api;
    private readonly ILogger _logger;

    public HomeViewModel(IApiService api, ILogger logger)
    {
        _api = api;
        _logger = logger;
    }
}
```

#### Constructor Selection
```csharp
[BlazorMvvmViewModel(ViewModelLifetime.Scoped)]
public partial class MyViewModel : BlazorViewModel
{
    public MyViewModel() { }

    [BlazorMvvmViewModelFactoryConstructor] // Use this constructor
    public MyViewModel(IService service) { }
}
```

#### Component Usage
```csharp
public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
{
    // ViewModel is automatically resolved and injected via BaseViewModel
    // No need for manual SetDataContext()
    protected override void OnInitialized()
    {
        base.OnInitialized();
        // BaseViewModel is ready to use!
    }
}
```

---

## Key Highlights

| Feature | Benefit |
|---------|---------|
| **Source Generation** | Zero runtime reflection, AOT-ready |
| **Trimming Safe** | Works with .NET trimmer |
| **Weak References** | Automatic memory cleanup in messenger |
| **Selective Updates** | Fine-grained UI re-rendering |
| **Auto-Refresh** | No boilerplate for loading states |
| **Multiple Parameters** | Tuple support for complex commands |
| **Channel Tokens** | Isolated message channels |
| **Request/Response** | Synchronous messaging pattern |

---

## Command Attribute Quick Reference

```csharp
[BlazorCommand]                                    // Basic command
[BlazorCommand(CanExecute = nameof(CanExecute))]   // With validation
[BlazorCommand(AllowConcurrentExecutions = true)]  // Allow parallel execution
[BlazorCommand(autoRefreshOnIsExecutingChanged: true)]  // Auto UI refresh
[BlazorCommand(OnIsExecutingChangedCallback = nameof(Callback))]  // Custom callback
[BlazorCommand(autoRefreshOnIsExecutingChanged: true, OnIsExecutingChangedCallback = nameof(Callback))]  // Both combined
```

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Third-Party Libraries

See the [NOTICE](NOTICE) file for attribution information.

## See it in action

**[BarcodeTool](https://github.com/gbastecki/BarcodeTool)** | [Demo](https://gbastecki.github.io/BarcodeTool/) — A production app built with BlazorMvvm, featuring barcode generation and scanning.