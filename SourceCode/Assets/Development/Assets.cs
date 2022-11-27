using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assets : MonoBehaviour
{
    static Assets instance;
    public static Assets Instance
    {
        get { return instance; }
        private set
        {
            if (instance == null)
                instance = value;
            else
                Destroy(value);
        }
    }


    [SerializeField]
    ParticleSystem explosionFx;
    [SerializeField]
    ParticleSystem afterExplosionFx;
    [SerializeField]
    HandleVFX flareHandleVFX;
    [SerializeField]
    HandleVFX chafHandleVFX;


    #region Getters
    public ParticleSystem ExplosionFx { get { return explosionFx; } }
    public ParticleSystem AfterExplosionFx { get { return afterExplosionFx; } }
    public HandleVFX FlareHandleVFX { get { return flareHandleVFX; } }
    public HandleVFX ChaffHandleVFX { get { return chafHandleVFX; } }
    #endregion

    
    private void Awake()
    {
        Instance = this;
    }


}
