using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class BaseSelectionUI : MonoBehaviour
{
    #region const variables
    public const float JOYSTICK_THRESHOLD = .65f;
    public const float TIME_BEFORE_BEGIN_SCROLLING = .25f;
    public const float TIME_BETWEEN_SELECT_OPTION = .1f;
    #endregion const variables

    public BaseSelectionNode currentBaseSelectionNode;
    private bool isAutoScrolling;

    #region monobehaviour methods

    private void OnEnable()
    {
        isAutoScrolling = false;
    }
    #endregion monobehaviour methods


    private void SetNextSelectionNode(Vector2Int inputDirection)
    {

    }

    /// <summary>
    /// Due to a lot of 
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoActionAfterOneFrame(Action action)
    {
        yield return null;
        action.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScrollThroughSelectionNodes()
    {
        isAutoScrolling = true;
        yield return null;
        isAutoScrolling = false;
    }
}
