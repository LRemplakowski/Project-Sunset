using System;
using Sirenix.OdinInspector;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Party;
using SunsetSystems.Tooltips;
using SunsetSystems.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SunsetSystems.UI
{
    public class InventoryItemDisplay : SerializedMonoBehaviour, IUserInterfaceView<InventoryEntry>, IPointerEnterHandler, IPointerExitHandler
    {
        public static Action<InventoryItemDisplay> OnPointerEnterItem, OnPointerExitItem;

        [Title("Config")]
        [SerializeField, Required]
        private RectTransform _tooltipHookPoint;
        [SerializeField, Required]
        private Button _button;
        [SerializeField, Required]
        private Image _icon;
        [SerializeField, Required]
        private TextMeshProUGUI _stackSize;
        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        private InventoryEntry _itemEntry;
        [field: ShowInInspector, ReadOnly]
        public InventoryNameplateData TooltipData { get; private set; }

        private void Awake()
        {
            if (_tooltipHookPoint == null)
                _tooltipHookPoint = transform as RectTransform;
        }

        private void OnDisable()
        {
            if (_itemEntry.ItemReference != null)
                OnPointerExitItem?.Invoke(this);
        }

        private void Start()
        {
            TooltipData = new(this);
        }

        public void UpdateView(IUserInfertaceDataProvider<InventoryEntry> dataProvider)
        {
            ResetView();
            if (dataProvider != null)
            {
                _itemEntry = dataProvider.UIData;
                _icon.sprite = _itemEntry.ItemReference.Icon;
                _icon.gameObject.SetActive(true);
                if (_itemEntry.ItemReference.Stackable)
                {
                    _stackSize.gameObject.SetActive(true);
                    _stackSize.text = _itemEntry.StackSize.ToString();
                }
                else
                {
                    _stackSize.gameObject.SetActive(false);
                }
                _button.interactable = true;
            }
            else
            {
                _icon.gameObject.SetActive(false);
                _stackSize.gameObject.SetActive(false);
                _button.interactable = false;
            }
        }

        public void OnClick()
        {
            if (_itemEntry.ItemReference is IEquipableItem equipableItem)
            {
                if (InventoryManager.Instance.TakeItemFromPlayer(equipableItem, postLogMessage: false) is false)
                    return;
                string currentCharacterID = CharacterSelector.SelectedCharacterKey;
                var eqManager = PartyManager.Instance.GetPartyMemberByID(currentCharacterID).References.EquipmentManager;
                var slotID = eqManager.GetSlotForItem(equipableItem);
                if (eqManager.EquipItem(slotID, equipableItem, out var previouslyEquipped))
                {
                    if (ShouldAddItemBackToInventory(previouslyEquipped))
                        InventoryManager.Instance.GiveItemToPlayer(previouslyEquipped, postLogMessage: false);
                }
                else
                {
                    InventoryManager.Instance.GiveItemToPlayer(equipableItem, postLogMessage: false);
                }
            }
        }

        private bool ShouldAddItemBackToInventory(IEquipableItem item)
        {
            return item != null && !item.IsDefaultItem;
        }

        private void ResetView()
        {
            _icon.gameObject.SetActive(false);
            _itemEntry = new();
            _stackSize.gameObject.SetActive(false);
            _button.interactable = false;
        }

        public void OpentContextMenu()
        {
            //TODO: Actually make a context menu for an item instead of just equipping it
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_itemEntry.ItemReference != null)
                OnPointerEnterItem?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_itemEntry.ItemReference != null)
                OnPointerExitItem?.Invoke(this);
        }

        public class InventoryNameplateData : ITooltipContext
        {
            private readonly InventoryItemDisplay _dataSource;

            public GameObject TooltipSource => _dataSource.gameObject;
            public Vector3 TooltipPosition => _dataSource._tooltipHookPoint.position;
            public string ItemNameText => _dataSource._itemEntry.ItemReference?.Name;

            public InventoryNameplateData(InventoryItemDisplay dataSource)
            {
                _dataSource = dataSource;
            }
        }
    }
}
