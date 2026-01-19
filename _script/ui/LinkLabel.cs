using Godot;
using System.Text.RegularExpressions;

public partial class LinkLabel : RichTextLabel
{
	public override void _Ready()
	{
		BbcodeEnabled = true;
		MetaClicked += OnMetaClicked;

		/*Text = 
             "World Procedural Generation Project, brought by Adrien Pierret for Bouncerock Co.\nFull source code available under MIT Licence: [meta=https://github.com/SirNeirda/godot_procedural_infinite_world]https://github.com/SirNeirda/godot_procedural_infinite_world[/meta] .";*/
		


	}

	private void OnMetaClicked(Variant meta)
	{
		if (meta.VariantType == Variant.Type.String)
		{
			OS.ShellOpen(meta.AsString());
		}
	}
}