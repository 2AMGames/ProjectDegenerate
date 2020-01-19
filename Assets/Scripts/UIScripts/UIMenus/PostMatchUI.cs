using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Utilities;
public class PostMatchUI : BaseSelectionUI
{

    #region const fields

    private const string PlayerWinText = "Player {0} wins";

    private const string DrawMatchText = "DRAW";

    #endregion

    #region member variables
    [SerializeField]
    private SceneField MainMenuScene;

    [SerializeField]
    private Text MatchResultText;

    #endregion

    public void SetupPanel(PlayerController winningPlayer)
    {
        MatchResultText.text = winningPlayer != null ? string.Format(PlayerWinText, winningPlayer.PlayerIndex + 1).ToUpper() : DrawMatchText;
    }

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
