using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region const variables

    public const string MOVEMENT_HORIZONTAL = "Horizontal";
    public const string MOVEMENT_VERTICAL = "Vertical";
    public const string MOVEMENT_JUMP = "Jump";
    public const float INPUT_THRESHOLD_RUNNING = .6f;

    #endregion

    #region member variables

    public CharacterStats CharacterStats;

    public InteractionHandler InteractionHandler;

    public CommandInterpreter CommandInterpreter;

    [HideInInspector]
    public int PlayerIndex;

    private PlayerController Opponent
    {
        get
        {
            return Overseer.Instance.GetNextCharacterByIndex(PlayerIndex);
        }
    }

    #endregion

    #region monobehaviour methods

    private void Update()
    {
        CharacterStats.MovementMechanics.SetHorizontalInput(Input.GetAxisRaw(MOVEMENT_HORIZONTAL));
        CharacterStats.MovementMechanics.SetVerticalInput(Input.GetAxisRaw(MOVEMENT_VERTICAL));
        
        //if (Input.GetButtonDown(MOVEMENT_JUMP))
        //{
        //    CharacterStats.MovementMechanics.Jump();
        //}

        CharacterStats.MovementMechanics.FlipSpriteBasedOnOpponentDirection(Opponent.CharacterStats.transform);

    }

    #endregion

    #region public interface

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        CharacterStats.PlayerIndex = index;
    }

    #endregion

}
