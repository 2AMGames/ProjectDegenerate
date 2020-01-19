using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    #region const variables

    private const float TimeUntilFadeout = 1.5f;

    private const float FadeoutTime = 1f;

    private const string HitString = "HITS";

    #endregion

    #region main variables

    public int CharacterId;

    public Text ComboText;

    public CharacterInteractionHandler InteractionHandler;

    public CharacterStats CharacterStats;

    private IEnumerator ComboEndCoroutine;

    #endregion

    #region monobehaviour methods

    private void Start()
    {
        CharacterStats = Overseer.Instance.PlayerObjects[CharacterId].GetComponent<CharacterStats>();
        InteractionHandler = CharacterStats.gameObject.GetComponent<CharacterInteractionHandler>();
        CharacterStats.OnMoveHit.AddListener(OnComboCountChanged);
        CharacterStats.OnComboFinished.AddListener(OnComboFinished);
        OnComboCountChanged();
    }

    #endregion

    #region public interface

    public void OnComboCountChanged()
    {
        if (ComboEndCoroutine != null)
        {
            StopCoroutine(ComboEndCoroutine);
        }
        ComboText.color = Color.white;
        ComboText.enabled = InteractionHandler.CurrentComboCount > 1;
        ComboText.text = InteractionHandler.CurrentComboCount + " " + HitString;
    }

    public void OnComboFinished()
    {
        if (InteractionHandler.CurrentComboCount > 1)
        {
            ComboEndCoroutine = ComboFadeOut();
            StartCoroutine(ComboEndCoroutine);
        }
    }

    #endregion

    #region private interface

    private IEnumerator ComboFadeOut()
    {
        yield return new WaitForSeconds(TimeUntilFadeout);

        float fadeOutTime = FadeoutTime;

        while (fadeOutTime > 0.0f)
        {
            if (Overseer.Instance.GameReady)
            {
                Color textColor = Color.white;
                textColor.a = fadeOutTime / FadeoutTime;
                ComboText.color = textColor;
                yield return new WaitForEndOfFrame();
                fadeOutTime -= Time.deltaTime;
            }
        }

        ComboText.enabled = false;

    }

    #endregion
}
