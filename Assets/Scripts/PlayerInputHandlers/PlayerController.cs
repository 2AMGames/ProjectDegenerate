using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;
public abstract class PlayerController : MonoBehaviour
{

    #region const variables

    public const ushort DefaultInputPattern = 0xF000;

    public const string MOVEMENT_HORIZONTAL = "Horizontal_P";
    public const string MOVEMENT_VERTICAL = "Vertical_P";
    public const string MOVEMENT_JUMP = "Jump_";
    public const float INPUT_THRESHOLD_RUNNING = .6f;

    #endregion

    #region enum

    public enum PlayerType
    {
        Local,
        Remote,
        AI
    }

    #endregion

    #region member variables

    private PlayerController Opponent
    {
        get
        {
            return Overseer.Instance.GetNextCharacterByIndex(PlayerIndex);
        }
    }

    public Player AssociatedPlayer;

    public CharacterStats CharacterStats;

    public CharacterInteractionHandler InteractionHandler;

    public CommandInterpreter CommandInterpreter;

    public int PlayerIndex;


    #endregion

    #region player specific input keys

    protected KeyCode LightHitKey
    {
        get
        {
            if (PlayerIndex == 0)
            {
                return CustomInput.LightHitKey_Player1;
            }
            else
            {
                return CustomInput.LightHitKey_Player2;
            }
        }
    }

    protected KeyCode MediumHitKey
    {
        get
        {
            if (PlayerIndex == 0)
            {
                return CustomInput.MediumKey_Player1;
            }
            else
            {
                return CustomInput.MediumKey_Player2;
            }
        }
    }

    protected KeyCode HardHitKey
    {
        get
        {
            if (PlayerIndex == 0)
            {
                return CustomInput.HeavyKey_Player1;
            }
            else
            {
                return CustomInput.HeavyKey_Player2;
            }
        }
    }

    protected KeyCode SpecialHitKey
    {
        get
        {
            if (PlayerIndex == 0)
            {
                return CustomInput.SpecialKey_Player1;
            }
            else
            {
                return CustomInput.SpecialKey_Player2;
            }
        }
    }

    protected float HorizontalInputValue
    {
        get
        {
            return CustomInput.GetHorizontalAxis(PlayerIndex);
        }
    }

    protected float VerticalInputValue
    {
        get
        {
            return CustomInput.GetVerticalAxis(PlayerIndex);
        }
    }

    #endregion

    #region public interface

    public void SetPhotonPlayer(Player player)
    {
        AssociatedPlayer = player;
    }

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        CharacterStats.PlayerIndex = index;
    }

    #endregion

    #region virtual interface
    public virtual void ResetController()
    {

    }

    protected virtual void UpdateButtonInput(ref ushort inputPattern)
    {

    }

    protected virtual void UpdateJoystickInput(ref ushort inputPattern)
    {

    }

    protected virtual void OnGameReady(bool isReady)
    {
        enabled = isReady;
    }

    #endregion

}
