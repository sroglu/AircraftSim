using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FreeFallSimObject : SimRelativeObject
{
    public const float GRAVITY = 9.81f;

    protected Rigidbody rigidbody;
    [SerializeField]
    SimObjTransform initialPos=SimObjTransform.SeaLevelZero;
    Vector3 startSimPivot,objStartPos;

    //public override SimObjTransform GetObjTransform()
    //{
    //    return initialPos;
    //}

    protected override void Init()
    {
        base.Init();
        rigidbody = GetComponent<Rigidbody>();
        //ObjTransform.localScale = Vector3.one;



        //rotation = transform.rotation;
        //transform.rotation=Quaternion.identity;
        //ObjTransform.rotation=rotation;
    }

    protected override bool CheckInitiated()
    {
        return base.CheckInitiated() && initialPos!=null;
    }

    public void SetInitialPosition(SimObjTransform initialPos)
    {
        this.initialPos = initialPos;
        startSimPivot = GameManager.SimPivot;
        objStartPos=transform.position;
    }

    //Vector3 physicsEffect=Vector3.zero;
    protected override void FixedUpdated()
    {
        base.FixedUpdated();
        if (Initiated)
        {
            //physicsEffect += rigidbody.velocity;

            ////Ignore Rigidbody changes
            //transform.localPosition = Vector3.zero;

            //rigidbody.AddForce( -SurfaceNormal * rigidbody.mass*rigidbody.drag, ForceMode.Acceleration);

            ApplyGravity();

            //DrawArrow.ForDebug(transform.position, transform.position + SurfaceNormal * -100);

        }
    }

    [SerializeField]
    protected Vector3 physicalMovement = Vector3.zero;
    //float G = 6.674f * Mathf.Pow(10, -11);
    //float earthMass = 5.9722f * Mathf.Pow(10, 24);
    void ApplyGravity()
    {

        //float veloc = rigidbody.velocity.magnitude * rigidbody.drag;
        //float coeff = (1 - Time.fixedDeltaTime * rigidbody.drag);
        //float force = (veloc / coeff) * rigidbody.mass;
        //rigidbody.AddForce(-SurfaceNormal * force, ForceMode.Force);



        //float veloc = rigidbody.velocity.magnitude * rigidbody.drag;
        //float coeff = (1 - Time.fixedDeltaTime * rigidbody.drag);
        //Vector3 gravitationalForce = ((veloc + GRAVITY) / coeff) * rigidbody.mass * -SurfaceNormal;

        //rigidbody.AddForce(gravitationalForce, ForceMode.Force);


        //Vector3 gravitationalForce = G * (rigidbody.mass * earthMass) / Mathf.Pow((float)initialPos.alt, 2) * -SurfaceNormal;
        ////rigidbody.AddForce(gravitationalForce, ForceMode.Force);
        //rigidbody.AddForce(gravitationalForce, ForceMode.Impulse);

        //float velocity = Vector3.Project(rigidbody.velocity, SurfaceNormal).magnitude * rigidbody.drag;
        float velocity = Vector3.Project(rigidbody.velocity, SurfaceNormal).magnitude * (Time.fixedDeltaTime * rigidbody.drag);

        //Debug.DrawLine(transform.position,transform.position+ SurfaceNormal*100);

        float force = rigidbody.mass * GRAVITY;
        rigidbody.AddForce(Mathf.Clamp(velocity,0, force) * SurfaceNormal, ForceMode.Impulse);
        rigidbody.AddForce(force * -SurfaceNormal, ForceMode.Impulse);

        //Debug.Log(velocity);
        //Debug.Log(velocity + "  " + force);

        physicalMovement += rigidbody.velocity*Time.fixedDeltaTime;
    }




    //protected override void AfterTransformSet()
    //{
    //    base.AfterTransformSet();

    //    //ObjTransform.rotation = rotation;


    //    //SetToInitialRotation();
    //}

    //protected override void UpdateRotation()
    //{
    //    //base.UpdateRotation();

    //}

    public override Quaternion GetBaseRotation()
    {
        return Utility.CreateBaseRotationByCoords(initialPos);
    }

    public override Quaternion GetRotation()
    {
        return Quaternion.identity;
    }

    public override Vector3 GetEarthPos()
    {
        //return transform.position- (GetEarthPos(initialPos)-startSimPivot)+ GetEarthPos(initialPos);
        //return /*transform.position - objStartPos +*/ CreatePositionByCoords(initialPos);

        //Debug.Log((transform.position - objStartPos)+"   "+ simBaseRotationHandle.InverseTransformVector(transform.position - objStartPos));
        //return transform.position - objStartPos + CreateEarthPositionByCoords(initialPos);
        return physicalMovement + CreateEarthPositionByCoords(initialPos);
    }

    //protected override void UpdateRotation()
    //{
    //    base.UpdateRotation();
    //}

    //protected override void UpdatePosition()
    //{
    //    base.UpdatePosition();
    //}
}
