using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugTestVelocity : MonoBehaviour
{
    public enum Player
    {
        Player1,
        Player2,
    }
    public float maxVelocity = 5;
    public float acceleration = 2;
    public Player player;
    CustomPhysics2D rigid;

    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
    }

    private void Update()
    {
        Vector2 goalVelocity = Vector2.zero;
        switch (player)
        {
            case Player.Player1:
                goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P1") * maxVelocity, Input.GetAxisRaw("Vertical_P1") * maxVelocity);
                break;
            case Player.Player2:
                goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P2") * maxVelocity, Input.GetAxisRaw("Vertical_P2") * maxVelocity);
                break;
        }
        rigid.velocity = Vector2.MoveTowards(rigid.velocity, goalVelocity, Overseer.DELTA_TIME * acceleration);
    }
}
