# UnityMVC
***THIS README IS FAIRLY INCOMPLETE, PLEASE WATCH FOR COMPLETE DOCUMENTATION.***

UnityMVC is a library for MVC Pattern, which is almost behave like ASP.NET MVC. This library is intentionally made to work mostly with User Interface management, but possible to work on other scenerio.
 
 # Installation
- Open Package Manager in Unity (Windows/Package Manager).
- Click on Plus Icon -> Add Package from git URL. 
- Paste the following link - `https://github.com/SrejonKhan/UnityMVC.git`
- Click Add.

# History
MVC (Model-View-Controller) is a widely used Software Design Pattern. It is commonly used for developing user interfaces that divide the related program logic into three interconnected elements.

As name suggests, there is 3 main components in MVC. They are - 
#### Model
Model represents an object that contains data. It contains no logic describing how to present or process data. It just contains data/
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

Or, If you want see all ***available reference, please jump to [Reference](#reference).*** 

## How does it work? 
UnityMVC makes it easier to load User Interface (it can be anything, any prefab), a.k.a Views without making many references or boilerplate work. Simply, if you want to load **Settings** panel in **Main Menu**, you just have to call `MVC.Navigate("Menu/Settings");` to load/instantiate **Settings** UI. 

As [Conventional Based Routing](#conventional-based-routing) refers, first part of the URL is Controller name. So, MVC will try to resolve first part of the URL when `Navigate()` get called. UnityMVC will look for that **Controller** class, in our case `MenuController`. UnityMVC uses **Reflection** behind the scene to execute most of it's task. UnityMVC caches all MonoController classes from all available assembly, at the very first `Awake()` call of Scene lifetime. It also try keep all instances of all controller classes in the scene, as MonoController (MonoBehaviour) can't be instantiated with `new` keyword. This finding instance query happens in a particular order to avoid full hierarchy combing for a instance. MVC will first try to get reference (`GetComponent()`) from **MvcContainer** gameobject, which has **MvcInitializer** as it's Component. If the reference is not there, it will then search in childs (`GetComponentInChildren()`), even if not found, it will search whole scene (`FindObjectOfType()`). In worst, if the scene also doesn't have any instance, the following instance will be created by UnityMVC while resolving particular request in Runtime. To avoid overhead, it's recommended to keep instances of Controller classes in Scene, as a child or flat level component of **MvcContainer** GameObject. 

This is how UnityMVC resolves first part of routeUrl, that means UnityMVC has now corresponding Controller class to proceed further. After resolving first part, it's time for resolving the second part of the Url, which basically is the **Action** name of corresponding Controller. Actions are methods in Controller class, that returns `ViewResult` object. Action basically handles all sort of work for that particular call, select view, load and return it. In our case, our action method is `Settings()`. In this part of resolving, UnityMVC will invoke `Settings()` in `HomeController`. `Settings()` action can perform some task, prepare model (optional) and select proper view*. Yes, Action method can select any View to show. `View()` in `MonoController` has some overloaded methods, where some of them accept **View Name** in their argument. If any view name has not passed, UnityMVC will take CalledMethod name as View name, in our case it is Settings.

`View()` in `MonoController` now tries to load view from Addressable by generating address. Generation of address is simply done by concatenating **Controller Name** and **View Name**, combined by a forward slash (`/`). In our case, the address would be `Home/Settings`. 

After loading view from Addressable, UnityMVC will instantiate it under `MVC.RootCanvas` or provided transform argument. After instantiation, UnityMVC will resolve the **View Class**, which is generally a class attached to root of View as a component. **View Class** is important, and it is responsible for all view oriented work for that following view. UnityMVC will first try to get it from instantiated view, if it's not added, UnityMVC will add that ViewClass to View as a component.  

If there is any Model for that particular view has been passed as argument in `View()` method, it will be injected to View Class in `Model` field. Note that, all view classes are derived from **`ViewContainer`** base class, and `ViewContainer` has a object field of `Model`. So any particual View Class doesn't need to explicitly declare field for `Model`. 

As the final step, UnityMVC invoke all methods in View Class, that has [InvokeAttribute](#invokeattribute) placed on them. 

**This is a diagram that is TL;DR of above explanation -**


## Creating a Main Menu with MVC
### Goal
Make a main menu with 
### Steps to create Controller
### Create Model 
### Create View
### Navigating 

# Reference
### MVC
```csharp
MVC.Navigate(string routeUrl);
MVC.Navigate(string routeUrl, param object[] args);
```
### MonoController
Base Class for every Controller Class.
```csharp
public ViewResult View(); // Method Name == View Name
public ViewResult View(string viewName);
public ViewResult View()
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