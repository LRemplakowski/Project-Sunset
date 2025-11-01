using System;
using SunsetSystems.Audio;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("Exploration", "Combat", "Dialogue")]
	public class ES3UserType_ScenePlaylistData : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3UserType_ScenePlaylistData() : base(typeof(SunsetSystems.Audio.ScenePlaylistData)){ Instance = this; priority = 1;}


		public override void Write(object obj, ES3Writer writer)
		{
			var instance = (SunsetSystems.Audio.ScenePlaylistData)obj;
			
			writer.WritePropertyByRef("Exploration", instance.Exploration as PlaylistConfig);
			writer.WritePropertyByRef("Combat", instance.Combat as PlaylistConfig);
			writer.WritePropertyByRef("Dialogue", instance.Dialogue as PlaylistConfig);
		}

		public override object Read<T>(ES3Reader reader)
		{
			var instance = new SunsetSystems.Audio.ScenePlaylistData();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "Exploration":
						instance.Exploration = reader.Read<PlaylistConfig>();
						break;
					case "Combat":
						instance.Combat = reader.Read<PlaylistConfig>();
						break;
					case "Dialogue":
						instance.Dialogue = reader.Read<PlaylistConfig>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}


	public class ES3UserType_ScenePlaylistDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ScenePlaylistDataArray() : base(typeof(SunsetSystems.Audio.ScenePlaylistData[]), ES3UserType_ScenePlaylistData.Instance)
		{
			Instance = this;
		}
	}
}