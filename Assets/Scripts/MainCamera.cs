using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Player player;
    public float baseHeight;
    public float maxHeight;
    public float vecticalInertiaTime;
    public float horizontalInertiaTime;
    private float height;
    private float verticalVelocity;
    private Vector2 horizontalVelocity;
    private bool wasInit = false;

    void Update()
    {
        Vector3 newPosition = player.transform.position;

        if (!wasInit) {
            newPosition.y = baseHeight;
            transform.position = newPosition;
            wasInit = true;
        }
        
        height = baseHeight + Mathf.Max(
            Mathf.SmoothDamp(
                height - baseHeight, 
                (maxHeight - baseHeight) * player.targetVelocity.magnitude / player.speed, 
                ref verticalVelocity, vecticalInertiaTime, Mathf.Infinity, Time.deltaTime),
            0.0f);
        newPosition.y = height;

        Vector2 horizontalPosition = Vector2.SmoothDamp(
            new Vector2(transform.position.x, transform.position.z), new Vector2(newPosition.x, newPosition.z), 
            ref horizontalVelocity, horizontalInertiaTime, Mathf.Infinity, Time.deltaTime);
        newPosition.x = horizontalPosition.x;
        newPosition.z = horizontalPosition.y;

        transform.position = newPosition;
    }
}
