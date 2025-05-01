using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock
{
    public partial class Bush : WorldItem 
	{
        [Export]
	public CollisionShape3D collider;
        public  void _on_area_3d_body_entered(Node3D area)
        {
            if (area is MainCharacter)
            {
                MainCharacter chara = area as MainCharacter;
                chara.SpeedMultiplier = 0.5f;
                GD.Print("Entered");
            }
        }

        public  void _on_area_3d_body_exited(Node3D area)
        {
            if (area is MainCharacter)
            {
                MainCharacter chara = area as MainCharacter;
                chara.SpeedMultiplier = 1f;
            }
        }
    }
}