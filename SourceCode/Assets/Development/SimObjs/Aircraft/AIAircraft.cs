using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAircraft : Aircraft
{

    float fallingTimer = 0f;

    bool dead = false;
    protected override void Updated()
    {
        base.Updated();
        if (IsExploded && !dead)
        {

            if (fallingTimer > 0)
            {
                if (fallingTimer < 20)
                {
                    transform.position = transform.position - new Vector3(0, fallingTimer, 0);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.down), fallingTimer);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                    dead = true;
                }
            }


            fallingTimer +=Time.deltaTime;
        }
    }






    //static int deadCounterNumber = 10;
    //int deadCounter = deadCounterNumber;

    //SimObjData prevSimObjData;
    //void CheckComm()
    //{
    //    if (HasCommunication && GetSimObjData().alive)
    //    {
    //        if (GetSimObjData() == prevSimObjData)
    //        {
    //            deadCounter--;
    //        }
    //        else
    //        {
    //            deadCounter = deadCounterNumber;
    //            prevSimObjData = GetSimObjData();
    //        }
    //        if (deadCounter == 0)
    //        {
    //            EndComm();
    //        }
    //    }
    //}




    //protected override void FixedUpdated()
    //{
    //    base.FixedUpdated();
    //    CheckComm(); 
    //}
}
