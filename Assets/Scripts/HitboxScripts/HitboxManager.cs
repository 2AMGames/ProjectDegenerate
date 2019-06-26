using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    public static List<Hitbox> allActiveHitboxes = new List<Hitbox>();


    #region monobehavoiur methods
    private void Awake()
    {
        Overseer.Instance.hitboxManager = this;
    }

    private void Update()
    {
        
    }

    private void CheckHitboxCollide(Hitbox h1, Hitbox h2)
    {

    }

    #endregion monobehaviour methods
}
