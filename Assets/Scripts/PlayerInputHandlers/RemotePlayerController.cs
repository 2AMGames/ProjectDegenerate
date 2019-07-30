using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RemotePlayerController : PlayerController, IOnEventCallback
{

    #region player controller override methods

    // Pattern: 
    // Bit 0: LP
    // Bit 1: MP
    // Bit 2: HP
    // Bit 3: LK
    // Bit 4: MK
    // Bit 5: HK
    // Bit 6: Left Directional Input
    // Bit 7: Right Directional Input
    // Bit 8: Up Directional Input
    // Bit 9: Down Directional Input

    protected override void UpdateButtonInput()
    {

    }

    protected override Vector2Int UpdateJoystickInput()
    {
        float horizontal = Input.GetAxisRaw(HorizontalInputKey);
        float vertical = Input.GetAxisRaw(VerticalInputKey);

        int horizontalInputAsInt = 0;
        int verticalInputAsInt = 0;

        if (Mathf.Abs(horizontal) > PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            horizontalInputAsInt = (int)Mathf.Sign(horizontal);
        }

        if (Mathf.Abs(vertical) > PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            verticalInputAsInt = (int)Mathf.Sign(vertical);
        }
        return new Vector2Int(horizontalInputAsInt, verticalInputAsInt);
    }

    #endregion

    #region monobehaviour methods

    public void Update()
    {
        
    }

    public void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    #endregion

    #region photon event callback

    public void OnEvent(ExitGames.Client.Photon.EventData eventData)
    {
        if (eventData.Code == NetworkManager.PlayerInputUpdate)
        {
            try
            {
                PlayerInputData data = eventData.CustomData as PlayerInputData;

                if (data != null && data.PlayerIndex == PlayerIndex && Overseer.Instance.IsGameReady)
                {
                    base.UpdateButtonsFromInputData(data);
                    CommandInterpreter.UpdateJoystickInput(base.GetJoystickInputFromData(data));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    #endregion
}
