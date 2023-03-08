using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing,
    Reverse
};

public class CarControllerNew : MonoBehaviour
{
    private float horizontalInput;
    public float verticalInput;
    private float handBrakeInput;
    private float steerAngle;
    private float speed;
    private float slipAngle;
    public float currentTorque;
    private float clutch;
    private float wheelRPM;
    private Rigidbody rb;
    public GearState gearState;

    public WheelCollider frontDriverW, frontPassengerW;
    public WheelCollider rearDriverW, rearPassengerW;
    public Transform frontDriverT, frontPassengerT;
    public Transform rearDriverT, rearPassengerT;
    public float brakeInput;
    public float maxSteerAngle = 30.0f;
    public float motorForce = 700f;
    public float handBrakeForce = 50000f;
    public float brakeForce = 50000f;
    public float RPM;
    public float redLine = 6800f;
    public float idleRPM = 800f;
    public float differentialRatio;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    public int currentGear;
    public float changeGearTime = 2f;
    public float motorBrakeForce = 4000f;
    public float reverseTorque = 500f;


    public float[] gearRatios;


    public AnimationCurve steeringCurve;
    public AnimationCurve hpToTorqueCurve;

    [SerializeField] private Vector3 _centerOfMass;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = _centerOfMass;
        gearState = GearState.Running;
    }

    public void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        handBrakeInput = Input.GetAxis("Jump");
        slipAngle = Vector3.Angle(transform.forward, rb.velocity - transform.forward);
        float movingDirection = Vector3.Dot(transform.forward, rb.velocity);

        if (movingDirection < -0.5f && verticalInput > 0)
        {
            brakeInput = Mathf.Abs(verticalInput);
        }
        else if (movingDirection > 0.5f && verticalInput < 0)
        {
            brakeInput = Mathf.Abs(verticalInput);
        }
        else
        {
            brakeInput = 0;
        }
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if(verticalInput > 0)
                {
                    gearState = GearState.Running;
                }
            }
            else
            {
                clutch = Input.GetButton("Fire3") ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0;
        }
        if (gearState == GearState.Reverse && verticalInput > 0)
        {
            gearState = GearState.Neutral;
        }

        
    }

    public void Steer()
    {
        steerAngle = horizontalInput * steeringCurve.Evaluate(speed);
        if (slipAngle < 120f)
        {
            steerAngle += Vector3.SignedAngle(transform.forward, rb.velocity + transform.forward, Vector3.up);
        }
        steerAngle = Mathf.Clamp(steerAngle, -90f, 90f);
        frontDriverW.steerAngle = Mathf.Lerp(frontDriverW.steerAngle, steerAngle, 0.4f); 
        frontPassengerW.steerAngle = Mathf.Lerp(frontPassengerW.steerAngle, steerAngle, 0.4f);
    }

    public void Accelerate()
    {
        

        if (gearState != GearState.Reverse)
        {
            currentTorque = CalculateTorque();
            frontDriverW.motorTorque = verticalInput * currentTorque * 0.65f;
            frontPassengerW.motorTorque = verticalInput * currentTorque * 0.65f;
            rearDriverW.motorTorque = verticalInput * currentTorque;
            rearPassengerW.motorTorque = verticalInput * currentTorque;
        }
        

        if (gearState == GearState.Neutral && verticalInput < 0)
        {
            gearState = GearState.Reverse;
            frontDriverW.motorTorque = verticalInput * reverseTorque;
            frontPassengerW.motorTorque = verticalInput * reverseTorque;
            rearDriverW.motorTorque = verticalInput * reverseTorque;
            rearPassengerW.motorTorque = verticalInput * reverseTorque;
        }
    }

    public float CalculateTorque()
    {
        float torque = 0;
        if (RPM < idleRPM + 200 && verticalInput == 0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }
        if (gearState == GearState.Running && clutch > 0)
        {
            if(RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if(RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if (clutch < 0.1f)
        {
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * verticalInput)+Random.Range(-50, 50), Time.deltaTime);
        }
        else
        {
            wheelRPM = Mathf.Abs((rearDriverW.rpm + rearPassengerW.rpm) / 2f) * gearRatios[currentGear] * differentialRatio;
            RPM = Mathf.Lerp(Mathf.Max(idleRPM - 100, wheelRPM), RPM, Time.deltaTime * 3f);
            torque = (hpToTorqueCurve.Evaluate(RPM / redLine) * motorForce / RPM)*gearRatios[currentGear] * differentialRatio * 5252f * clutch;
        }
        return torque;
    }

    public void Brake()
    {
        frontDriverW.brakeTorque = brakeInput * brakeForce * 0.75f;
        frontPassengerW.brakeTorque = brakeInput * brakeForce * 0.75f;
        rearDriverW.brakeTorque = brakeInput * brakeForce * 0.3f;
        rearPassengerW.brakeTorque = brakeInput * brakeForce * 0.3f;

        if ((verticalInput == 0 || gearState == GearState.Changing) && clutch != 0) 
        {
            frontDriverW.brakeTorque = motorBrakeForce * 0.14f;
            frontPassengerW.brakeTorque = motorBrakeForce * 0.14f;
            rearDriverW.brakeTorque = motorBrakeForce * 0.5f;
            rearPassengerW.brakeTorque = motorBrakeForce * 0.5f;
        }
    }

    public void Handbrake()
    {
        rearDriverW.brakeTorque = handBrakeInput * handBrakeForce;
        rearPassengerW.brakeTorque = handBrakeInput * handBrakeForce;
        rearDriverW.motorTorque = 0;
        rearPassengerW.motorTorque = 0;
    }
    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontDriverW, frontDriverT);
        UpdateWheelPose(frontPassengerW, frontPassengerT);
        UpdateWheelPose(rearDriverW, rearDriverT);
        UpdateWheelPose(rearPassengerW, rearPassengerT);
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);
        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                //increase gear
                yield return new WaitForSeconds(0.2f);
                if(RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                yield return new WaitForSeconds(0.1f);
                // decrease gear
                if (RPM > decreaseGearRPM || currentGear <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            currentTorque = 0;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
            currentTorque = CalculateTorque();
        }
        if(gearState!=GearState.Neutral)
        gearState = GearState.Running;
        
    }

    private void Update()
    {
        GetInput();
        Accelerate();
        speed = rb.velocity.magnitude;
    }

    private void FixedUpdate()
    {
        Steer();
        Brake();
        Handbrake();
        UpdateWheelPoses();
    }


}
