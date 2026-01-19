using System.IO;
using System.Collections.Generic;
using Bouncerock.Events;
using Godot;
using Bouncerock.UI;

namespace Bouncerock
{
    
    public class Debug
    {
        public static bool Initialized = false;
        public static List<Logentry> Log = new List<Logentry>();

		public static List<MeshInstance3D> GizmoHelpers = new List<MeshInstance3D>(); 

		public Node HelpersFolder;

        public class Logentry
		{
			public string TimeStamp;
			public string Content;
			public enum LogTypes { Log, Warning, Exception }
			public LogTypes LogType;

            

			public Logentry(string content, LogTypes logtype)
			{
				string minutes = Mathf.Floor((float)SoftwareManager.Runtime / 60).ToString("00");
				string seconds = Mathf.Floor((float)SoftwareManager.Runtime % 60).ToString("00");

				TimeStamp = minutes + ":" + seconds;
				Content = content;
				LogType = logtype;
			}

			public string PrintLog()
			{
				return "[" + TimeStamp + "]" +
					"[" + LogType.ToString() + "]" +
					": " +
					Content;
			}
		}



        public static void DebugStartSession()
		{
            if (Initialized) {return;}
            Initialized = true;
			string _content = "/////////Started new session///////////";
			string path = SoftwareManager.GetPersistentPath() + SoftwareManager.GetDebugFilePath();
			//string secondarypath = persistentpath + debugpath + "1";
			//bool createnewpath = false;
			if (File.Exists(path))
			{
				FileInfo info = new FileInfo(path);
				if (info.Length > 30000)
                {
					File.WriteAllText(path, System.String.Empty);
				}
				//Debug.Log(info.Length);
				StreamWriter writer = new StreamWriter(path, true);
				writer.WriteLine(_content);
				writer.Close();
			}
			if (!File.Exists(path))
			{
				/*SStreamWriter writer = new StreamWriter(path, true);
				string welcomecontent = "Isl Debug File";
				SystemInformations infos = SoftwareManager.GetSystemInformations();
				string systemcreds = "Operating System: " + infos.OS + ", " +
					"Processor: " + infos.Processor + ", " +
					"Clock speed: " + infos.ProcessorSpeed + ", " +
					"RAM: " + infos.RAM + ", " +
					"GPU: " + infos.GPUName + ", ";
				writer.WriteLine(welcomecontent);
				writer.WriteLine(systemcreds);
				writer.WriteLine(_content);
				writer.Close();*/
			}
		}

		public static void DebugLogEntry(string entry)
		{
			Logentry newlog = new Logentry(entry, Logentry.LogTypes.Log);
			ShouldWriteDebugLog();
			Log.Add(newlog);
		}

		public static void DebugLogWarning()
		{
			ShouldWriteDebugLog();
		}

		public static void DebugLogError(string entry)
		{
			ShouldWriteDebugLog();
			Logentry newlog = new Logentry(entry, Logentry.LogTypes.Exception);
			ShouldWriteDebugLog();
			Log.Add(newlog);
		}

		protected static void ShouldWriteDebugLog()
		{
			if (Log.Count >= 10)
			{
				WriteDebugLog();
			}
		}

		protected static void WriteDebugLog(string content = "")
		{
			string path = SoftwareManager.GetPersistentPath() + SoftwareManager.GetDebugFilePath();
			StreamWriter writer = new StreamWriter(path, true);
			if (File.Exists(path))
			{
				for (int i = 0; i < Log.Count; i++)
				{
					writer.WriteLine(Log[i].PrintLog());
				}
				writer.Close();
				Log.Clear();
			}
			else
			{
				GD.Print("Can't write log file: doesn't exist");
			}
		}

        public static void SetStaticBug(string content)
        {
           // IslandUITestEvent evt = new IslandUITestEvent(UITestEventType.Prompt, content);
           // IslandsEventManager.TriggerEvent<IslandUITestEvent>(evt);
        }

		public static WorldSpaceUI SetTextHelper(string content, Vector3 location, Node3D parent = null)
        {
            PackedScene World = ResourceLoader.Load<PackedScene>("res://_scenes/gui/worldspace/helper_label.tscn");
			Node node = World.Instantiate();
			WorldSpaceUI wsui = node as WorldSpaceUI;
			wsui.Position = location;
			wsui.SetText(content);
			wsui.Name = "MainCharLabel";
			if (parent != null)
			{
				parent.CallDeferred("add_child",wsui);
			}
			return wsui;
        }

		public static MeshInstance3D SetGizmoBox(Vector3 location, Vector3 size, Node3D parent = null)
        {
            Color semitransparent = new Color(1,1,1,0.5f);
			MeshInstance3D GizmoHelper = LineDrawer.DrawAABB(location, size, semitransparent);
			GizmoHelper.Name = "Gizmo Helper_" + GizmoHelpers.Count;
			GizmoHelpers.Add(GizmoHelper);
			return GizmoHelper;
        }


    }


}
