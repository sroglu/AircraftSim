using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBuildings : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 basePos = Utility.CreatePositionByCoords(40, 32, Constants.Zero);
        transform.position = basePos - GameManager.SimPivot;
    }
}
