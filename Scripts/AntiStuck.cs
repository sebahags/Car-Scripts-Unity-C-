using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class AntiStuck : MonoBehaviour
{
    public Transform wheelModel;
    public int raysNumber = 36;
    public float raysMaxAngle = 180f;
    public float wheelWidth = 0.20f;

    private WheelCollider _wheelCollider;
    private float orgRadius;
    private CarControllerNew carController;


    void Awake()
    {
        _wheelCollider = GetComponent<WheelCollider>();
        orgRadius = _wheelCollider.radius;
        carController = GetComponentInParent<CarControllerNew>();
    }


    void Update()
    {
        float radiusOffset = 0f;

        for (int i = 0; i <= raysNumber; i++)
        {
            Vector3 rayDirection = Quaternion.AngleAxis(_wheelCollider.steerAngle, transform.up) * Quaternion.AngleAxis(i * (raysMaxAngle / raysNumber) + ((180f - raysMaxAngle) / 2), transform.right) * transform.forward;

            if (Physics.Raycast(wheelModel.position, rayDirection, out RaycastHit hit, _wheelCollider.radius * 1.32f))
            {
                if (!hit.transform.IsChildOf(carController.transform) && !hit.collider.isTrigger)
                {
                    radiusOffset = Mathf.Max(radiusOffset, (_wheelCollider.radius * 1.32f) - hit.distance);
                }

            }

            Debug.DrawRay(wheelModel.position, rayDirection * orgRadius, Color.green);
            if (Physics.Raycast(wheelModel.position + wheelModel.right * wheelWidth * 0.5f, rayDirection, out RaycastHit rightHit, _wheelCollider.radius * 1.32f))
            {
                if (!rightHit.transform.IsChildOf(carController.transform) && !rightHit.collider.isTrigger)
                {
                    radiusOffset = Mathf.Max(radiusOffset, (_wheelCollider.radius *1.32f) - rightHit.distance);
                }

            }

            Debug.DrawRay(wheelModel.position + wheelModel.right * wheelWidth * 0.5f, rayDirection * orgRadius, Color.green);
            if (Physics.Raycast(wheelModel.position - wheelModel.right * wheelWidth * 0.5f, rayDirection, out RaycastHit leftHit, _wheelCollider.radius * 1.32f))
            {
                if (!leftHit.transform.IsChildOf(carController.transform) && !leftHit.collider.isTrigger)
                {
                    radiusOffset = Mathf.Max(radiusOffset, (_wheelCollider.radius * 1.32f) - leftHit.distance);
                }

            }

            Debug.DrawRay(wheelModel.position - wheelModel.right * wheelWidth * 0.5f, rayDirection * orgRadius, Color.green);
        }

        _wheelCollider.radius = Mathf.LerpUnclamped(_wheelCollider.radius, orgRadius + radiusOffset, Time.deltaTime * 10f);
    }
}
