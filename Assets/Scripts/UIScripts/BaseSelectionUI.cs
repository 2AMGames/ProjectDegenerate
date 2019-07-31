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


    #region UI input names
    public const string HORIZONTAL_AXIS = "Horizontal";
    public const string VERTICAL_AXIS = "Vertical";

    public const string ACCEPT_BUTTON = "Submit";
    public const string CANCEL_BUTTON = "Cancel";
    #endregion UI input names
    #endregion const variables

    public BaseSelectionNode initialNode;
    public BaseSelectionNode currentBaseSelectionNode;
    private bool isAutoScrolling;

    #region monobehaviour methods

    private void Update()
    {
        if (!isAutoScrolling)
        {
            float x = GetHorizontalAxis();
            float y = GetVerticalAxis();

            if (Mathf.Abs(x) > JOYSTICK_THRESHOLD)
            {
                StartCoroutine(ScrollThroughSelectionNodes(new Vector2Int((int)Mathf.Sign(x), 0)));
            }
            else if (Mathf.Abs(y) > JOYSTICK_THRESHOLD)
            {
                StartCoroutine(ScrollThroughSelectionNodes(new Vector2Int(0, (int)Mathf.Sign(y))));
            }
        }
        
    }

    private void OnEnable()
    {
        isAutoScrolling = false;
    }

    
    #endregion monobehaviour methods
    /// <summary>
    /// 
    /// </summary>
    public virtual void OpenSelectionMenu()
    {
        SetNodeToCurrent(initialNode);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputDirection"></param>
    private void SetNextSelectionNode(Vector2Int inputDirection)
    {
        if (inputDirection.x > 0)
        {
            SetNodeToCurrent(currentBaseSelectionNode.GetNodeInDirection(BaseSelectionNode.DIRECTION.RIGHT));
        }
        else if (inputDirection.x < 0)
        {
            SetNodeToCurrent(currentBaseSelectionNode.GetNodeInDirection(BaseSelectionNode.DIRECTION.LEFT));
        }

        if (inputDirection.y > 0)
        {
            SetNodeToCurrent(currentBaseSelectionNode.GetNodeInDirection(BaseSelectionNode.DIRECTION.UP));
        }
        else if (inputDirection.y < 0)
        {
            SetNodeToCurrent(currentBaseSelectionNode.GetNodeInDirection(BaseSelectionNode.DIRECTION.DOWN));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nodeToSet"></param>
    private void SetNodeToCurrent(BaseSelectionNode nodeToSet)
    {
        if (nodeToSet == null)
        {
            return;
        }
        if (currentBaseSelectionNode != null)
        {
            currentBaseSelectionNode.enabled = false;
        }
        nodeToSet.enabled = true;
        currentBaseSelectionNode = nodeToSet;
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
    private IEnumerator ScrollThroughSelectionNodes(Vector2Int axisInput)
    {
        isAutoScrolling = true;
        float timeThatHasPassed = 0;
        SetNextSelectionNode(axisInput);
        while (timeThatHasPassed < TIME_BEFORE_BEGIN_SCROLLING)
        {
            timeThatHasPassed += Time.unscaledDeltaTime;
            if (!CompareInputVectorToCurrentJoystickDirection(axisInput))
            {
                isAutoScrolling = false;
                yield break;
            }
            yield return null;
        }
        SetNextSelectionNode(axisInput);
        timeThatHasPassed = 0;
        while (CompareInputVectorToCurrentJoystickDirection(axisInput))
        {
            if (timeThatHasPassed > TIME_BETWEEN_SELECT_OPTION)
            {
                timeThatHasPassed = 0;
                SetNextSelectionNode(axisInput);
            }
            timeThatHasPassed += Time.unscaledDeltaTime;
        }
        isAutoScrolling = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CompareInputVectorToCurrentJoystickDirection(Vector2Int inputDirection)
    {
        Vector2Int currentInputDirection = new Vector2Int();
        float horizontalAxis = GetHorizontalAxis();
        float verticalAxis = GetVerticalAxis();
        if (Mathf.Abs(horizontalAxis) > JOYSTICK_THRESHOLD)
        {
            currentInputDirection.x = (int)Mathf.Sign(horizontalAxis);
        }
        if (Mathf.Abs(verticalAxis) > JOYSTICK_THRESHOLD)
        {
            currentInputDirection.y = (int)Mathf.Sign(verticalAxis);
        }

        return currentInputDirection == inputDirection;
    }


    #region input methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetVerticalAxis()
    {
        return Input.GetAxisRaw(VERTICAL_AXIS);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetHorizontalAxis()
    {
        return Input.GetAxisRaw(HORIZONTAL_AXIS);
    }
    #endregion input methods
}
