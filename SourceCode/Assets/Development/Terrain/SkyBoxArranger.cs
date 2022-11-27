using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxArranger : MonoBehaviour
{
    //public Aircraft baseAircraft;
    Vector3 rotationVector;

    // Start is called before the first frame update
    void Start()
    {
        //transform.parent = baseAircraft.transform.parent;
        rotationVector = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Quaternion.Euler(-baseAircraft.pitch, baseAircraft.yaw, -baseAircraft.roll);
        
        //transform.rotation = Quaternion.Euler(baseAircraft.yaw, 0, -baseAircraft.pitch) * baseAircraft.transform.parent.rotation;
        //transform.Rotate(new Vector3(0,-baseAircraft.roll,0));

       /* rotationVector.x = baseAircraft.roll;
        rotationVector.y = baseAircraft.yaw;
        rotationVector.z = -baseAircraft.pitch;
        transform.eulerAngles = rotationVector;*/



        //transform.rotation = Quaternion.Euler(0, -baseAircraft.lon, baseAircraft.lat);
        transform.rotation = Quaternion.Euler(0, (float)-GameManager.PlayerAircraft.CurrentAircraftData.transform.lon, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat);
    }
}
