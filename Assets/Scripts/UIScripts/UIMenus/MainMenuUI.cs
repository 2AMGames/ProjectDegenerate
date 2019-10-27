using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class MainMenuUI : BaseSelectionUI
{
    public SceneField splashScreenScene;
    public SceneField mainSceneToLoad;

    #region monobehaviour methods
    protected override void Update()
    {
        base.Update();
        if (GetCancelButtonDown())
        {

            LoadSceneManager.Instance.LoadNewScene(splashScreenScene.SceneName);
        }
    }
    #endregion monobehaviour methods

    #region button event methods
    /// <summary>
    /// This will open a screen to select your characters
    /// </summary>
    public void OnPlayGameLocalPressed()
    {
        LoadSceneManager.Instance.LoadNewScene(mainSceneToLoad.SceneName);
    }

    /// <summary>
    /// This will open up a screen that connects you to an online game. You will have to select your
    /// character before hand most likely
    /// </summary>
    public void OnPlayGameOnlinePressed()
    {
        LoadSceneManager.Instance.LoadNewScene(mainSceneToLoad.SceneName);
    }

    /// <summary>
    /// This will not load a new scene. Instead it will open up a 
    /// </summary>
    public void OnOptionsMenuButtonPressed()
    {
        print("Menu Button Pressed. Does nothing for now...");
    }

    public void OnQuitGamePressed()
    {
        Application.Quit();
    }
    #endregion button event methods
}
