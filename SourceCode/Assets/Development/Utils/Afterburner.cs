using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Afterburner : MonoBehaviour
{
    public float startValMultiplier = 1f;
    [SerializeField,Range(0,2f)]
    float valShifting=0f;
    [SerializeField]
    AnimationCurve fireSizeSpeedCurve= AnimationCurve.Linear(0, 0, 1, 1);



    public ParticleSystem fire;
    public ParticleSystem smoke;

    float fireSpeed,fireSize;
    float smokeSpeed,smokeSize;

    ParticleSystem.MainModule fireMainModule;
    ParticleSystem.MainModule smokeMainModule;


    public void SetActiveBurner(bool isActive)
    {
        if (isActive)
        {
            if(!fire.isPlaying)
                fire.Play();
            if (!smoke.isPlaying)
                smoke.Play();
        }
        else
        {
            if (fire.isPlaying)
                fire.Stop();
            if (smoke.isPlaying)
                smoke.Stop();
        }
    }
    public void SetAfterburnerVal(float val)
    {
        if (!init) Init();
        val = Mathf.Clamp01(val);

        fire.startSize =  fireSize* val * startValMultiplier;
        fire.startSpeed =  fireSpeed* fireSizeSpeedCurve.Evaluate(val) * startValMultiplier;

        smoke.startSize =  smokeSize* val * startValMultiplier;
        smoke.startSpeed = smokeSpeed * fireSizeSpeedCurve.Evaluate(val) * startValMultiplier;
    }

    private void Awake()
    {
        Init();
    }


    bool init = false;
    void Init()
    {
        if (!init)
        {
            SetActiveBurner(false);
            fireMainModule = fire.main;
            smokeMainModule = smoke.main;

            fireSpeed = fire.startSpeed;
            fireSize = fire.startSize;

            smokeSpeed = smoke.startSpeed;
            smokeSize = smoke.startSize;
            init = true;
        }
    }

}
