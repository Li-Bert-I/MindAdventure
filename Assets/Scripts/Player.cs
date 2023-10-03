using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float inertiaTime;
    public Vector3 velocity;
    public Vector3 targetVelocity;
    private Vector3 acceleration;
    private Collider[] collisionColliders;
    // private ContactPoint[] emptyContacts;
    private Collider collider;

    void Start()
    {
        // emptyContacts = new ContactPoint[0];
        collisionColliders = new Collider[10];
        collider = gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        targetVelocity = Vector3.ClampMagnitude(
            new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")), 
            1.0f) * speed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref acceleration, inertiaTime, Mathf.Infinity, Time.deltaTime);

        int numColliders = Physics.OverlapSphereNonAlloc(
            transform.position, collider.bounds.size.x * 0.5f, collisionColliders, 1 << 8);

        for (int i = 0; i < numColliders; ++i)
        {
            Collider otherCollider = collisionColliders[i];
            Vector3 direction;
            float distance;
            bool overlapped = Physics.ComputePenetration(
                collider, transform.position, transform.rotation,
                otherCollider, otherCollider.gameObject.transform.position, otherCollider.gameObject.transform.rotation,
                out direction, out distance
            );
            if (overlapped)
            {
                transform.position += direction * distance;
            }
        }

        transform.position += velocity * Time.deltaTime;
    }
}
