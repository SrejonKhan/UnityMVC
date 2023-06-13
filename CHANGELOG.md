# [0.3.0] - 14-06-2023

New middleware functionality.

### Added

- Middleware functionality for route and view processing.
  - Introduced the ability to configure middleware for specific routes or views.
  - Middleware functions can perform checks and operations before processing the route or instantiating the view.
  - Middleware can be used to add custom logic, validations, or pre-processing requirements.

# [0.2.4] - 06-05-2023

`MVC.BeforeNavigate` event in `MVC` class.

### Added

- A navigate event, that get invoked before even the view is instantiated. In invokes when `MVC.Navigation` or similar method is called.

# [0.2.3] - 05-06-2022

`MVC.OnNavigated` event in `MVC` class.

### Added

- Navigate Event, that get invoked when `MVC.Navigation` is called.

# [0.2.2] - 04-06-2022

`Refresh()` feature for Views and some methods in `MVC` class.

### Added

- `OnRefreshAttribute` to mark methods for Refresh invokes.
- `GetHistory()` and `GetLastHistory()` methods in `MVC.cs`
- `ViewResult` prop in `ViewContainer` / View Classes.

# [0.2.1] - 17-04-2022

WebGl Support.

### Added

- WebGL Support
- Synchronous Addressable Loading

### Changed

- Addressable dependency version changed to `1.17.6-preview`.

# [0.2.0] - 23-03-2022

Improvements, fixes and missing features added.

### Added

- History
- Backward and Forward Navigation
- Controller Class Code Gen
- Partial View
- Layout View

### Fixed

- Scaffolding behavior fix
- Navigate Latency fix

### Changed

- `ReflectionHelper.cs` -> `MVCReflection.cs`
- Routing mechanism extracted to `Route.cs`

# [0.1.0] - 17-03-2022

Initially implemented MVC Pattern with Reflection and Addressable.

### Added

- Navigation with Routing URL
- Passing args in Navigation
- Controller and Actions
- View and ViewPrefab
- Model
- Registering ViewPrefab with View class and Actions (Scaffolding)
- Settings Provider (Config)
