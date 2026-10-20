
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 10f;
    public float pitch = 30f;
    public float yaw = 0f;

    [Header("Framing")]
    public float targetHeight = 0f;

    [Header("Smooth Follow")]
    public float followSpeed = 5f;

    void Start()
    {
        if (target != null)
        {
            UpdateCamera(true);
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            UpdateCamera(false);
        }
    }

    void UpdateCamera(bool instant)
    {
        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focusPoint =
            target.position + Vector3.up * targetHeight;

        Vector3 desiredPosition =
            focusPoint - rotation * Vector3.forward * distance;

        if (instant)
        {
            transform.position = desiredPosition;
        }
        else
        {
            float t = 1f - Mathf.Exp(
                -followSpeed * Time.deltaTime
            );

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                t
            );
        }

        transform.rotation = rotation;
    }
}
