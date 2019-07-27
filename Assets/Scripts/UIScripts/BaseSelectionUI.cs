using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class BaseSelectionUI : MonoBehaviour
{
    public BaseSelectionNode currentBaseSelectionNode;



    private void SetNextSelectionNode()
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
        yield return null;
    }
}
