# BlazorMvvm
![https://github.com/github/docs/actions/workflows/main.yml](https://github.com/gbastecki/BlazorMvvm/actions/workflows/build.yml/badge.svg)
[![GitHub](https://img.shields.io/github/license/gbastecki/BlazorMvvm?color=0000a4&style=plastic)](https://github.com/gbastecki/BlazorMvvm/blob/main/LICENSE)
[![NuGet version](https://img.shields.io/nuget/v/gbastecki.BlazorMvvm?color=6a00d5&label=nuget%20version&logo=nuget&style=plastic)](https://www.nuget.org/packages/gbastecki.BlazorMvvm/)
[![NuGet downloads](https://img.shields.io/nuget/dt/gbastecki.BlazorMvvm?color=6a00d5&label=nuget%20downloads&logo=nuget&style=plastic)](https://www.nuget.org/packages/gbastecki.BlazorMvvm/)

Use the MVVM pattern for Blazor with a simple and lightweight library.

## Quick start

This library is distributed via [NuGet](https://www.nuget.org/packages/gbastecki.BlazorMvvm).

Check [Live Demo](https://gbastecki.github.io/BlazorMvvm/).

## Usage

BlazorMvvm provides a lightweight set of base classes and components to implement the Model-View-ViewModel (MVVM) pattern in Blazor applications.

This guide outlines the core components and their usage.

##
### ViewModel

Viewmodels encapsulate the application's presentation logic and state. In BlazorMvvm, your viewmodels must inherit from the `BlazorViewModel` base class.

This base class implements `INotifyPropertyChanged`, which is essential for notifying the UI when a property's value has changed.

Example: `HomeViewModel.cs`
```c#
using BlazorMvvm;
using System.Runtime.CompilerServices;

namespace YourNamespace;

public class HomeViewModel : BlazorViewModel
{
    // --- Option 1: Manual Property Notification ---
    
    private int _counter;
    public int Counter
    {
        get => _counter;
        set
        {
            // Manual equality check
            if (_counter == value) return;
            
            _counter = value;
            
            // Manually raise the OnPropertyChanged event
            // This will trigger the UI to refresh
            base.OnPropertyChanged(); 
        }
    }

    // --- Option 2: Using the Set<T> Helper ---
    
    private int _counter2;
    public int Counter2
    {
        get => _counter2;
        
        // The Set() helper method simplifies this pattern:
        // 1. It performs an equality check.
        // 2. If the value is new, it updates the backing field.
        // 3. It raises the OnPropertyChanged event.
        set => Set(ref _counter2, value);
    }
}
```

##
### ComponentBase

To bind a view (Blazor Component) to a viewmodel, your components should inherit from `BlazorMvvmComponentBase<T>`, where `T` is the type of your viewmodel.

The final step is to connect your viewmodel instance to the component by calling `SetDataContext()` in the component's `OnInitialized` lifecycle method. This subscribes the component to the viewmodel's `PropertyChanged` events, automatically triggering `StateHasChanged()` to re-render the component when a property is updated.

Example: `Home.razor`
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>
```

Example: `Home.razor.cs`
```c#
using BlazorMvvm;
using Microsoft.AspNetCore.Components;

namespace YourNamespace;

public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
{
    // Instantiate the ViewModel
    HomeViewModel ViewModel = new();

    protected override void OnInitialized()
    {
        // Set the DataContext to link the View and ViewModel.
        // This is the essential step for enabling data binding.
        SetDataContext(ViewModel);
        
        base.OnInitialized();
    }
}
```

##
### ObservableComponent
By default, when a viewmodel property changes, the entire component bound via `SetDataContext` is re-rendered. For complex UIs, this can be inefficient.

The `ObservableComponent` allows you to define fine-grained "observable" fragments within your component. These fragments can be bound to a viewmodel and will only re-render when specific properties change, isolating the UI update.

#### Usage

- **Full Update**: Pass a `ViewModel` instance. The `ObservableComponent`'s child content will re-render for any property change on that viewmodel.

- **Selective Update**: Pass a `ViewModel` and a `PropertyNames` array. The child content will only re-render when one of the specified properties raises its `OnPropertyChanged` event.

Example: `Home.razor`
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>

<!-- 
  This fragment binds to ObservablePartViewModel and will
  refresh when ANY property on it changes.
-->
<ObservableComponent ViewModel="ObservablePartViewModel">
    <div>ObservableComponent current counter: @ObservablePartViewModel.Counter</div>
</ObservableComponent>

<!-- 
  This fragment binds to SharedObservableViewModel but will ONLY
  refresh when 'Counter1' changes.
-->
<ObservableComponent ViewModel="SharedObservableViewModel" PropertyNames="[nameof(SharedObservableViewModel.Counter1)]">
    <div>SharedObservableViewModel current counter 1: @SharedObservableViewModel.Counter1</div>
</ObservableComponent>

<!-- 
  This fragment also binds to SharedObservableViewModel but will ONLY
  refresh when 'Counter2' or 'Counter3' changes.
  A change to 'Counter1' will not affect this fragment.
-->
<ObservableComponent ViewModel="SharedObservableViewModel" PropertyNames="[nameof(SharedObservableViewModel.Counter2), nameof(SharedObservableViewModel.Counter3)]">
    <div>SharedObservableViewModel current counter 2: @SharedObservableViewModel.Counter2</div>
    <div>SharedObservableViewModel current counter 3: @SharedObservableViewModel.Counter3</div>
</ObservableComponent>
```

Example: `Home.razor.cs`
```c#
using BlazorMvvm;
using Microsoft.AspNetCore.Components;

namespace YourNamespace;

public class ObservablePartViewModel : BlazorViewModel { /* ... */ }
public class SharedObservableViewModel : BlazorViewModel { /* ... */ }

public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
{
    // Main ViewModel for the component
    HomeViewModel ViewModel = new();
    
    // ViewModels for the observable fragments
    ObservablePartViewModel ObservablePartViewModel = new();
    SharedObservableViewModel SharedObservableViewModel = new();
    
    protected override void OnInitialized()
    {
        // The main viewmodel is set as the primary DataContext
        SetDataContext(ViewModel);
        base.OnInitialized();
    }
}
```

##
### Commands
BlazorMvvm provides `Command` implementations that allow you to bind UI actions (like `@onclick`) to methods on your viewmodel, while also managing execution state (e.g., disabling a button while an async task is running).

#### Available Implementations

1. **Parameterless:**

    - `BlazorCommand(Action execute, Func<bool>? canExecute = null)`

    - `BlazorAsyncCommand(Func<Task> execute, Func<Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)`

2. **Generic (Type-Safe Parameter):**

    - `BlazorRelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null)`

    - `BlazorAsyncRelayCommand<T>(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)`

3. **Object-based Parameter:**

    - `BlazorRelayCommand(Action<object[]?> execute, Func<object[]?, bool>? canExecute = null)`

    - `BlazorAsyncRelayCommand(Func<object[]?, Task> execute, Func<object[]?, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false)`

#### Key Features

- `canExecute`: An optional delegate that determines if the command is allowed to run.

- `IsExecuting` (Async only): A `bool` property that is `true` while the `execute` task is running.

- `allowConcurrentExecutions` (Async only): If `false` (the default), prevents the command from executing if it `IsExecuting`.

- `OnIsExecutingChanged` (Async only): An event raised when `IsExecuting` changes. You must subscribe to this and call `OnPropertyChanged()` to notify the UI to update.

Example: `ButtonExampleViewModel.cs`

This example demonstrates an async command that disables a button for 5 seconds. It implements IDisposable to safely unsubscribe from the event handler.
```c#
using BlazorMvvm;
using System;
using System.Threading.Tasks;

namespace YourNamespace;

public class ButtonExampleViewModel : BlazorViewModel, IDisposable
{
    public IBlazorAsyncCommand DisableButtonCommand { get; }

    public ButtonExampleViewModel()
    {
        // Initialize the command, passing the method to execute
        DisableButtonCommand = new BlazorAsyncCommand(DisableButton);
        
        // Subscribe to the event to update the UI
        DisableButtonCommand.OnIsExecutingChanged += DisableButtonCommand_OnIsExecutingChanged;
    }

    private async Task DisableButton()
    {
        // Simulate a long-running operation
        await Task.Delay(5000);
    }

    private void DisableButtonCommand_OnIsExecutingChanged(bool isExecuting)
    {
        // Notify the UI that the command's state has changed
        // This allows the button's 'disabled' attribute to update
        base.OnPropertyChanged(nameof(DisableButtonCommand));
    }

    // Implement IDisposable to clean up event subscriptions
    public void Dispose()
    {
        DisableButtonCommand.OnIsExecutingChanged -= DisableButtonCommand_OnIsExecutingChanged;
    }
}
```

Example: `.razor` Component

This component hosts the `ButtonExampleViewModel` inside an `ObservableComponent` to ensure the button state updates correctly.
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>

<ObservableComponent ViewModel="ButtonExampleViewModel">
    <button 
        @onclick="ButtonExampleViewModel.DisableButtonCommand.Execute" 
        disabled="@ButtonExampleViewModel.DisableButtonCommand.IsExecuting">
        
        Disable button for 5 seconds
    </button>
</ObservableComponent>

@code {
    ButtonExampleViewModel ButtonExampleViewModel = new();

    protected override void OnDispose()
    {
        ButtonExampleViewModel.Dispose();
        base.OnDispose();
    }
}
```