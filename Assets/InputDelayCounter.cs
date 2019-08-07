using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class InputDelayCounter : MonoBehaviour
{
    #region const variables

    private const string MillisecondsText = "Milliseconds";

    private const string FramesText = "Frames";

    private const string Space = " ";

    #endregion

    #region main variables

    public Text FrameDelayText;

    #endregion

    void Start()
    {
        if (Overseer.Instance.SelectedGameType != Overseer.GameType.PlayerVsRemote)
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (FrameDelayText != null)
        {
            FrameDelayText.text = NetworkManager.Instance.CurrentDelayInMilliSeconds + Space + MillisecondsText + "\n" + "(" + NetworkManager.Instance.CurrentDelayFrames + Space + FramesText + ")";
        }
    }
}
