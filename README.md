# UnityMVC

UnityMVC is a library for MVC Pattern, which is almost behave like ASP.NET MVC. This library is intentionally made to work mostly with User Interface management, but possible to work on other scenerio.

<p align="center">
    <img src ="https://i.ibb.co/8079VMG/xs-optimized-social-banner.png"/>
</p>

Here are the key features of UnityMVC:

- MVC pattern implementation for user interface management
- Convention-based routing for navigation between views
- Support for passing data to controllers and views
- Automatic loading and instantiation of view prefabs
- Reflection-based execution of controller actions
- Support for partial views
- Middleware support for executing operations before processing routes or instantiating views
- Navigation Events (`OnNavigated`, `BeforeNavigate`)
- Layout functionality for handling common UI layouts
- Easy creation of controller classes using the provided context menu
- Scaffolding feature for generating view classes and registering view prefabs
- Zenject (DI Library) Support
- Route History Debugger

# Installation

- Open Package Manager in Unity (Windows/Package Manager).
- Click on Plus Icon -> Add Package from git URL.
- Paste the following link - `https://github.com/SrejonKhan/UnityMVC.git`
- Click Add.

### Sample Project

If you are interested in exploring practical examples and use cases of the UnityMVC library, we have a dedicated sample project repository available. The [UnityMVC Sample Project](https://github.com/SrejonKhan/UnityMVC-Sample) provides a collection of samples that showcase the features and capabilities of the UnityMVC library in action.

In the sample project, you will find demonstrations of various features of the library. It's a great resource for getting hands-on experience with UnityMVC and understanding how it can enhance your Unity projects.

To access the UnityMVC Sample Project repository, please visit this [repository](https://github.com/SrejonKhan/UnityMVC-Sample).

We encourage you to explore the sample project and leverage the provided examples as a reference for your own UnityMVC implementations. If you have any feedback or suggestions regarding the sample project, feel free to open an issue or contribute to the repository.

# History

MVC (Model-View-Controller) is a widely used Software Design Pattern. It is commonly used for developing user interfaces that divide the related program logic into three interconnected elements.

As name suggests, there is 3 main components in MVC. They are -

#### Model

Model represents an object that contains data. It contains no logic describing how to present or process data. It just contains data.

#### View

View is the representation of Model's data, aka User Interface. View knows how to access Model's data.

#### Controller

Controller exist between the view and model. It listens to events triggered by the view (or another external source) and executes the appropriate reaction to these events. Controller handle user interaction, work with model and ultimately select a view to render.

#### MVC in Unity

MVC in Unity acts same as described above. In term of View, the View actually is prefab of User Interface, loaded by addressable. Model and Controller acts the same as described above.

#### Navigation/Routing

For navigating to different views, we use convention based routing like ASP.NET MVC. For example, if we want to navigate to Settings in Main Menu, routing path would be like -

```csharp
MVC.Navigate("Menu/Settings");
```

The convention of this routing URL is - `"ControllerName/ActionName/Id"`. Each Action is basically a view of that particular controller. In the following example, the Controller is **Menu Controller**, View is **MenuSettingsView** and Model can be **Settings**.

In UnityMVC, you can pass other data as arguments also, which can be processed/manipulated in corresponding Controller action.

```csharp
MVC.Navigate("Menu/Settings/en-bn", new Settings(defaultValue));
```

# Getting Started

If you want to get started quickly, it would be recommended to at least skimming through this example section, which will cover most of the features. You can consider reading [Creating a Main Menu with MVC](#creating-a-main-menu-with-mvc) section for **step-by-step guide**.

Or, If you want see all **_available reference, please jump to [Reference](#reference)._**

## How does it work?

UnityMVC makes it easier to load User Interface (it can be anything, any prefab), a.k.a Views without making many references or boilerplate work. Simply, if you want to load **Settings** panel in **Main Menu**, you just have to call `MVC.Navigate("Menu/Settings");` to load/instantiate **Settings** UI.

**_This is a diagram that is TL;DR of the explanation -_**

![Diagram](https://i.ibb.co/98k4Z75/2-4-DG5-Vy1.png)

As [Conventional Based Routing](#conventional-based-routing) refers, first part of the URL is Controller name. So, MVC will try to resolve first part of the URL when `Navigate()` get called. UnityMVC will look for that **Controller** class, in our case `MenuController`. UnityMVC uses **Reflection** behind the scene to execute most of it's task. UnityMVC caches all MonoController classes from all available assembly, at the very first `Awake()` call of Scene lifetime. It also try keep all instances of all controller classes in the scene, as MonoController (MonoBehaviour) can't be instantiated with `new` keyword. This finding instance query happens in a particular order to avoid full hierarchy combing for a instance. MVC will first try to get reference (`GetComponent()`) from **MvcContainer** gameobject, which has **MvcInitializer** as it's Component. If the reference is not there, it will then search in childs (`GetComponentInChildren()`), even if not found, it will search whole scene (`FindObjectOfType()`). In worst, if the scene also doesn't have any instance, the following instance will be created by UnityMVC while resolving particular request in Runtime. To avoid overhead, it's recommended to keep instances of Controller classes in Scene, as a child or flat level component of **MvcContainer** GameObject.

This is how UnityMVC resolves first part of routeUrl, that means UnityMVC has now corresponding Controller class to proceed further. After resolving first part, it's time for resolving the second part of the Url, which basically is the **Action** name of corresponding Controller. Actions are methods in Controller class, that returns `ViewResult` object. Action basically handles all sort of work for that particular call, select view, load and return it. In our case, our action method is `Settings()`. In this part of resolving, UnityMVC will invoke `Settings()` in `HomeController`. `Settings()` action can perform some task, prepare model (optional) and select proper view\*. Yes, Action method can select any View to show. `View()` in `MonoController` has some overloaded methods, where some of them accept **View Name** in their argument. If any view name has not passed, UnityMVC will take CalledMethod name as View name, in our case it is Settings.

`View()` in `MonoController` now tries to load view from Addressable by generating address. Generation of address is simply done by concatenating **Controller Name** and **View Name**, combined by a forward slash (`/`). In our case, the address would be `Home/Settings`.

After loading view from Addressable, UnityMVC will instantiate it under `MVC.RootCanvas` or provided transform argument. After instantiation, UnityMVC will resolve the **View Class**, which is generally a class attached to root of View as a component. **View Class** is important, and it is responsible for all view oriented work for that following view. UnityMVC will first try to get it from instantiated view, if it's not added, UnityMVC will add that ViewClass to View as a component.

If there is any Model for that particular view has been passed as argument in `View()` method, it will be injected to View Class in `Model` field. Note that, all view classes are derived from **`ViewContainer`** base class, and `ViewContainer` has a object field of `Model`. So any particual View Class doesn't need to explicitly declare field for `Model`.

As the final step, UnityMVC invoke all methods in View Class, that has [InvokeAttribute](#invokeattribute) placed on them.

For **Partial View**, it is mostly same. In **Partial View**, view is not pushed to **History** and no Active View is being destoyed.

## Controller

All Controller class should derives from `MonoController`. On the other hand, `MonoController` derives from `MonoBehaviour`, which let `MonoController`'s derived class (Controllers) to be added to GameObject as Component and receive Unity messages (Awake, Start, Update, FixedUpdate).

Example -

```csharp
using UnityMVC;

public class MenuController : MonoController
{
    // Start is called before the first frame update
    void Start()
    {
        // Do something in Start
    }

    // Update is called once per frame
    void Update()
    {
        // Do something in Update
    }

    // This is an Action Method
    // calling MVC.Navigate("Menu/Index");
    // will instantiate MenuIndex View
    public ViewResult Settings()
    {
        return View();
    }
}
```

Please note that, `Index()` is an Action Method for `MenuController`. This method will invoked when we call `MVC.Navigate("Menu/Settings")`. Every action method **must return ViewResult object**. `ViewResult` can be returned by calling `View()` method in any Controller class that derives from `MonoController`. This `View()` method also accept other arguments, please visit [Reference](#reference) for details.

There is a Context Menu for creating a Controller Class. It can be also done manually. For doing it quickly and without any mistakes, use Context Menu.

Create a Controller class with MVC Context Menu -

1. Right click on any empty space in Asset window.
2. Click `MVC -> Create Controller Class`
3. Write Controller name in Coge Gen Editor Window. Controller name should be without 'Controller' in it's name. For example, to create a MenuController, controller name should **'Menu'** only.
4. Click on **Create Controller Class** button.
5. Done.

![ControllerClassContextMenu.gif](https://i.ibb.co/qy2sD6H/1-hni3-FIv.gif)

## View

### View Prefab

View Prefab is the prefab that instantiate when action method is invoked for that particular view. View Prefab is loaded by Addressable. So, all View Prefabs are Addressable. View Prefab can be located in any location in Assets. It doesn't need to be follow any special convention.

Creating a View Prefab is simple, just make a prefab of your UI Panel or something else by drag and drop in Asset Window. No need to follow any other steps for View Prefab, or it doesn't need to be manually addressed in Addressable. Scaffolding will take care of that. We will talk about Scaffolding later.

Example -

![ViewPrefab.png](https://i.ibb.co/vjNMSR3/3-s5-HAp-F0.png)

### View Class

View Class added to View Prefab as a Component. View Class can be either added in prefab or it will be added by UnityMVC when resolving a Navigate call. It's better to add before in Prefab. View Class is derived from `ViewContainer` class.

View Class must maintain a convention for it's name. ViewClass name should be a concatenated string of Controller Name, Action Name and suffix "View" to the end (`{ControllerName}{ActionName}View.cs`). For example, View Class for **Home Controller** and **Settings** action should be **`HomeSettingsView`**.

Creating View Class is also hassle free. Scaffolding will ask you to create one on behalf of you. It will be discussed in [Scaffolding](#scaffolding) section.

Example -

```csharp
using UnityMVC;
using UnityEngine.UI;

public class MenuSettingsView : ViewContainer
{
    public Dropdown musicOptionDropdown;
    public Slider musicVolumeSlider;
    public Dropdown videoQualityDropdown;

    // unity messages are supported
    // as ViewContainer derives from MonoBehaviour
    void Start() { }
    void Update() { }

    // it will be automatically called when View is isn
    [Invoke]
    void Init()
    {
        if(Model == null) return;

        var model = (Settings)Model; // model object

        musicOptionDropdown.value = model.musicOption;
        musicVolumeSlider.value = model.musicVolume;
        videoQualityDropdown.value = model.videoQuality;

        musicOptionDropdown.onValueChanged.AddListener(MusicOptionChanged);
        musicVolumeSlider.onValueChanged.AddListener(VolumeChange);
        videoQualityDropdown.onValueChanged.AddListener(VideoQualityChange);
    }

    void MusicOptionChanged(int value)
    {
        // some functionality
        ...
    }

    // rest of the code
    ...

}
```

### Partial View

Partial View is same as View. Actual difference is in invoking `MVC.Navigate()`. To load any view as Partial View -

```csharp
MVC.Navigate("Alert/Warning", true, someData); // 2nd arg indicate it is a Partial View

// Normally loading View
// It will be pushed to History,
// so it can be navigate back
MVC.Navigate("Alert/Warning", someData);
```

![PartiaView.gif](https://i.ibb.co/x7vhmCP/5-Qmtr-R06.gif)

#### Remarks for Partial View -

1. PartialView doesn't destroy Active View. It just instantiate on top of that.
2. PartialView doesn't get pushed to History. So it can't be navigate back or forward.
3. PartialView is normal View. Scaffolding works same way as normal View.

## Model

Model is a simple C# class, without any base class or any convention. Model is responsible for holding data, not any other responsibility. Model object can be passed in `View(model)` from Action, then it will be injected into View Class for that corresponding Controller and Action. Later, Model object can be access from View Class's `Model` field. Note that, all view classes are derived from **`ViewContainer`** base class, and `ViewContainer` has a object field of `Model`. So any particual View Class doesn't need to explicitly declare field for `Model`.
Example -

```csharp
// This is Model Class
public class Settings
{
    public string currentLocale = "en-bn";
    public int musicOption = 0;
    public float musicVolume = 1;
    public int videoQuality = 3;
}
```

## Scaffolding

Scaffolding reduces a lot of work. Scaffolding comes handy when you have created Controller Class, Action Methods and View Prefab for Action.

Scaffolding Window will create a View Class for corresponding Controller and Action. Then register View Prefab in Addressable.

![ScaffoldingWindow](https://i.ibb.co/9ZCDp6j/4-AOn-PN1-B.gif)

Features:

1. Create View Class for selected Controller and Action Method
2. Register View Prefab to Addressable
3. Error check (if View Prefab is registered to wrong address).

## Layout

Layout Prefab is instantiate when MVC Initialize. Layout is not pushed to History.

Layout options in `MvcInitializer.cs` -

![LayoutMvcInit.gif](https://i.ibb.co/TYXcRp3/6-9-KUtp1u.png)

Layout Demo -

![LayoutDemo.gif](https://i.ibb.co/BsNXNkP/7-m7-TTHTA.gif)

#### OnLayoutInstantiated

This event is fired when Layout is instantiated in Root. We can take advantage of it in many way.

For example, setting Root transform to Layout's container after instantiation -

```csharp
mvcInitializer.onLayoutInstantiated.AddListener(go =>
{
    // set root to instantiated layout's container
    MVC.Root = go.transform.GetChild(1).gameObject;
});
```

# Middleware

Middleware functionality allows you to configure middleware to be executed before processing a specific route or view. This middleware can perform checks or operations and determine whether the route should be processed or the view should be instantiated.

To understand how the middleware works, let's take a look at an example configuration:

```csharp
MVC.ConfigureMiddleware().OnRoute("Menu/Index", (ctx, type) =>
{
    if (!User.IsAuthenticated)
        return false;

    // Perform other necessary operations or checks

    return true;
});
```

In this example, we configure middleware for the `Menu/Index` route. The ctx parameter represents the `ActionResult`, which is the result of the view. However, since the middleware is executed before instantiating the view, the ctx in this context is `PendingViewResult`, which contains route url only. It always has a room to improve, but for now, we will stick to this simple implementation.

The type parameter represents the `ActionType` for the requested view. If the view is requested to be instantiated as a partial view, the type will be set to `PartialView`; otherwise, it will be set to `View`.

Within the middleware function, you can perform various checks and operations. In the provided example, we check if the user is authenticated (`User.IsAuthenticated`). If the user is not authenticated, we return `false`, indicating that the middleware should block further processing of the route or view. In such case of cancellation, MVC.Navigate() or other similar navigate method will return FailedViewResult which contains nothing but the Route URL. If the user is authenticated, we can perform any other necessary operations or checks and return `true` to allow the route or view to be processed.

Please note that per route can have multiple configurations, which makes this feature much flexible.

If we want to re-configure middleware from scratch again, we can simply call -

```csharp
MVC.ClearMiddleware();
```

By configuring middleware in this manner, we can easily add custom logic or validations before handling specific routes or views within application. This can be particularly useful for implementing authentication checks, authorization rules, or any other pre-processing requirements.

Feel free to explore this feature and leverage its capabilities to enhance your Unity projects and streamline your MVC implementation.

# InvokeAttribute

InvokeAttribute is a special attribute provided by UnityMVC to invoke methods in View Class when that particular view class is instantiated.

```csharp
using UnityEngine;
using UnityMVC;

public class HomeIndexView : ViewContainer
{
    // SanityChecks() will be called when HomeIndexView is instantiated
    [Invoke]
    void SanityChecks()
    {
        Debug.Log(((Home)Model).homeIdentifier);
    }
}
```

# OnRefresh Attribute

`OnRefresh` is an attribute to mark methods in View Class to be invoked when `ViewResult.Refresh()` is called.

```csharp
using UnityEngine;
using UnityMVC;

public class HomeIndexView : ViewContainer
{
    [OnRefresh]
    void UpdateData()
    {
        // server calls
        // ...
        // ...
        Debug.Log("Data Updated");
    }
}
```

### How to invoke `Refresh()` on any View

Reference of `ViewResult` is required to call `Refresh()` for specific view. Reference can be obtained from different sources -

- Upon calling `MVC.Navigate()`.\*
- From reference of View Class. (View Class derives from ViewContainer, which has a property of `ViewResult`).
- `MVC.GetLastHistory()`\*

\*These method returns `ActionResult` which should be casted to `ViewResult`.

Example -

```csharp
// Keeping reference of ActionResult from MVC.Navigate()
var viewResult = (ViewResult)MVC.Navigate("Home/Index");
...
viewResult.Refresh();

// From reference of view class
HomeIndexView homeIndexView = /*Assuming we get a reference*/;
homeIndexView.ViewResult.Refresh();

// MVC.GetLastHistory()
var lastView = (ViewResult)MVC.GetLastHistory();
lastView.Refresh();
```

# `MVC.OnNavigated`

`MVC.OnNavigated` get invoked each time we call `MVC.Navigate()` / `MVC.NavigateForward()` / `MVC.NavigateBackward()` . It's a way to execute something common. For example -

```csharp
MVC.OnNavigated += (ctx, type) =>
{
    if (type == ActionType.View)
        audioSource.PlayOneShot(viewSfx);

    if (type == ActionType.PartialView)
        audioSource.PlayOneShot(partialViewSfx);
};
```

# `MVC.BeforeNavigate`

`MVC.BeforeNavigate` get invoked each time we call `MVC.Navigate()` / `MVC.NavigateForward()` / `MVC.NavigateBackward()` . But, it invoked before even the view instantiate or other navigation process. It can be treated like middleware (not much right now). If it return `true`, it will instantiate and complete the full journey of Navigation process. Or, if it return `false`, it will just cancel the reamining navigation process, nothing will be instantiated. In such case of cancellation, `MVC.Navigate()` or other similar navigate method will return `FailedViewResult` which contains nothing but the Route URL.

For example, if the route is for Admin and user is not Admin, we will just cancel the navigation process -

```csharp
MVC.BeforeNavigate += (ctx, type) =>
{
    if(actionResult.RouteUrl.StartsWith("Admin"))
    {
        if (!auth.user.IsAdmin)
            return false;
    }
    return true;
};
```

# Conventional Based Routing

Navigations are done by conventional based routing. Each routeUrl defines something, that makes whole MVC to work perfect. Following is a general route url -

```csharp
string routeUrl = "Home/Settings";
```

If we take this `routeUrl` and split it by '/', we will get 2 major part.

```
Home -> Home(Controller)
Settings -> Settings(View)
```

So, when we try to navigate to `"Home/Settings"`, behind the scene, we are calling `HomeController`, it's `Settings` method a.k.a Action, which basically then Instantiate `SettingView` which is a prefab of something, most probably User Interface.

Route Url can pass ID in it, for example -

```csharp
string routeUrl = "Home/User/1242";
```

Last part of routeUrl is ID, it is quite useful in some cases where passing ID is necessary.

Object can be passed as argument in `Navigate()` method, which is out of scope in this section. For details, check [Reference](#reference).

# Zenject Support

UnityMVC doesn't implement Zenject in Core, as dynamic instantiation is different in Zenject. Rather than changing, there is a handly workaround.

1. Clone [this repo](https://github.com/legionwfz/Zenject-DynamicObjectInjection/tree/master/UnityProject/Assets/Plugins/ZenjectDynamicObjectInjection/Source)
2. Copy `ZenjectDynamicObjectInjection.cs` and `ZenjectLocator.cs` to your project.
3. Add `using System.Linq;` as directive at the top of `ZenjectDynamicObjectInjection.cs` file.
4. In each view prefab, which corresponding ViewClass implement zenject, add `ZenjectDynamicObjectInjection.cs` to it.
5. Done.

# Debugger

### Route History Debugger

Route History Debugger is a useful tool provided by UnityMVC for debugging and visualizing the history of routes in your application. To access the debugger, navigate to `Window/MVC/History Debugger/Route History` in the Unity Editor.

During runtime, the Route History Debugger displays a list of all the routes that have been navigated in your application. The currently active route is highlighted for easy identification. Each row of history in the debugger contains a `details` button that allows you to view additional details about that specific route.

![history_debugger.png](https://i.ibb.co/kqg0G8T/history-debugger.png)

# Reference

### MVC

```csharp
public static ActionResult Navigate(string routeUrl);
public static ActionResult Navigate(string routeUrl, params object[] args);
public static ActionResult Navigate(string routeUrl, bool partialView);
public static ActionResult Navigate(string routeUrl, bool partialView, params object[] args);
public static ActionResult NavigateBackward(int steps);
public static ActionResult NavigateForward(int steps);
public static ActionResult[] GetHistory();
public static ActionResult GetLastHistory();
public static ActionResult GetCurrentHistoryIndex();
public static ActionResult GetCurrentHistory();

public static GameObject MvcContainer;
public static Canvas RootCanvas;
public static event NavigateEventHandler OnNavigated;

public static MiddlewareConfiguration ConfigureMiddleware();
public static void ClearMiddleware();
```

### MonoController

Base Class for every Controller Class.

```csharp
// Note - Method Name == View Name if not passed any
public ViewResult View(string viewName = null);
public ViewResult View(Transform parent, string viewName = null);
public ViewResult View(object model, string viewName = null);
public ViewResult View(Transform parent, object model, string viewName);
```

### ActionResult

```csharp
public UnityEngine.Object Result;
public Transform Parent;
public ActionType NavigationActionType;
public void ReleaseAddressableReference();
```

### ViewContainer

```csharp
public object Model;
// Handy method for UI OnClick
public virtual void Navigate(string routeUrl);
public virtual void NavigatePartial(string route);
public virtual void NavigateBack();
public virtual void NavigateForward();

public virtual ActionResult NavigateActionResult(string routeUrl);
public virtual ActionResult NavigatePartialActionResult(string route);
public virtual ActionResult NavigateBackActionResult();
public virtual ActionResult NavigateForwardActionResult();

public virtual void ReleaseAddressableReference()
```

### ViewResult

```csharp
public void Refresh();
public ViewContainer ViewContainerComponent;
```

### PendingViewResult

```csharp
public Action<ActionResult> OnFulfilled;
```

### MiddlewareConfiguration

```csharp
using MiddlewareDelegate = System.Func<UnityMVC.ActionResult, UnityMVC.ActionType, bool>;

public void OnRoute(string route, MiddlewareDelegate callback);
public void RemoveRouteConfiguration(string route);
```

Happy coding with UnityMVC!
