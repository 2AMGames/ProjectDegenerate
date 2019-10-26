using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using UnityEngine.UI;

/// <summary>
/// This will be the main start up screen. This is not really a main menu for the game. It is more to confirm who is the main player
/// 
/// Whoever presses start here should most likely be set to player 1 by default. We can change that later.
/// </summary>
public class SplashScreenUI : MonoBehaviour
{
    private const float TIME_TO_FADE_SCENE = .5f;
    public Image fadeToBlackImage;
    public SceneField mainMenuSceneToLoad;

    #region monobehaviour methods

    private void Awake()
    {
        
    }

    private void Update()
    {

        if (GetStartButtonPressed())
        {
            StartCoroutine(LoadNewScene());
        }
    }
    #endregion monobehaviour methods


    #region button input events
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool GetStartButtonPressed()
    {
        return Input.GetButtonDown(BaseSelectionUI.ACCEPT_BUTTON);
    }

    private void OnStartButtonPressed()
    {

    }
    #endregion button input events

    private IEnumerator LoadNewScene()
    {


        float timeThatHasPassed = 0;
        AsyncOperation asynLoad = SceneManager.LoadSceneAsync(mainMenuSceneToLoad.SceneName);
        asynLoad.allowSceneActivation = false;

        Color originalColor = fadeToBlackImage.color;
        Color goalColor = new Vector4(0, 0, 0, 1);
        while (timeThatHasPassed < TIME_TO_FADE_SCENE)
        {
            timeThatHasPassed += Time.unscaledDeltaTime;
            fadeToBlackImage.color = Vector4.Lerp(originalColor, goalColor, timeThatHasPassed / TIME_TO_FADE_SCENE);
            yield return null;
        }
        fadeToBlackImage.color = goalColor;
        asynLoad.allowSceneActivation = true;
    }

}
