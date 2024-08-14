using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private Vector3 offsetPosition;

    [SerializeField]
    private Space offsetPositionSpace = Space.Self;

    [SerializeField]
    private bool lookAt = true;

    [SerializeField]
    private Vector3 rotationOffset; // Rotation offset in x, y, z

    [SerializeField]
    private bool freezeXRotation = false; // Freeze rotation offset for X axis
    [SerializeField]
    private bool freezeYRotation = false; // Freeze rotation offset for Y axis
    [SerializeField]
    private bool freezeZRotation = false; // Freeze rotation offset for Z axis

    [SerializeField]
    private float positionSmoothSpeed = 0.125f; // Smoothing speed for position

    [SerializeField]
    private float rotationSmoothSpeed = 0.125f; // Smoothing speed for rotation

    private Transform target;

    private void Update()
    {
        if (target != null)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (target == null)
        {
            Debug.LogWarning("Missing target ref!", this);
            return;
        }

        // Compute target position
        Vector3 desiredPosition;
        if (offsetPositionSpace == Space.Self)
        {
            desiredPosition = target.TransformPoint(offsetPosition);
        }
        else
        {
            desiredPosition = target.position + offsetPosition;
        }

        // Smoothly move to the target position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);

        // Apply rotation offset with freeze settings
        Vector3 appliedRotationOffset = new Vector3(
            freezeXRotation ? 0 : rotationOffset.x,
            freezeYRotation ? 0 : rotationOffset.y,
            freezeZRotation ? 0 : rotationOffset.z
        );

        // Compute target rotation
        Quaternion targetRotation;
        if (lookAt)
        {
            targetRotation = Quaternion.LookRotation(target.position - transform.position);
        }
        else
        {
            targetRotation = target.rotation;
        }

        // Apply the rotation offset
        targetRotation *= Quaternion.Euler(appliedRotationOffset);

        // Smoothly rotate to the target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        Debug.Log("CameraControl SetTarget called");
        target = newTarget;
    }
}
