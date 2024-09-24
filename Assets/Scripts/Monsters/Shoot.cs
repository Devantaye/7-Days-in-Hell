using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    public void Attack()
    {
        Vector2 targetDirection = PlayerController.Instance.transform.position - transform.position;

        GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        newProjectile.transform.right = targetDirection;



    }

}
