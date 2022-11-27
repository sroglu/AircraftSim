using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Varjo.XR;
using UnityEngine.SpatialTracking;


public class VRManager : MonoBehaviour
{
    public TrackedPoseDriver HeadRef;
    public Transform eyePoint;

    // Start is called before the first frame update
    void Start()
    {
        VarjoMixedReality.StartRender();
        VarjoRendering.SetOpaque(false);
        ResetHeadPos();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetHeadPos()
    {
        HeadRef.transform.parent = null;
        Transform oldParent = transform.parent;

        transform.parent = HeadRef.transform;
        //Vector3 posDif = transform.localPosition;
        Quaternion rotDif = transform.localRotation;

        transform.parent = oldParent;
        HeadRef.transform.parent = transform;

        transform.position = eyePoint.transform.position + transform.position - HeadRef.transform.position;
        transform.rotation = eyePoint.transform.rotation * rotDif;

    }
}
