using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkInputHandler : MonoBehaviour, IMatchmakingCallbacks
{

    #region main variables

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    #endregion

    #region Callbacks
    public void OnCreatedRoom()
    {
        //
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        //
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        //
    }

    public void OnJoinedRoom()
    {
        Debug.LogWarning("Joined room");
        PlayerController.PlayerIndex = PhotonNetwork.CurrentRoom.PlayerCount;
        StartCoroutine(SendInputIfNeccessary());
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        //
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        //
    }

    public void OnLeftRoom()
    {
        //
    }

    #endregion

    #region monobehaviour methods

    void Update()
    {
        
    }

    void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        CommandInterpreter = PlayerController.CommandInterpreter;

        PhotonNetwork.AddCallbackTarget(this);
    }

    #endregion

    #region private interface

    private IEnumerator SendInputIfNeccessary()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            PlayerInputData inputData = CommandInterpreter.GetPlayerInputDataIfUpdated();
            if (inputData != null)
            {
                Debug.LogWarning("Sending input data");
                inputData.PlayerIndex = PlayerController.PlayerIndex;
                NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, inputData);
            }
        }
    }

    #endregion
}
