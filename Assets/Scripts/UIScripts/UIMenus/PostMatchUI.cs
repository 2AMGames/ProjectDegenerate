using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
public class PostMatchUI : BaseSelectionUI
{
    [SerializeField]
    private SceneField MainMenuScene;

    public void OnRematchPressed()
    {
        Debug.LogWarning("Rematch pressed: " + SceneManager.GetActiveScene().name);
        LoadSceneManager.Instance.LoadNewScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitPressed()
    {
        Debug.LogWarning("Quit pressed");
        LoadSceneManager.Instance.LoadNewScene(MainMenuScene.SceneName);
    }
}
