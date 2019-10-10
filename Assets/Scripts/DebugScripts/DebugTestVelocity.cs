using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class DebugTestVelocity : MonoBehaviour
{
    public enum Player
    {
        Player1,
        Player2,
    }
    public float maxVelocity = 5;
    public float acceleration = 2;
    public float jumpVelocity = 5;
    public Player player;
    public bool simulateGravity;
    CustomPhysics2D rigid;


    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
    }

    private void OnValidate()
    {
        if (!rigid)
        {
            rigid = GetComponent<CustomPhysics2D>();
        }
        rigid.useGravity = simulateGravity;
    }

    private void Update()
    {
        Vector2 goalVelocity = Vector2.zero;
        switch (player)
        {
            case Player.Player1:
                if (!simulateGravity)
                {
                    goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P1"), Input.GetAxisRaw("Vertical_P1")) * maxVelocity;

                }
                else
                {
                    goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P1") * maxVelocity, rigid.Velocity.y);
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        JumpPlayer();
                    }
                }

                break;
            case Player.Player2:
                if (!simulateGravity)
                {
                    goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P2"), Input.GetAxisRaw("Vertical_P2")) * maxVelocity;
                }
                else
                {
                    goalVelocity = new Vector2(Input.GetAxisRaw("Horizontal_P2") * maxVelocity, rigid.Velocity.y);
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        JumpPlayer();
                    }
                }

                break;
        }
        rigid.Velocity = Vector2.MoveTowards(rigid.Velocity, goalVelocity, Overseer.DELTA_TIME * acceleration);
    }

    private void JumpPlayer()
    {
        rigid.Velocity.y = jumpVelocity;
    }
}
