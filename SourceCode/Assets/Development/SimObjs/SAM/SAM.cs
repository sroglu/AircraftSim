using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAM : SimCommObject
{
    const float maxCoverage = 50f;
    [SerializeField]
    GameObject coverageObj;
    [SerializeField]
    bool coverageIsActive = false;
    [SerializeField,Range(0, maxCoverage)]
    float coverageSize = 15f;

    Material material;

    protected override void Init()
    {
        base.Init();
        //Copy and reset material since it affets other renderers that shares the same material
        material= coverageObj.GetComponent<Renderer>().material;
        material= new Material(material);
        coverageObj.GetComponent<Renderer>().material=material;

        coverageObj.transform.localScale = Vector3.one * coverageSize;

        SetActiveCoverage(coverageIsActive);
    }

    public void SetActiveCoverage(bool isActive)
    {
        coverageIsActive=isActive;
        coverageObj.SetActive(isActive);
    }

    protected override void OnExplode()
    {
        base.OnExplode();

        coverageObj.SetActive(false);
    }
}
