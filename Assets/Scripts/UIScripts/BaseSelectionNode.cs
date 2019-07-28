using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSelectionNode : MonoBehaviour
{

    #region enums
    public enum DIRECTION
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
    }
    #endregion enums

    #region main variables
    public BaseSelectionNode selectionNodeLeft;
    public BaseSelectionNode selectionNodeRight;
    public BaseSelectionNode selectionNodeUp;
    public BaseSelectionNode selectionNodeDown;
    #endregion main variables


    public BaseSelectionNode GetNodeInDirection(DIRECTION direction)
    {
        switch (direction)
        {
            case DIRECTION.UP:

                return selectionNodeUp;
            case DIRECTION.DOWN:

                return selectionNodeDown;
            case DIRECTION.LEFT:

                return selectionNodeLeft;
            case DIRECTION.RIGHT:

                return selectionNodeRight;
        }

        return null;
    }
}
