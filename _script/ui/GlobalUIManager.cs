using System.Collections;
using System.Collections.Generic;
using Godot;
using Bouncerock.Events;

namespace Bouncerock.UI
{
    public enum UIModuleStatus { Unloaded, Loaded, Active, Inactive }

    public enum UIModuleTypes { Header, Footer, Menu, Fullscreen, Character, Prompt, Controller, Test }

    public enum UIEventType { Close, Open}

   

    public struct BouncerockUIEvent
    {
        public UIModule Module;
        public UIEventType EventType;
        public string EventComment;
        public bool BlockMovement;
        public BouncerockUIEvent(UIModule module, UIEventType eventType, string comment, bool blockMovement = false)
        {
            Module = module;
            EventType = eventType;
            EventComment = comment;
            BlockMovement = blockMovement;
        }
    }

    public partial class GlobalUIManager : Node,
                                   BouncerockEventListener<BouncerockUIEvent>
    {
        public static GlobalUIManager Instance;

        protected CanvasLayer MainCanvas;

        public Control MainFrame;

        

       // public TestUI _testUI;
        
        public List<UIModule> AvailableUIModules;
         [Export] public Control MobileUI;
       // public Dictionary<string, UIModule> AvailableUIModules = new Dictionary<string, UIModule>();

        [Export]
        public string[] ModulesToLoad;

        public bool MovementBlocked = false;

        public override void _Ready()
        {
            Instance = this;
            MobileUI.Visible = false;
            #if GODOT_ANDROID
			MobileUI.Visible = true;
			#endif
            //StartModules();
        }

        protected void StartModules()
        {
            MainFrame = GetNode<Control>("CanvasLayer/MainUIFrame");

            foreach (string module in ModulesToLoad)
            {
                //GD.Print("res://_scenes/gui/" + module + ".tscn");
                PackedScene newScenedd = ResourceLoader.Load<PackedScene>("res://_scenes/gui/"+module+".tscn");
                 Node uiModulex = newScenedd.Instantiate();
                
                //MainFrame.CallDeferred("add_child",uiModulex);
                MainFrame.AddChild(uiModulex);
                UIModule uimodule = uiModulex as UIModule;
                uiModulex.Name = module;
                if (uimodule.LoadOnStart) { uimodule.Initialize();}

                
            }
            //IslandUITestEvent testEvent = new IslandUITestEvent(UITestEventType.Prompt, "it worked");
            //IslandsEventManager.TriggerEvent<IslandUITestEvent>(testEvent);
        }

        public bool IsModuleLoaded(UIModuleTypes moduletype)
        {
            
            for (int i = 0; i < AvailableUIModules.Count; i++)
            {
                if (AvailableUIModules[i] == GetModuleFromType(moduletype))
                {
                    return true;
                }
            }
            return false;
        }

        public UIModule GetModuleFromType(UIModuleTypes moduletype)
        {
            foreach (UIModule mod in AvailableUIModules)
            {
                if (mod.Type == moduletype)
                {
                    return mod;
                }
            }
            return null;
        }

        //return true if successful
        private bool RemoveModuleFromList(UIModule module)
        {
            for (int i = 0; i < AvailableUIModules.Count; i++)
            {
                if (AvailableUIModules[i] == module) 
                { 
                    AvailableUIModules.RemoveAt(i); 
                    return true;
                }
                
            }
            return false;
        }

        private bool AddModuleToList(UIModule module)
        {
            for (int i = 0; i < AvailableUIModules.Count; i++)
            {
                if (AvailableUIModules[i] == module) 
                {
                    GD.Print("Module " + module.ToString() + " is already opened!");
                    return false;
                }
            }
            AvailableUIModules.Add(module);
            return true;
        }

        private void OnEnable()
        {
            this.BouncerockEventStartListening<BouncerockUIEvent>();
        }
        private void OnDisable()
        {
            this.BouncerockEventStopListening<BouncerockUIEvent>();
        }

        public void OnBouncerockEvent(BouncerockUIEvent evt)
        {
            GD.Print("UI Event:" + evt.Module.ToString() + " Event: " + evt.EventType.ToString() + "Comment: " + evt.EventComment);
            if (evt.EventType == UIEventType.Close)
            {
                RemoveModuleFromList(evt.Module);
            }
            if (evt.EventType == UIEventType.Open)
            {
                AddModuleToList(evt.Module);
            }
            /*if (evt.BlockMovement && evt.EventType == UIEventType.Close)
            {
                BlockPlayerMovement();
            }
            if (evt.BlockMovement && evt.EventType == UIEventType.Close)
            {
                UnblockPlayerMovement();
            }*/
        }

        

        public UIModule OpenModule(UIModuleTypes moduletype)
        {
            foreach (UIModule module in AvailableUIModules)
            {
                if (module.Type == moduletype)
                {
                    AddModuleToList(module);
                   // UIModule mod = Instantiate(module.Module as UIModule, this.transform);
                    //mod.InternalBoot();
                    return module;
                }
            }
            return null;
        }

        public void CloseModule(UIModuleTypes moduletype)
        {
            foreach (UIModule module in AvailableUIModules)
            {
                if (module.Type == moduletype)
                {
                    RemoveModuleFromList(module);
                    CallDeferred("free", module);
                }
            }
        }

        public void BlockPlayerMovement()
        {
            MovementBlocked = true;
        }

        public void UnblockPlayerMovement()
        {
            MovementBlocked = false;
        }

        public T GetUIModule<T>() where T : UIModule
        {
            foreach (UIModule module in AvailableUIModules)
            {
                if (module is T typedModule)
                {
                    return typedModule;
                }
            }

            return null; // If no module of the specified type is found
        }

    }
}
