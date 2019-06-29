using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandInterpreter : MonoBehaviour
{

    #region main variables

    public List<CharacterMove> AvailableMoves = new List<CharacterMove>();

    private Dictionary<string, CharacterMove> MoveDictionary = new Dictionary<string, CharacterMove>();

    #endregion

    #region monobehaviour methods

    void Start()
    {

    }

    void Update()
    {

    }

    private void Awake()
    {
        if (AvailableMoves != null)
        {
            MoveDictionary.Clear();
            foreach (CharacterMove move in AvailableMoves)
            {
                MoveDictionary.Add(move.InputPattern, move);
            }
        }
    }

    private void OnValidate()
    {
        if (AvailableMoves != null)
        {
            MoveDictionary.Clear();
            foreach (CharacterMove move in AvailableMoves)
            {
                MoveDictionary.Add(move.InputPattern, move);
            }
        }
    }

    #endregion
}
