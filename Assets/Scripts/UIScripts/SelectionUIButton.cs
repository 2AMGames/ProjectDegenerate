using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 
/// </summary>
public class SelectionUIButton : BaseSelectionNode
{
    private const string ACCEPT_BUTTON = "Submit";

    public Text buttonTitleText;
    public Image buttonImage;

    public UnityEvent onButtonPressedEvent;
    public UnityEvent onButtonReleasedEvent;

    #region monobehaviour methods
    private void Update()
    {
        if (GetAcceptButtonDown())
        {
            onButtonPressedEvent.Invoke();
        }
        if (GetAcceptButtonUp())
        {
            onButtonReleasedEvent.Invoke();
        }
    }

    #endregion monobehaviour methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GetAcceptButtonDown()
    {
        return Input.GetButtonDown(ACCEPT_BUTTON);
    }

    public bool GetAcceptButtonUp()
    {
        return Input.GetButtonUp(ACCEPT_BUTTON);
    }
}
