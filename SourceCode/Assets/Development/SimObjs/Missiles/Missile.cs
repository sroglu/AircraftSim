using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : SimCommObject
{
    [SerializeField]
    Afterburner afterburner;

    protected override void Init()
    {
        base.Init();
        afterburner.startValMultiplier = ObjSize;
        afterburner.SetAfterburnerVal(1);
        afterburner.SetActiveBurner(false);
    }


    SimObjData prevData;
    Vector3 prevLookDir = Vector3.zero;
    [SerializeField]
    bool missileStartedToMove = false;
    protected override void UpdateRotation()
    {
        if (simObjDataBuffer.Count > 0)
        {
            prevData = simObjDataBuffer.Peek();

            if (SimObjCommunication.IsRcvDataValid(ref prevData) && prevData != simObjData)
            {
                Vector3 lookDir = CreateEarthPositionByCoords(simObjData.transform) - CreateEarthPositionByCoords(prevData.transform);
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);

                    prevLookDir = lookDir;
                }
                else
                {
                    if (prevLookDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Euler(prevLookDir);
                    }
                }
            }
        }
        simBaseRotationHandle.localRotation = Quaternion.identity;
        simObjectRotationHandle.localRotation = Quaternion.identity;
    }

    protected override void OnExplode()
    {
        base.OnExplode();
        transform.gameObject.SetActive(false);
    }

    float missileLaunchTimer = 0f;
    const float missileLaunchEns = 5f;
    protected override float GetAttachObjectCohesion()
    {
        if (simObjData.status == SimObjCommunication.InstanceStatus.MOVING)
        {
            return 1 - (missileLaunchTimer / missileLaunchEns);
        }
        else
        {
            return 1;
        }
    }

    [SerializeField]
    float debug_cohesion=1;
    protected override void Updated()
    {
        base.Updated();

        if (simObjData.status == SimObjCommunication.InstanceStatus.MOVING)
        {
            if (missileLaunchTimer < missileLaunchEns)
                missileLaunchTimer += Time.deltaTime;
            else
                missileLaunchTimer = missileLaunchEns;

            debug_cohesion = GetAttachObjectCohesion();
        }
    }

    protected override void StatusChanged(SimObjCommunication.InstanceStatus status)
    {
        base.StatusChanged(status);

        afterburner.SetActiveBurner((status == SimObjCommunication.InstanceStatus.MOVING));

    }
}
