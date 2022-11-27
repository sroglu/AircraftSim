using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour
{
    // For other class to access this singleton
    static FeedbackUI instance;
    public static FeedbackUI Instance
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

    private void Awake()
    {
        Instance = this;
    }


    public Text x, y,action,dest,cursor, DMS_TMS;

    public void ShowVals(string xTxt,string yTxt, string actionTxt,string dest, string cursor,string DMS_TMS)
    {
        x.text = xTxt;
        y.text = yTxt;
        action.text = actionTxt;
        this.dest.text = dest;
        this.cursor.text = cursor;
        this.DMS_TMS.text = DMS_TMS;
    }
}
