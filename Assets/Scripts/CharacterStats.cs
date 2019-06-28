using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public enum CharacterTeam
    {
        PLAYER_1,
        PLAYER_2,
        NEUTRAL,
    }
    public MovementMechanics MovementMechanics { get; private set; }

    [HideInInspector]
    public int PlayerIndex;

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
    }

}
