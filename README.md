# BlazorMvvm
![https://github.com/github/docs/actions/workflows/main.yml](https://github.com/gbastecki/BlazorMvvm/actions/workflows/build.yml/badge.svg)

Use the MVVM pattern for Blazor with a simple and lightweight library.

## Quick start

This library is distributed via [NuGet](https://www.nuget.org/packages/gbastecki.BlazorMvvm).

## Usage

### ViewModel
View models have to inherit `BlazorMvvm.BlazorViewModel`.

Example HomeViewModel.cs:

```c#
using BlazorMvvm;
namespace YourNamespace;
public class HomeViewModel : BlazorViewModel
{
    private int _counter;
    public int Counter
    {
        get => _counter;
        set
        {
            if (_counter == value) return;
            _counter = value;
            base.OnPropertyChanged(); //this will trigger the UI refresh
        }
    }

    private int _counter2;
    public int Counter2
    {
        get => _counter2;
        set => Set(ref _counter2, value); //this will perform an equality check and if the property and value don't match it will raise OnPropertyChanged event
    }
}
```

### ComponentBase
Components have to inherit `BlazorMvvm.BlazorMvvmComponentBase`.


When initializing a component, set the data context for the element when it participates in data binding using the SetDataContext method.

Example Home.razor:
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>
```
Example Home.razor.cs:
```c#
using BlazorMvvm;
namespace YourNamespace;
public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
{
    HomeViewModel ViewModel = new();

    protected override void OnInitialized()
    {
        SetDataContext(ViewModel);
        base.OnInitialized();
    }
}
```

### ObservableComponent
You can control the rendering of specific parts of a component by implementing the ObservableComponent.

Example Home.razor:
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>
<ObservableComponent ViewModel="ObservablePartViewModel">
    <div>ObservableComponent current counter: @ObservablePartViewModel.Counter</div>
</ObservableComponent>
<ObservableComponent ViewModel="SharedObservableViewModel" PropertyNames="[nameof(SharedObservableViewModel.Counter1)]">
    <div>SharedObservableViewModel current counter 1: @SharedObservableViewModel.Counter1</div>
</ObservableComponent>
<ObservableComponent ViewModel="SharedObservableViewModel" PropertyNames="[nameof(SharedObservableViewModel.Counter2), nameof(SharedObservableViewModel.Counter3)]">
    <div>SharedObservableViewModel current counter 2: @SharedObservableViewModel.Counter2</div>
    <div>SharedObservableViewModel current counter 3: @SharedObservableViewModel.Counter3</div>
</ObservableComponent>

```

Example Home.razor.cs:
```c#
using BlazorMvvm;
namespace YourNamespace;
public partial class Home : BlazorMvvmComponentBase<HomeViewModel>
{
    HomeViewModel ViewModel = new();
    ObservablePartViewModel ObservablePartViewModel = new();
    SharedObservableViewModel SharedObservableViewModel = new();
	
    protected override void OnInitialized()
    {
        SetDataContext(ViewModel);
        base.OnInitialized();
    }
}
```
Passing a ViewModel to an ObservableComponent automatically binds to it. 

If you also pass PropertyNames, the component is refreshed only if the OnPropertyChanged event is invoked for the associated property names. 

Based on the above example, modifying the SharedObservableViewModel.Counter3 property will refresh the fragment that contains the passed property names Counter2 and Counter3, but will not refresh the fragment that contains Counter1.

### Commands
Command implementations can expose a method or delegate to the component.

Available commands:
```c#
//Parameterless commands
BlazorAsyncCommand(Func<Task> execute, Func<Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false);
BlazorCommand(Action execute, Func<bool>? canExecute = null);

//Object type params commands
BlazorAsyncRelayCommand(Func<object[]?, Task> execute, Func<object[]?, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false);
BlazorRelayCommand(Action<object[]?> execute, Func<object[]?, bool>? canExecute = null);

//Generic type commands
BlazorAsyncRelayCommand<T>(Func<T, Task> execute, Func<T, Task<bool>>? canExecute = null, bool allowConcurrentExecutions = false);
BlazorRelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null);
```

If `canExecute` is provided, it will be executed before `execute`, and if it returns `false`, it will prevent the `execute` delegate from invoking.

Asynchronous commands also expose an `IsExecuting` property, and if `AllowConcurrentExecutions` is set to `false`, it will prevent the same command from being executed multiple times.

You can also subscribe to the `OnIsExecutingChanged` event, which is raised when the `IsExecuting` property changes.

Example ButtonExampleViewModel.cs:
```c#
using BlazorMvvm;
namespace YourNamespace;
public class ButtonExampleViewModel : BlazorViewModel
{
    public readonly IBlazorAsyncCommand DisableButtonCommand;
    public ButtonExampleViewModel()
    {
        DisableButtonCommand = new BlazorAsyncCommand(DisableButton);
        DisableButtonCommand.OnIsExecutingChanged += DisableButtonCommand_OnIsExecutingChanged;
    }
    ~ButtonExampleViewModel()
    {
        DisableButtonCommand.OnIsExecutingChanged -= DisableButtonCommand_OnIsExecutingChanged;
    }
    private void DisableButtonCommand_OnIsExecutingChanged(bool isExecuting)
    {
        base.OnPropertyChanged();
    }

    private async Task DisableButton()
    {
        await Task.Delay(5000);
    }
}
```

Example .razor:
```c#
@using BlazorMvvm
@inherits BlazorMvvmComponentBase<HomeViewModel>

<ObservableComponent ViewModel="ButtonExampleViewModel">
    <button @onclick="ButtonExampleViewModel.DisableButtonCommand.Execute" disabled="@ButtonExampleViewModel.DisableButtonCommand.IsExecuting">Disable button for a few seconds</button>
</ObservableComponent>
```
