using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("<DatabaseID>k__BackingField", "<ReadableID>k__BackingField", "<FirstName>k__BackingField", "<LastName>k__BackingField", "<Faction>k__BackingField", "<BodyType>k__BackingField", "<CreatureType>k__BackingField", "<BaseLookWardrobeReadableID>k__BackingField", "<EquipmentSlotsData>k__BackingField", "<StatsData>k__BackingField", "<KnownPowers>k__BackingField")]
	public class ES3UserType_TemplateFromInstance : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_TemplateFromInstance() : base(typeof(SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)obj;
			
			writer.WritePrivateField("<DatabaseID>k__BackingField", instance);
			writer.WritePrivateField("<ReadableID>k__BackingField", instance);
			writer.WritePrivateField("<FirstName>k__BackingField", instance);
			writer.WritePrivateField("<LastName>k__BackingField", instance);
			writer.WritePrivateField("<Faction>k__BackingField", instance);
			writer.WritePrivateField("<BodyType>k__BackingField", instance);
			writer.WritePrivateField("<CreatureType>k__BackingField", instance);
			writer.WritePrivateField("<BaseLookWardrobeReadableID>k__BackingField", instance);
			writer.WritePrivateField("<EquipmentSlotsData>k__BackingField", instance);
			writer.WritePrivateField("<StatsData>k__BackingField", instance);
			writer.WritePrivateField("<KnownPowers>k__BackingField", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "<DatabaseID>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<DatabaseID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "<ReadableID>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<ReadableID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "<FirstName>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<FirstName>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "<LastName>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<LastName>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "<Faction>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<Faction>k__BackingField", reader.Read<Faction>(), instance);
					break;
					case "<BodyType>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<BodyType>k__BackingField", reader.Read<SunsetSystems.Entities.Characters.BodyType>(), instance);
					break;
					case "<CreatureType>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<CreatureType>k__BackingField", reader.Read<CreatureType>(), instance);
					break;
					case "<BaseLookWardrobeReadableID>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<BaseLookWardrobeReadableID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "<EquipmentSlotsData>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<EquipmentSlotsData>k__BackingField", reader.Read<System.Collections.Generic.Dictionary<SunsetSystems.Equipment.EquipmentSlotID, System.String>>(), instance);
					break;
					case "<StatsData>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<StatsData>k__BackingField", reader.Read<SunsetSystems.Entities.Data.StatsData>(), instance);
					break;
					case "<KnownPowers>k__BackingField":
					instance = (SunsetSystems.Entities.Characters.Creature.TemplateFromInstance)reader.SetPrivateField("<KnownPowers>k__BackingField", reader.Read<System.Collections.Generic.List<System.String>>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new SunsetSystems.Entities.Characters.Creature.TemplateFromInstance();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_TemplateFromInstanceArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_TemplateFromInstanceArray() : base(typeof(SunsetSystems.Entities.Characters.Creature.TemplateFromInstance[]), ES3UserType_TemplateFromInstance.Instance)
		{
			Instance = this;
		}
	}
}