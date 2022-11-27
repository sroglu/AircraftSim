
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect),typeof(Rigidbody))]
public class HandleVFX : FreeFallSimObject
{
    public float Size
    {
        get { return size; }
        set
        {
            if(value < 100f&& value > 1f)
            {
                size = value;
                UpdateVFX();
            }
        }
    }
    [SerializeField]
    bool isWorld = false;
    [SerializeField]
    float gravityAnimForceMultiplyer = 0f;
    [SerializeField, Range(1f, 100f)]
    float size = 1f;
    [SerializeField, Range(1f, 100f)]
    float sizeMultiplayer = 1f;
    [SerializeField,ColorUsage(true,true)]
    Color overrideBaseColor;
    [SerializeField]
    Gradient overrideVisibilityGradient= new Gradient();
    [SerializeField]
    VisualEffect vfx;    
    [SerializeField]
    Rigidbody objPhysics;
    [SerializeField]
    Vector3 animForce=Vector3.zero;
    [SerializeField]
    Light coreLight;
    [SerializeField]
    float lifeTime=5;
    [SerializeField]
    bool updateVFX = false;
    [SerializeField]
    bool playVFX = false;

    #region Accesors
    public Vector3 AnimForce
    {
        get { return animForce; } set {
        animForce = value;
            UpdateVFX();
        }
    }
    public Rigidbody ObjPhysics
    {
        get { return objPhysics; }
    }
    public Color OverrideBaseColor
    {
        get { return overrideBaseColor; }
        set
        {
            overrideBaseColor = value;
            UpdateVFX();
        }
    }
    public Gradient OverrideVisibilityGradient
    {
        get { return overrideVisibilityGradient; }
        set
        {
            overrideVisibilityGradient = value;
            UpdateVFX();
        }
    }


    #endregion



    protected override void Init()
    {
        base.Init();
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        if (objPhysics == null)
            objPhysics = GetComponent<Rigidbody>();

        vfx.startSeed = (uint)Random.Range(0, uint.MaxValue);

        UpdateVFX();
        PlayVFX();


        isPlayed = false;
        countDown = lifeTime;

        //GameManager.Instance.RegisterRelativeInstances(gameObject);
    }

    //private void OnDestroy()
    //{
    //    //GameManager.Instance.RemoveRelativeInstances(gameObject);
    //}

    // Start is called before the first frame update

    //private void OnEnable()
    //{
    //    Init();
    //}

    protected override void FixedUpdated()
    {
        base.FixedUpdated();
        if (updateVFX)
        {
            UpdateVFX();
            updateVFX = false;
        }
        if (playVFX)
        {
            PlayVFX();
            playVFX = false;
        }
        UpdateAnimForce();
        CheckIsDead();
    }

    public void SetDestroyAction(out System.Action destroyCallback)
    {
        destroyCallback = DestroyVFX;
    }

    void DestroyVFX()
    {
        Destroy(gameObject);
    }

    float countDown = 0;
    void CheckIsDead()
    {
        if (countDown == 0)
        {
            Dead();
        }
        else
        {
            countDown--;
        }

    }

    void Dead()
    {
        Destroy(gameObject);
    }

    bool isPlayed=false;
    public void PlayVFX()
    {
        vfx.Play();
        isPlayed = true;
    }

    void UpdateVFX()
    {
        if (coreLight != null)
        {
            coreLight.intensity = size;
            coreLight.range = size;
            if (overrideBaseColor != Color.black && overrideBaseColor.a == 0f)
            {
                coreLight.color = overrideBaseColor;
            }
        }

        if (vfx != null)
        {
            if (vfx.HasFloat("Size"))
                vfx.SetFloat("Size", size* sizeMultiplayer);

            if (vfx.HasGradient("VisibilityGradient") &&
                overrideVisibilityGradient.colorKeys[0].color==Color.white&&
                overrideVisibilityGradient.colorKeys[1].color == Color.white &&
                overrideVisibilityGradient.alphaKeys[0].alpha == 0 &&
                overrideVisibilityGradient.alphaKeys[1].alpha == 0)
            {
                vfx.SetGradient("VisibilityGradient", overrideVisibilityGradient);
            }
            if (vfx.HasVector4("BaseColor")&&
                overrideBaseColor != Color.black && 
                overrideBaseColor.a == 0f)
            {                
                vfx.SetVector4("BaseColor", overrideBaseColor);
            }

            //if (vfx.HasVector3("Force"))
            //{
            //    //Vector3 gravitationalAnimForce= Physics.gravity.normalized * gravityAnimForceMultiplyer ;
            //    //if(!isWorld)
            //    //    gravitationalAnimForce = transform.InverseTransformVector(gravitationalAnimForce);


            //    vfx.SetVector3("Force", transform.InverseTransformVector(SurfaceNormal) * gravityAnimForceMultiplyer );

            //}
        }
    }

    void UpdateAnimForce()
    {
        if (vfx.HasVector3("Force"))
        {
            vfx.SetVector3("Force", transform.InverseTransformVector(physicalMovement.normalized) * gravityAnimForceMultiplyer);
        }
    }

}
