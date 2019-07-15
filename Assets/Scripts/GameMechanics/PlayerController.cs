using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region const variables

    public const string MOVEMENT_HORIZONTAL = "Horizontal_P";
    public const string MOVEMENT_VERTICAL = "Vertical_P";
    public const string MOVEMENT_JUMP = "Jump_";
    public const float INPUT_THRESHOLD_RUNNING = .6f;

    #endregion

    #region member variables

    public CharacterStats CharacterStats;

    public InteractionHandler InteractionHandler;

    public CommandInterpreter CommandInterpreter;

    [HideInInspector]
    public int PlayerIndex;

    private string HorizontalInputKey
    {
        get
        {
            return MOVEMENT_HORIZONTAL + (PlayerIndex + 1);
        }
    }

    private string VerticalInputKey
    {
        get
        {
            return MOVEMENT_VERTICAL + (PlayerIndex + 1);
        }
    }

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
        CharacterStats.MovementMechanics.SetHorizontalInput(Input.GetAxisRaw(HorizontalInputKey));
        CharacterStats.MovementMechanics.SetVerticalInput(Input.GetAxisRaw(VerticalInputKey));
        
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
        CommandInterpreter.PlayerIndex = index;
    }

    #endregion

}
