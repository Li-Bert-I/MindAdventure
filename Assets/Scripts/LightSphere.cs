using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSphere : MonoBehaviour
{
    public Player player;
    public Light lightSource;
    public AudioSource attach;
    public AudioSource release;

    public bool connectedToPlayer = true;
    public bool inPosition = false;
    public Vector3 floorPosition;
    public float speed;
    public float inertiaTimeRotation;
    public float radiusIdle;
    public float rotationSpeed;
    public float rotationSpeedRotation;
    private float inertiaTime;
    private Vector3 normal;
    private Vector3 normalRotation;
    private Vector3 relativePosition;
    private float radiusOnPlayer;
    private Vector3 velocity;
    private Vector3 localPosition;

    void Start()
    {
        radiusOnPlayer = player.GetComponent<Collider>().bounds.size.x / 2;
        normal = Random.insideUnitSphere.normalized;
        normalRotation = Vector3.Cross(Random.insideUnitSphere, normal).normalized;
        localPosition = Vector3.Cross(normalRotation, normal).normalized;
    }

    void Update()
    {
        normal = Quaternion.AngleAxis(rotationSpeedRotation * Time.deltaTime, normalRotation) * normal;
        localPosition = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, normal) * localPosition;

        if (connectedToPlayer) {
            MoveAround(player.transform.position, radiusOnPlayer);

            lightSource.transform.position = player.transform.position;
            inPosition = false;
            inertiaTime = -1;
        } else if (inPosition) {
            MoveAround(floorPosition, radiusIdle);
        }
            else {
            if (inertiaTime == -1) {
                inertiaTime = (transform.position - floorPosition).magnitude / speed;
            }
            transform.position = Vector3.SmoothDamp(
                transform.position, floorPosition, 
                ref velocity, inertiaTime, Mathf.Infinity, Time.deltaTime);
        }
        if ((transform.position - floorPosition).magnitude < 0.5f) {
            inPosition = true;
        }
    }

    private void MoveAround(Vector3 targetPosition, float radius) 
    {
        targetPosition += localPosition * radius;

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, 
            ref velocity, inertiaTimeRotation, Mathf.Infinity, Time.deltaTime);
    }
}
