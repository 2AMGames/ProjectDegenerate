using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region const variables

    private const string MOVEMENT_HORIZONTAL = "Horizontal";
    private const string MOVEMENT_VERTICAL = "Vertical";
    private const string MOVEMENT_JUMP = "Jump";

    #endregion

    #region member variables

    public CharacterStats CharacterStats;

    #endregion

    private void Update()
    {
        CharacterStats.MovementMechanics.SetHorizontalInput(Input.GetAxisRaw(MOVEMENT_HORIZONTAL));
        CharacterStats.MovementMechanics.SetVerticalInput(Input.GetAxisRaw(MOVEMENT_VERTICAL));
        
        if (Input.GetButtonDown(MOVEMENT_JUMP))
        {
            CharacterStats.MovementMechanics.Jump();
        }

    }

}
