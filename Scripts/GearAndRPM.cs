using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GearAndRPM : MonoBehaviour
{
    [SerializeField] private GameObject car;
    [SerializeField] private CarControllerNew carController;
    [SerializeField] private Transform rpmNeedle;
    [SerializeField] private float minNeedleRotation;
    [SerializeField] private float maxNeedleRotation;
    

    private void FixedUpdate()
    {
        rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, carController.RPM / carController.redLine));
    }

    
}
