using System;
using System.Collections;
using UnityEngine;

interface ISimRelativeObject
{
    Vector3 GetEarthPos();
    Vector3 PlayerRelativePos();
    Quaternion GetBaseRotation();
    Quaternion GetRotation();
    Vector3 GetPosition();
}

public abstract class SimRelativeObject : MonoBehaviour, ISimRelativeObject
{

    public float ObjSize
    {
        get
        {
            return simBaseRotationHandle.lossyScale.magnitude *
                Mathf.Max(simBaseRotationHandle.lossyScale.normalized.x, simBaseRotationHandle.lossyScale.normalized.y, simBaseRotationHandle.lossyScale.normalized.z);
        }
    }

    public Quaternion BaseRotation { get { return GetBaseRotation(); } }
    public Vector3 SurfaceNormal { get { return (BaseRotation * Vector3.up).normalized; } }
    public Quaternion Rotation {
        get { return GetRotation(); }
        set { simBaseRotationHandle.rotation = value; simObjectRotationHandle.localRotation = Quaternion.identity; }
    }
    public Vector3 Position { get { return PlayerRelativePos(); } set { transform.position = value; } }

    public Vector3 GetPosition()
    {
        return PlayerRelativePos();
    }
    public abstract Quaternion GetBaseRotation();
    public abstract Quaternion GetRotation();


    public bool IsExploded { get; private set; }
    public bool Initiated { get { return CheckInitiated(); } }
    [SerializeField]
    bool initiated = false;


    //[SerializeField]
    //Transform simModelTransform;
    [SerializeField]
    float simModelScale = 1;
    public Vector3 simModelOffset = Vector3.zero;

    [SerializeField]
    Vector3 calculatedPos,additionalPos,finalPos;


    protected Transform transformCorrectionRotationHandle,simBaseRotationHandle, simObjectRotationHandle;
    ParticleSystem explosionFx, afterExploasion;

    Transform[] childrens;

    //Transform upCube;


    //Quaternion initialRotation;
    void Awake()
    {
        //initialRotation=transform.rotation;

        childrens=new Transform[transform.childCount];
        for (int i = 0; i < childrens.Length; i++)
        {
            childrens[i]=transform.GetChild(i);
        }

        simBaseRotationHandle = new GameObject(transform.name + "_TransformSimBaseRotationHandle").transform;
        simBaseRotationHandle.parent = transform;
        simBaseRotationHandle.localPosition = Vector3.zero;
        simBaseRotationHandle.localRotation= Quaternion.identity;
        simBaseRotationHandle.localScale= Vector3.one;


        simObjectRotationHandle = new GameObject(transform.name + "_TransformSimRotationHandle").transform;
        simObjectRotationHandle.parent = simBaseRotationHandle;
        simObjectRotationHandle.localPosition = Vector3.zero;
        simObjectRotationHandle.localRotation = Quaternion.identity;
        simObjectRotationHandle.localScale = Vector3.one;


        for (int i = 0; i < childrens.Length; i++)
        {
            childrens[i].transform.parent = simObjectRotationHandle;
            //childrens[i].transform.localRotation = simRotationCorrection;
        }

        //transformCorrectionRotationHandle.localRotation = CorrectionRotation;
        simBaseRotationHandle.transform.localScale = Vector3.one * simModelScale;

        //upCube= GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        //upCube.parent = simBaseRotationHandle;
        //upCube.localPosition = new Vector3(0,0,0);
        //upCube.localRotation=Quaternion.identity;

        //Transform cubeT = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        //cubeT.parent = simBaseRotationHandle;
        //cubeT.localPosition = Vector3.zero;
        //cubeT.transform.localRotation = Quaternion.identity;
        //cubeT.localScale = new Vector3(1f, 1f, 1000f);

        StartCoroutine(OnCreate(() =>
        {
            Init();
        }));
    }

    IEnumerator OnCreate(Action callback)
    {
        //yield return new WaitUntil(()=>GameManager.Instance != null);
        if (GameManager.Instance == null)
            yield return new WaitForEndOfFrame();
        callback?.Invoke();
        yield return null;
    }

    public static Vector3 CreateEarthPositionByCoords(SimObjTransform simObjTransform)
    {
        return Utility.CreatePositionByCoords(simObjTransform.lat, simObjTransform.lon, simObjTransform.alt);
    }

    public static Vector3 PlayerRelativePos(SimObjTransform simObjTransform)
    {
        return CreateEarthPositionByCoords(simObjTransform) - GameManager.SimPivot;
    }

    //public virtual Vector3 GetEarthPos()
    //{
    //    calculatedPos = GetEarthPos(GetObjTransform());
    //    additionalPos = OtherForcesAffectTransform();
    //    finalPos = calculatedPos+additionalPos;

    //    return GetEarthPos(GetObjTransform()) + OtherForcesAffectTransform();
    //}
    public abstract Vector3 GetEarthPos();
    public Vector3 PlayerRelativePos()
    {
        return GetEarthPos()-GameManager.SimPivot + simModelOffset ;
    }
    protected void Update()
    {
        Updated();
    }
    void FixedUpdate()
    {
        if (Initiated)
        {

            BeforeTransformSet();
            UpdateTransform();
            AfterTransformSet();
        }

        Debug.DrawLine(transform.position,transform.position+SurfaceNormal*25);


        //if (upCube)
        //{
        //    upCube.transform.position = transform.position + SurfaceNormal * 100f;

        //}

        FixedUpdated();
    }


    //public void SetToInitialRotation()
    //{
    //    simBaseRotationHandle.rotation = initialRotation;
    //}


    public void UpdateTransform()
    {
        UpdateRotation();
        UpdatePosition();
    }

    protected virtual void UpdateRotation()
    {
        // Rotate the aircraft with respect to position (default, facing north at N0 E0)
        //transformRotationHandle.rotation = Quaternion.Euler(0, (float)-GetObjTransform().lon, (float)GetObjTransform().lat);

        simBaseRotationHandle.rotation = BaseRotation;

        // Rotate the aircraft with respect to rotation

        simObjectRotationHandle.localRotation = Rotation;
        //simObjectRotationHandle.localRotation = Quaternion.Euler((float)GetObjTransform().yaw, 0, (float)-GetObjTransform().pitch) * Quaternion.Euler(0, (float)-GetObjTransform().roll, 0);
        //transformSimRotationHandle2.Rotate(new Vector3(0, (float)-GetObjTransform().roll, 0));
    }
    protected virtual void UpdatePosition()
    {
        //transform.localRotation = Quaternion.Euler((float)GetObjTransform().yaw, 0, (float)-GetObjTransform().pitch);
        //transform.Rotate(new Vector3(0, (float)-GetObjTransform().roll, 0));

        transform.position = Position;
    }

    void InitFxs()
    {

        explosionFx = Instantiate(Assets.Instance.ExplosionFx);
        explosionFx.transform.parent = simBaseRotationHandle;
        explosionFx.transform.localPosition = Vector3.zero;
        explosionFx.transform.localScale = explosionFx.transform.lossyScale * ObjSize;
        //explosionFx.transform.localScale = transformHandle.transform.lossyScale*10;

        afterExploasion = Instantiate(Assets.Instance.AfterExplosionFx);
        afterExploasion.transform.parent = simBaseRotationHandle;
        afterExploasion.transform.localPosition = Vector3.zero;
        afterExploasion.transform.localScale = afterExploasion.transform.lossyScale * ObjSize;
        //afterExploasion.transform.localScale = transformHandle.transform.lossyScale * 10;
        afterExploasion.transform.rotation = Quaternion.LookRotation(Vector3.right);

    }


    public void Explode()
    {
        if (!IsExploded)
        {
            OnExplode();
            explosionFx.Play();
            afterExploasion.Play();
            IsExploded = true;
        }
    }



    //public abstract Vector3 OtherForcesAffectTransform();


    protected virtual void Init() { 
        InitFxs();
        initiated = true;
    }

    protected virtual bool CheckInitiated() { return initiated; }

    protected virtual void Updated() { }
    protected virtual void FixedUpdated() { }


    protected virtual void BeforeTransformSet() { }
    protected virtual void AfterTransformSet() { }

    #region ActionMethods
    protected virtual void OnExplode() { }
    #endregion

}
