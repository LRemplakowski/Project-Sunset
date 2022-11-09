﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SunsetSystems.Journal;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.UI;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> { }

[Serializable]
public class StringFloatDictionary : SerializableDictionary<string, float> { }

[Serializable]
public class StringBoolDictionary : SerializableDictionary<string, bool> { }

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> { }

[Serializable]
public class StringQuestDictionary : SerializableDictionary<string, Quest> { }

[Serializable]
public class StringQuestDataDictionary : SerializableDictionary<string, QuestData> { }


[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> { }

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> { }

[Serializable]
public class StringEquipmentSlotDictionary : SerializableDictionary<string, EquipmentSlot> { }

[Serializable]
public class StringEquipmentDataDictionary : SerializableDictionary<string, EquipmentData> { }

[Serializable]
public class StringVector3Dictionary : SerializableDictionary<string, Vector3> { }

[Serializable]
public class StringEquipmentSlotDisplayDictionary : SerializableDictionary<string, EquipmentSlotDisplay> { }