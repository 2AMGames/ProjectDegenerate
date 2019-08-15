using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class InputDelayCounter : MonoBehaviour
{
    #region const variables

    private const string LocalDelay = "Local Delay";

    private const string MillisecondsText = "Milliseconds";

    private const string FramesText = "Frames";

    private const string Space = " ";

    private const string NewLineCharacter = "\n";

    #endregion

    #region main variables

    public Text FrameDelayText;

    #endregion

    void Start()
    {
        FrameDelayText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (FrameDelayText != null)
        {
            string textToDisplay = LocalDelay + ": " + GameStateManager.Instance.LocalFrameDelay + Space + "Frames" + NewLineCharacter;
            if (Overseer.Instance.IsNetworkedMode)
            {
               textToDisplay += NetworkManager.Instance.CurrentDelayInMilliSeconds + Space + MillisecondsText + NewLineCharacter + "(" + NetworkManager.Instance.TotalDelayFrames + Space + FramesText + ")";
            }
            FrameDelayText.text = textToDisplay;
        }
    }
}
