using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

/// <summary>
/// This will be the main start up screen. This is not really a main menu for the game. It is more to confirm who is the main player
/// 
/// Whoever presses start here should most likely be set to player 1 by default. We can change that later.
/// </summary>
public class SplashScreenUI : BaseSelectionUI
{
    public SceneField mainMenuSceneToLoad;

    #region monobehaviour methods

    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }
    #endregion monobehaviour methods


    #region button input events
    public void OnStartButtonPressed()
    {

    }
    #endregion button input events
}
