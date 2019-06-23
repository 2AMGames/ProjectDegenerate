using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileColliderSloped : TileCollider {
    protected override void Start()
    {
        base.Start();
        canHitWhileMovingLeft = false;
        canHitWhileMovingRight = false;
        canHitWhileMovingUp = false;
    }


    
}
