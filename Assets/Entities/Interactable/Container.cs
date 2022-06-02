﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.UI;

namespace Entities.Interactable
{
    [RequireComponent(typeof(ItemStorage))]
    public class Container : InteractableEntity
    {
        [SerializeField]
        private ItemStorage _myInventory;
        private ContainerGUI _containerGUI;

        private void Awake()
        {
            if (!_myInventory)
                _myInventory = GetComponent<ItemStorage>();
        }

        public override void Interact()
        {
            OpenContainerUI();
            base.Interact();
        }

        private void OpenContainerUI()
        {
            if (this.TryFindFirstWithTag(TagConstants.CONTAINER_GUI, out GameObject guiGO))
            {
                if (guiGO.TryGetComponent(out _containerGUI))
                {
                    _containerGUI.OpenContainerGUI(_myInventory, "TEST");
                }
            }
        }
    }
}
