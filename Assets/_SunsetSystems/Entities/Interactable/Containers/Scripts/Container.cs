using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.UI;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using System;

namespace SunsetSystems.Entities.Interactable
{
    [RequireComponent(typeof(ItemStorage))]
    public class Container : SerializedMonoBehaviour, IInteractionHandler
    {
        [SerializeField]
        private ItemStorage _myInventory;
        private ContainerGUI _containerGUI;

        protected void Awake()
        {
            if (!_myInventory)
                _myInventory = GetComponent<ItemStorage>();
        }

        public bool HandleInteraction(IActionPerformer interactee)
        {
            OpenContainer();
            return true;
        }

        private void OpenContainer()
        {
            throw new NotImplementedException();
            //if (this.TryFindFirstGameObjectWithTag(TagConstants.CONTAINER_GUI, out GameObject guiGO))
            //{
            //    if (guiGO.TryGetComponent(out _containerGUI))
            //    {
            //        _containerGUI.OpenContainerGUI(_myInventory, "TEST");
            //    }
            //}
        }
    }
}
