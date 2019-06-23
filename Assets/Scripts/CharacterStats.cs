using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{

    public MovementMechanics MovementMechanics { get; private set; }

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
    }

}
