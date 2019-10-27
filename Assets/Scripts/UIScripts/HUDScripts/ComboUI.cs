using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    #region const variables

    private const float TimeUntilFadeout = .43f;

    private const string HitString = "HITS";

    #endregion

    #region main variables

    public int CharacterId;

    public Text ComboText;

    public InteractionHandler InteractionHandler;

    public CharacterStats CharacterStats;

    #endregion

    #region monobehaviour methods

    private void Start()
    {
        CharacterStats = Overseer.Instance.PlayerObjects[CharacterId].GetComponent<CharacterStats>();
        InteractionHandler = CharacterStats.gameObject.GetComponent<InteractionHandler>();
        CharacterStats.OnMoveHit.AddListener(OnComboCountChanged);
        OnComboCountChanged();
    }

    #endregion

    #region public interface

    public void OnComboCountChanged()
    {
        ComboText.enabled = InteractionHandler.CurrentComboCount > 1;
        ComboText.text = InteractionHandler.CurrentComboCount + " " + HitString;
    }

    public void OnComboEnded()
    {
        // TODO Add coroutine to fade out text.
    }

    #endregion
}
