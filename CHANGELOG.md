# Changelog

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