using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    private static LoadSceneManager instance;

    public static LoadSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<LoadSceneManager>();
            }
            return instance;
        }
    }

    #region main ui variables
    [Header("UI Components")]
    public Image blackOutImage;

    private bool isLoadingScene;
    #endregion main ui variables




    private void Awake()
    {
        instance = this;
    }

    public void LoadNewScene(string sceneNameToLoad)
    {
        StartCoroutine(LoadNewSceneFadeToBlack(sceneNameToLoad));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeOutOfBlack()
    {
        yield break;
    }

    /// <summary>
    /// Use this to load in a new Scene. This will safely allow us to load a new scene without worrying about loading multiple scenes by mistake
    /// This will also handle any ui transitions that may occur
    /// </summary>
    /// <param name="sceneNameToLoad"></param>
    /// <param name="timeToFadeToBlack"></param>
    /// <returns></returns>
    protected IEnumerator LoadNewSceneFadeToBlack(string sceneNameToLoad, float timeToFadeToBlack = 1.5f)
    {
        if (isLoadingScene)
        {
            yield break;
        }
        blackOutImage.color = new Color(0, 0, 0, 0);
        Time.timeScale = 0;
        isLoadingScene = true;
        float timeThatHasPassed = 0;
        AsyncOperation loadLevelAsync = SceneManager.LoadSceneAsync(sceneNameToLoad);
        loadLevelAsync.allowSceneActivation = false;

        Color originalColor = blackOutImage.color;
        Color goalColor = Color.black;
        while (timeThatHasPassed < timeToFadeToBlack)
        {
            timeThatHasPassed += Time.unscaledDeltaTime;
            blackOutImage.color = Vector4.Lerp(originalColor, goalColor, timeThatHasPassed / timeToFadeToBlack);
            yield return null;
        }

        loadLevelAsync.allowSceneActivation = true;
    }
}
