/*using System.Collections.Generic;
using Godot;
using Bouncerock.Events;

namespace Bouncerock.UI
{
    public enum UITestEventType { Prompt, Error }

        public struct BouncerockUITestEvent
        {
            public UITestEventType EventType;
            public string EventComment;
            public bool BlockMovement;
            public BouncerockTestEvent(UITestEventType eventType, string comment, bool blockMovement = false)
            {
                EventType = eventType;
                EventComment = comment;
                BlockMovement = blockMovement;
            }
        }
    public partial class TestUI : UIModule,
                                 BouncerockEventListener<BouncerockUITestEvent>
       {  
        
        protected RichTextLabel LabelContent;

        
        protected TabBar TabBar1;

        // Start is called before the first frame update
        protected override void Boot()
        {
            LabelContent = GetNode<RichTextLabel>("TabContainer/TabBar/PanelContainer/DebugReport");
            LabelContent.Text = "Success";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            
            this.BouncerockEventStartListening<IslandUITestEvent>();
        }
        public override void OnClose()
        {
            base.OnClose();
            this.BouncerockEventStopListening<IslandUITestEvent>();
        }

        public void SetLabel(string content)
        {
            LabelContent.Text = content;
        }

        public void OnBouncerockEvent(IslandUITestEvent evt)
        {
            LabelContent.Text = evt.EventComment; 
           
        }
    }
}*/