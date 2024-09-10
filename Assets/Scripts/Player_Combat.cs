using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
     Developed by: Dev
     
     > Player_Combat class
       - Handles player combat with both enemy players AND monsters

 */

public class Player_Combat : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
        
    }

    void Attack()
    {
        //Play attack animation

        //Detect enemies

        //Apply damage
    }
}
