﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : ContextAction
{
    public override void OnClick()
    {
        Debug.Log("Dropping item "+item.name);
        item.DropOnGround(GameManager.player.transform.position);
        GameManager.player.inventory.Remove(item);
    }
}
