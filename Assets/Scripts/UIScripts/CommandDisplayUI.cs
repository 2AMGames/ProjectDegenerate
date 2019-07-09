using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Draws all the inputs made by our player. This will only display player 1's inputs
/// </summary>
public class CommandDisplayUI : MonoBehaviour
{
    #region const variables
    private readonly Vector2 UP = Vector2.up;
    private readonly Vector2 DOWN = Vector2.down;
    private readonly Vector2 RIGHT = Vector2.right;
    private readonly Vector2 LEFT = Vector2.left;
    private readonly Vector2 UP_RIGHT = new Vector2(1, 1);
    private readonly Vector2 UP_LEFT = new Vector2(-1, 1);
    private readonly Vector2 DOWN_RIGHT = new Vector2(1, -1);
    private readonly Vector2 DOWN_LEFT = new Vector2(-1, -1);
    private readonly Vector2 NEUTRAL = Vector2.zero;

    #endregion const variables
    [Tooltip("")]
    public Color buttonPressedColor = Color.green;
    [Tooltip("")]
    public Color buttonReleasedColor = Color.white;
    [Tooltip("")]
    public Dictionary<string, Image> buttonImageDictionary = new Dictionary<string, Image>();
    public Image joystickImage;
    public Image cursorImage;


    #region monobehaviour methods
    private void Start()
    {
        PlayerController p = Overseer.Instance.players[0];
        if (p.CommandInterpreter)
        {
            p.CommandInterpreter.OnButtonPressedEvent += OnButtonPressed;
            p.CommandInterpreter.OnbuttonReleasedEvent += OnButtonReleased;
            p.CommandInterpreter.OnDirectionSetEvent += OnDirectionSet;

        }

    }
    #endregion monobehaviour methods

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonPressed(string buttonPressed)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonReleased(string buttonReleased)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    public void OnDirectionSet(CommandInterpreter.DIRECTION direction)
    {
        float sizeOfImage = joystickImage.rectTransform.sizeDelta.x /2;
        switch (direction)
        {
            case CommandInterpreter.DIRECTION.UP:
                cursorImage.rectTransform.localPosition = UP * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.DOWN:
                cursorImage.rectTransform.localPosition = DOWN * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.FORWARD:
                cursorImage.rectTransform.localPosition = RIGHT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.BACK:
                cursorImage.rectTransform.localPosition = LEFT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.FORWARD_UP:
                cursorImage.rectTransform.localPosition = UP_RIGHT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.FORWARD_DOWN:
                cursorImage.rectTransform.localPosition = DOWN_RIGHT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.BACK_UP:
                cursorImage.rectTransform.localPosition = UP_LEFT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.BACK_DOWN:
                cursorImage.rectTransform.localPosition = DOWN_LEFT * sizeOfImage;
                return;
            case CommandInterpreter.DIRECTION.NEUTRAL:
                cursorImage.rectTransform.localPosition = NEUTRAL * sizeOfImage;
                return;

        }
    }
}
