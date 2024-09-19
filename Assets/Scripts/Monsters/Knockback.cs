using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public bool gettingPushedBack { get; private set; }

    [SerializeField] private float knockBackTime = .2f;

    [SerializeField] private float maxKnockbackDistance = 0.5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        gettingPushedBack = true;
        Vector2 difference = (transform.position - damageSource.position).normalized * knockBackThrust * rb.mass;
        if (difference.magnitude > maxKnockbackDistance)
        {
            difference = difference.normalized * maxKnockbackDistance;
        }

        rb.AddForce(difference, ForceMode2D.Impulse);
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        gettingPushedBack = false;
    }
}
