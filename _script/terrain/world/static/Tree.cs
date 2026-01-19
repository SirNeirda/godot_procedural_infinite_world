using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock.Terrain
{
    public partial class Tree : WorldItemModel 
	{
        public override void Initialize(int lod)
        {
          // OnChangedLOD(lod);
        }

        public override void OnChangedLOD(int lod)
        {
           //if (lod <3){Visible = true;}
           // else{Visible = false;}
        }
    }
}