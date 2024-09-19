using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPathfinding : MonoBehaviour
{

    //Default speed at which the enemy is moving at
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D rb; //Rigidbody2D component giving control to the physics of the monster
    private Vector2 movingDirection; //Direction the monster will move 
    private Knockback knockback;

    //Loaded when the script is first loaded
    private void Awake()
    {
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>(); //Call the rigidbody2d component
    }

    //Used to calculate the direction and movement speed for the game physics 
    private void FixedUpdate()
    {
        if (knockback.gettingPushedBack) { return; }
        rb.MovePosition(rb.position + movingDirection * (moveSpeed * Time.fixedDeltaTime));
    }

    //Setting the enemy to move to the targeted position
    public void MoveTo(Vector2 targetPosition)
    {
        movingDirection = targetPosition;
    }
}
