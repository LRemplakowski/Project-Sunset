﻿using System.Collections;
using System.Collections.Generic;
using Systems.Management;
using UnityEngine;

public class DialogueEntity : InteractableEntity
{
    [SerializeField]
    private TextAsset entityDialogue;

    public override void Interact()
    {
        ReferenceManager.GetManager<DialogueManager>().StartDialogue(entityDialogue);
        base.Interact();
    }
}
