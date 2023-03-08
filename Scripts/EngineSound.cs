using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSound : MonoBehaviour
{
    public AudioSource motorSound;
    public float maxVolume;
    public float maxPitch;
    private float rpmClamped;
    public float limiterSound = 1f;
    public float limiterFreq = 3f;
    public float limiterEngage = 0.8f;
    public float revLimiter;
    [SerializeField] float pitchRatio;
    public CarControllerNew carController;
    void Start()
    {
        carController = GetComponentInParent<CarControllerNew>();
    }


    private void Update()
    {
        rpmClamped = Mathf.Lerp(rpmClamped, carController.RPM, Time.deltaTime);
        pitchRatio = PitchRatio();
        if (pitchRatio > limiterEngage)
        {
            revLimiter = (Mathf.Sin(Time.time * limiterFreq)+1f)*limiterSound*(pitchRatio - limiterEngage);
        }
        if (carController.gearState == GearState.Reverse)
        {
            motorSound.volume = 0.45f;
            motorSound.pitch = 0.9f;
        }
        else
        {
            motorSound.volume = Mathf.Lerp(0.35f, maxVolume, pitchRatio);
            motorSound.pitch = Mathf.Lerp(0.7f, maxPitch + revLimiter, pitchRatio);
        }
        
    }

    public float PitchRatio()
    {
        return rpmClamped / (carController.redLine + 200f);
    }
}
