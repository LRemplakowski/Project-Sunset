using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("SaveID", "SaveName", "SaveDate", "ActiveQuestName", "PlayTime", "LevelLoadingData", "PlaylistData", "SaveScreenShot")]
	public class ES3UserType_SaveMetaData : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3UserType_SaveMetaData() : base(typeof(SunsetSystems.Persistence.SaveMetaData)){ Instance = this; priority = 1;}


		public override void Write(object obj, ES3Writer writer)
		{
			var instance = (SunsetSystems.Persistence.SaveMetaData)obj;
			
			writer.WriteProperty("SaveID", instance.SaveID, ES3Type_string.Instance);
			writer.WriteProperty("SaveName", instance.SaveName, ES3Type_string.Instance);
			writer.WriteProperty("SaveDate", instance.SaveDate, ES3Type_string.Instance);
			writer.WriteProperty("ActiveQuestName", instance.ActiveQuestName, ES3Type_string.Instance);
			writer.WriteProperty("PlayTime", instance.PlayTime, ES3Type_double.Instance);
			writer.WriteProperty("LevelLoadingData", instance.LevelLoadingData, ES3UserType_LevelLoadingData.Instance);
			writer.WriteProperty("PlaylistData", instance.PlaylistData, ES3UserType_ScenePlaylistData.Instance);
			writer.WriteProperty("SaveScreenShot", instance.SaveScreenShot, ES3.ReferenceMode.ByValue);
		}

		public override object Read<T>(ES3Reader reader)
		{
			var instance = new SunsetSystems.Persistence.SaveMetaData();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "SaveID":
						instance.SaveID = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "SaveName":
						instance.SaveName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "SaveDate":
						instance.SaveDate = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "ActiveQuestName":
						instance.ActiveQuestName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "PlayTime":
						instance.PlayTime = reader.Read<System.Double>(ES3Type_double.Instance);
						break;
					case "LevelLoadingData":
						instance.LevelLoadingData = reader.Read<SunsetSystems.Core.SceneLoading.LevelLoadingData>(ES3UserType_LevelLoadingData.Instance);
						break;
					case "PlaylistData":
						instance.PlaylistData = reader.Read<SunsetSystems.Audio.ScenePlaylistData>(ES3UserType_ScenePlaylistData.Instance);
						break;
					case "SaveScreenShot":
						instance.SaveScreenShot = reader.Read<UnityEngine.Texture2D>(ES3Type_Texture2D.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}


	public class ES3UserType_SaveMetaDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SaveMetaDataArray() : base(typeof(SunsetSystems.Persistence.SaveMetaData[]), ES3UserType_SaveMetaData.Instance)
		{
			Instance = this;
		}
	}
}