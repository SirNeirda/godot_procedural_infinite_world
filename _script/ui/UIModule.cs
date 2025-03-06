using System;
using Bouncerock;
using Bouncerock.Events;
using Godot;

namespace Bouncerock.UI
{
    public partial class UIModule : Node
    {
        [Export]
        public string ModuleName;
        public UIModuleStatus Status = UIModuleStatus.Unloaded;

        //public UIModuleTypes Type;

        public Image Background;
        public Font FontOverride; // If this is not set, then font will be set by the global ui manager

         [Export]
        public UIModuleTypes Type;

         [Export]
        public bool LoadOnStart;

        [Export]
        public bool OpenOnStart;

         [Export]
        public bool IsActive;

         [Export]
        public bool BlockMovementOnStart;

        public virtual bool Initialize()
        {
            if (Status != UIModuleStatus.Unloaded) {return false;}
            try { 
                    if (LoadOnStart)
                    {
                        Boot(); 
                        Status = UIModuleStatus.Loaded; 
                    }
                    if (OpenOnStart)
                    {
                        OnOpen(); 
                        Status = UIModuleStatus.Active; 
                    }
                    return true; 
                }
            catch
            {
                return false;
            }
        }

        protected virtual void Boot()
        {

        }

        protected virtual void OnUIStyleChanged()
        {
            
        }

        public virtual void OnOpen()
        {
            
        }

        public virtual void OnClose()
        {
            
        }

        public Font SetFont()
        {

            if (FontOverride != null)
            {
                return FontOverride;
            }
            return null;
           // return GlobalUIManager.Instance.CurrentStyle.BaseFont;
        }

    }
}
