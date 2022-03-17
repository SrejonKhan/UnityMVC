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
Controller exist between the view and model. It listens to events triggered by the view (or another external source) and executes the appropriate reaction to these events. In most cases, the reaction is to call a method on the model. Since the view and the model are connected through a notification mechanism, the result of this action is then automatically reflected in the view.

#### MVC in Unity
MVC in Unity acts same as described above. In term of View, the View actually is prefab of User Interface, loaded from Resources or defined folder. Model and Controller acts same. 
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
If you want to get started quickly, it would be recommended to at least skimming through this example section, which will cover most of the features.  
Or, If you want see all ***available reference, please jump to [Reference](#reference).*** 

## Creating a Main Menu with MVC
### Goal
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