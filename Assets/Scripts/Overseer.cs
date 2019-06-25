using UnityEngine;

public class Overseer : MonoBehaviour
{

    #region member variables

    private static Overseer instance;
    public static Overseer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Overseer>();
            }
            if (instance == null)
            {
                GameObject container = new GameObject("Overseer");
                instance = container.AddComponent<Overseer>();
            }
            return instance;
        }
    }

    public PlayerController[] players;

    #endregion

    #region monobehaviour methods
    void Awake()
    {
        instance = this;
        AssignPlayerIndices();

    }

    #endregion

    #region public interface

    public PlayerController GetOpponentCharacter(int index)
    {
        int indexToGet = (index + 1) % players.Length;
        return players[indexToGet];
    }

    #endregion

    #region private interface

    private void AssignPlayerIndices()
    {
        for(int index = 0; index < players.Length; ++index)
        {
            PlayerController controller = players[index];
            if (controller != null)
            {
                controller.SetPlayerIndex(index);
            }
        }
    }

    #endregion
}
