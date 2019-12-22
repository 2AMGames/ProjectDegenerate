using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Draws all the inputs made by our player. This will only display player 1's inputs
/// </summary>
public class CommandDisplayUI : MonoBehaviour
{
    

    [Tooltip("Color of the button while it is currently being held")]
    public Color buttonPressedColor = Color.green;
    [Tooltip("The color of the button when it is not currenltly being held")]
    public Color buttonReleasedColor = Color.white;
    [Tooltip("Dictionary of images that represent the buttons that the player currently is pressing. The key is the button value that is being pressed")]
    public Dictionary<int, Image> buttonImageDictionary = new Dictionary<int, Image>();
    [Tooltip("List of all the buttons that are displayed in our button pressed UI")]
    public Image[] buttonImages;
    [Tooltip("The images of our joystick crosshair. Really just need this to map out the bounds of the image so that the cursor knows where to go when at max range")]
    public Image joystickImage;
    [Tooltip("The pointer that visually represents where you're joystick is being held")]
    public Image cursorImage;


    #region monobehaviour methods

    private void Awake()
    {
        this.gameObject.SetActive(Overseer.Instance.DebugEnabled);
        Overseer.Instance.OnGameReady += OnGameReady;
    }
    private void Start()
    {

        buttonImageDictionary.Add(CommandInterpreter.LP_ANIM_TRIGGER, buttonImages[0]);
        buttonImageDictionary.Add(CommandInterpreter.MP_ANIM_TRIGGER, buttonImages[1]);
        buttonImageDictionary.Add(CommandInterpreter.HP_ANIM_TRIGGER, buttonImages[2]);
        buttonImageDictionary.Add(CommandInterpreter.LK_ANIM_TRIGGER, buttonImages[3]);
        buttonImageDictionary.Add(CommandInterpreter.SPECIAL_ANIM_TRIGGER, buttonImages[4]);
        buttonImageDictionary.Add(CommandInterpreter.HK_ANIM_TRIGGER, buttonImages[5]);
    }
    #endregion monobehaviour methods

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonPressed(int buttonPressed)
    {
        buttonImageDictionary[buttonPressed].color = buttonPressedColor;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnButtonReleased(int buttonReleased)
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

    private void OnGameReady(bool isGameReady)
    {
        if (isGameReady)
        {
            LinkPlayerWithDisplay();
        }
    }

    private void LinkPlayerWithDisplay()
    {
        PlayerController p = Overseer.Instance.Players[0];
        if (p.CommandInterpreter)
        {
            p.CommandInterpreter.OnButtonPressedEvent += OnButtonPressed;
            p.CommandInterpreter.OnButtonReleasedEvent += OnButtonReleased;
            p.CommandInterpreter.OnDirectionSetEvent += OnDirectionSet;

        }
    }
}
