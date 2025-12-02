using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Party;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UI.CharacterPortraits
{
    public class PartyPortraitsController : SerializedMonoBehaviour
    {
        [SerializeField]
        private GameObject _portraitPrefab;
        [ShowInInspector, ReadOnly]
        private readonly List<PortraitController> _portraits = new();

        public void OnPartyInitialized()
        {
            var activeParty = PartyManager.Instance.ActiveParty;
            foreach (var member in activeParty)
            {
                AddPortrait(member.References.CreatureData.Portrait, member.References.CreatureData.DatabaseID);
            }
        }

        public void AddPortrait(Sprite portrait, string characterID)
        {
            GameObject portraitGO = Instantiate(_portraitPrefab, this.transform);
            if (!portraitGO.TryGetComponent<PortraitController>(out var portraitController))
                Debug.LogWarning("jebany null");
            portraitController.InitPotrait(portrait, characterID);
            _portraits.Add(portraitController);
        }

        public void Clear()
        {
            _portraits.ForEach(p => Destroy(p.gameObject));
            _portraits.Clear();
        }

        private struct PortraitData
        {
            public AssetReferenceSprite AssetRef;
            public Sprite AssetInstance;
        }
    }
}
