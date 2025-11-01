using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("_tracks", "serializationData")]
	public class ES3UserType_PlaylistConfig : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlaylistConfig() : base(typeof(SunsetSystems.Audio.PlaylistConfig)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (SunsetSystems.Audio.PlaylistConfig)obj;
			
			writer.WritePrivateField("_tracks", instance);
			writer.WritePrivateField("serializationData", instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (SunsetSystems.Audio.PlaylistConfig)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "_tracks":
					instance = (SunsetSystems.Audio.PlaylistConfig)reader.SetPrivateField("_tracks", reader.Read<System.Collections.Generic.List<UnityEngine.AddressableAssets.AssetReference>>(), instance);
					break;
					case "serializationData":
					instance = (SunsetSystems.Audio.PlaylistConfig)reader.SetPrivateField("serializationData", reader.Read<Sirenix.Serialization.SerializationData>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_PlaylistConfigArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlaylistConfigArray() : base(typeof(SunsetSystems.Audio.PlaylistConfig[]), ES3UserType_PlaylistConfig.Instance)
		{
			Instance = this;
		}
	}
}