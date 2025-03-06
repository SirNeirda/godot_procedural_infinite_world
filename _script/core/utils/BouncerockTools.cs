using Godot;
using System.Collections;
using System;

namespace Bouncerock
{
	

	public static class BouncerockTools
	{
		
		public static void DebugLogTime(object message, string color = "")
		{
			string colorPrefix = "";
			string colorSuffix = "";
			if (color != "")
			{
				colorPrefix = "<color=" + color + ">";
				colorSuffix = "</color>";
			}
			GD.Print(colorPrefix + Time.GetTimeStringFromSystem() + " " + message + colorSuffix);

		}
	}
}
