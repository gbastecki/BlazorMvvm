# Changelog

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