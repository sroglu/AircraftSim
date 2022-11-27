using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPlacedInfinity : MonoBehaviour
{
    static uint humanMaxViewDist = 20000;
    [SerializeField]
    Vector3 dirVector;

    public GameObject canvasViewPivot;
    private void Update()
    {
        if (canvasViewPivot != null)
        {
            dirVector = (canvasViewPivot.transform.position - Camera.main.transform.position).normalized;

            transform.position = canvasViewPivot.transform.position + dirVector * humanMaxViewDist;
            transform.rotation.SetLookRotation(canvasViewPivot.transform.position, canvasViewPivot.transform.up);

        }

    }
}
