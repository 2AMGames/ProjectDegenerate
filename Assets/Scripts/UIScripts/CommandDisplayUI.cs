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
    public Image[] buttonImages;
    public Image joystickImage;
    public Image cursorImage;


    #region monobehaviour methods
    private void Start()
    {
        PlayerController p = Overseer.Instance.Players[0];
        if (p.CommandInterpreter)
        {
            p.CommandInterpreter.OnButtonPressedEvent += OnButtonPressed;
            p.CommandInterpreter.OnbuttonReleasedEvent += OnButtonReleased;
            p.CommandInterpreter.OnDirectionSetEvent += OnDirectionSet;

        }

        buttonImageDictionary.Add(CommandInterpreter.LP_ANIM_TRIGGER, buttonImages[0]);
        buttonImageDictionary.Add(CommandInterpreter.MP_ANIM_TRIGGER, buttonImages[1]);
        buttonImageDictionary.Add(CommandInterpreter.HP_ANIM_TRIGGER, buttonImages[2]);
        buttonImageDictionary.Add(CommandInterpreter.LK_ANIM_TRIGGER, buttonImages[3]);
        buttonImageDictionary.Add(CommandInterpreter.MK_ANIM_TRIGGER, buttonImages[4]);
        buttonImageDictionary.Add(CommandInterpreter.HK_ANIM_TRIGGER, buttonImages[5]);
    }
    #endregion monobehaviour methods

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonPressed(string buttonPressed)
    {
        buttonImageDictionary[buttonPressed].color = buttonPressedColor;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonReleased(string buttonReleased)
    {
        buttonImageDictionary[buttonReleased].color = buttonReleasedColor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    public void OnDirectionSet(CommandInterpreter.DIRECTION direction, Vector2Int directionOfJoystick)
    {
        float sizeOfImage = joystickImage.rectTransform.sizeDelta.x /2;
        cursorImage.rectTransform.localPosition = new Vector2(directionOfJoystick.x, directionOfJoystick.y) * sizeOfImage;
        switch (direction)
        {
            case CommandInterpreter.DIRECTION.UP:
                return;
            case CommandInterpreter.DIRECTION.DOWN:
                return;
            case CommandInterpreter.DIRECTION.FORWARD:
                return;
            case CommandInterpreter.DIRECTION.BACK:
                return;
            case CommandInterpreter.DIRECTION.FORWARD_UP:
                return;
            case CommandInterpreter.DIRECTION.FORWARD_DOWN:
                return;
            case CommandInterpreter.DIRECTION.BACK_UP:
                return;
            case CommandInterpreter.DIRECTION.BACK_DOWN:
                return;
            case CommandInterpreter.DIRECTION.NEUTRAL:
                
                return;
        }
    }
}
