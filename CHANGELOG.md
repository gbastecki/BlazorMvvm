# Changelog

## [1.5.0]
### Added
- `ExecuteAsync` method on asynchronous commands (`IBlazorAsyncCommand`, `IBlazorAsyncRelayCommand`, `IBlazorAsyncRelayCommand<T>`), allowing commands to be awaited directly.
- `ContinueOnCapturedContext` property on asynchronous commands to control context capturing (`ConfigureAwait(continueOnCapturedContext)`), defaulting to `true`.
- `BlazorCommandAttribute` now supports `ContinueOnCapturedContext` (e.g., `[BlazorCommand(ContinueOnCapturedContext = false)]`), which the source generator automatically configures for the generated commands.
- Fully awaitable `OnPropertyChangedAsync` method in `BlazorViewModel` that awaits actual UI rendering completion, supported by a non-breaking `OnTriggerRefreshAsync` event.
- `continueOnCapturedContext` optional parameter on `OnPropertyChangedAsync` (defaulting to `true`) to configure synchronization context capturing dynamically per-call.
- `DebounceRefresh(delayMs, propertyName)` method on `BlazorViewModel` to debounce property changed notifications (and full view renders) with automatic reset and cancellation behavior, including automatic cancellation of property-specific debounces when a full view refresh is scheduled.
- `CancelAllDebounces()` helper method on `BlazorViewModel` for manual disposal safety cleanup to cancel all active debounce timers.
- Hash-based uniqueness in source generator filename output (hintNames), resolving namespace collision bugs (MAX_PATH limitations and class name duplication across namespaces).
- A CollisionViewModel unit test suite to verify filename generation stability under name collision conditions.
- Updated existing async unit tests to use `ExecuteAsync` for fully deterministic execution.
- Added `SynchronizationContext` unit tests verifying `ContinueOnCapturedContext` functionality for both commands and property changes.

### Fixed
- Thread-safe `IsExecuting` state tracking during concurrent executions (`AllowConcurrentExecutions = true`) in all asynchronous command classes, ensuring the state only reverts to `false` once all active executions are completed.

### Changed
- Upgraded `Microsoft.CodeAnalysis.Analyzers` in Generator to 5.6.0
- Downgraded `Microsoft.CodeAnalysis.CSharp` in Generator to 4.3.0 to provide broader compatibility with older .NET SDKs (e.g., .NET 6.0) while maintaining support for .NET 10.0.
- Updated `README.md` and Demo with new features and usage examples.

## [1.2.0]
### Added
- `BlazorMessenger` implementation for communicating between recipients (Cross-ViewModel communication).
- `BlazorMessengerAttribute` for source-generating messenger recipient registration code.
- `BlazorCommandAttribute` now supports optional `OnIsExecutingChangedCallback` parameter to specify a callback method that will be invoked when the `IsExecuting` state changes.
- `BlazorCommandAttribute` now supports optional `AutoRefreshOnIsExecutingChanged` parameter to automatically refresh the UI when the `IsExecuting` state changes.
- `BlazorObservablePropertyAttribute` now supports optional `Name` parameter to specify a custom generated property name.
- Unit tests for new features.

### Changed
- Reworked Demo project.
- Reworked `README.md`.
- Added `#nullable enable` to source generated files.

## [1.1.2]
### Changed
- `IBlazorMvvmViewModelFactory` and `BlazorMvvmScopedCache` registering in dependency injection via `builder.Services.UseBlazorMvvmViewModelFactory()` is now optional. `BlazorMvvmComponentBase` will not throw an exception if these are not registered.

## [1.1.1]
### Changed
- `BlazorMvvmViewModelFactory` no longer throws an exception when trying to resolve a ViewModel that is not registered. Instead, it returns `null`.
- `BlazorMvvmViewModelFactoryExtensions` is now generated for any `OutputKind`.

## [1.1.0]
### Added
- `BlazorMvvmViewModelFactory` for resolving ViewModels.
- `BlazorMvvmViewModelAttribute` for registering ViewModels in ViewModelFactory.
- `BlazorMvvmViewModelFactoryConstructorAttribute` to mark which ViewModel constructor should be used when resolving ViewModel if it has multiple constructors.
- Source generator to auto-generate ViewModel registration code.
- Unit tests for ViewModelFactory and related attributes.
### Changed
- `InvokeRefresh` in `BlazorMvvmComponentBase` is now `protected virtual` instead of `private`.
- Updated `README.md` with new features.

## [1.0.3]
### Added
- `BlazorCommandAttribute` and `BlazorObservablePropertyAttribute`.
- Source generator to auto-generate `BlazorCommands` and `BlazorObservableProperties`.
- Unit tests for source generator.
### Changed
- All `Commands` now throw `ArgumentNullException` if the `execute` parameter is `null`.
- Removed the upper limit on `Microsoft.AspNetCore.Components.Web` dependencies.
- Updated `README.md` with new features.

## [1.0.2]
### Added
- Support for .NET 10.
### Changed
- Updated `README.md`.

## [1.0.1]
### Changed
- Updated `README.md` file for better clarity.
- Reworked async `Commands`, changed synchronization primitive from `lock` to `SemaphoreSlim`.

## [1.0.0]
### Added
- Initial release.