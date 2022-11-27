using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : SimCommObject
{
    [SerializeField]
    GameObject[] flareStartPoints;
    [SerializeField]
    float flareForce = 100f;

    public void UseFlare()
    {
        foreach (var flareP in flareStartPoints)
        {
            HandleVFX flareVFX = GameObject.Instantiate(Assets.Instance.FlareHandleVFX);
            flareVFX.Size = ObjSize;

            flareVFX.transform.position = flareP.transform.position;
            flareVFX.transform.rotation = flareP.transform.rotation;

            flareVFX.SetInitialPosition(simObjData.transform);

            //flareVFX.AnimForce = Vector3.back;

            flareVFX.PlayVFX();
            flareVFX.ObjPhysics.AddForce(flareVFX.transform.forward * ObjSize * flareForce, ForceMode.Impulse);
        }
    }
    public void UseChaff()
    {
        HandleVFX chaffVFX = GameObject.Instantiate(Assets.Instance.ChaffHandleVFX);
        chaffVFX.Size = ObjSize;

        chaffVFX.transform.position = transform.position;
        chaffVFX.transform.rotation = transform.rotation;

        chaffVFX.SetInitialPosition(simObjData.transform);

        chaffVFX.PlayVFX();
    }


    protected override void EventTriggered(EventType type, bool evOccurs)
    {
        base.EventTriggered(type, evOccurs);

        switch (type)
        {
            case EventType.chaff:
                if (evOccurs)
                    UseChaff();
                //else
                //TODO destroy chaff
                break;

            case EventType.flare:
                if (evOccurs)
                    UseFlare();
                //else
                //TODO destroy flare
                break;
        }

    }
}
