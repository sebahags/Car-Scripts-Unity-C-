using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpeedMeter : MonoBehaviour
{
    [SerializeField] private GameObject car;
    [SerializeField] private float maxSpeed;
    [SerializeField] private Transform speedNeedle;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float minNeedleRotation;
    [SerializeField] private float maxNeedleRotation;
    [SerializeField] private TMP_Text GearText;
    [SerializeField] private CarControllerNew carController;
    private Rigidbody rb;
    

    private void Awake()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f;
        speedNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, currentSpeed / maxSpeed));
        GearText.text = generateGearText();
    }
    private string generateGearText()
    {
        if (carController.gearState == GearState.Neutral)
        {
            return "N";
        }
        else if (carController.gearState == GearState.Reverse)
        {
            return "R";
        }
        else
        {
            return (carController.currentGear + 1).ToString();
        }
    }

}
