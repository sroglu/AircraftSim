using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaker : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        CreateWorldWithQuad();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateWorldWithQuad(){

        List<Vector3> vPos = new List<Vector3>();
        List<int> triList = new List<int>();

        int quadCount = 0;


            for(int lon = 26; lon < 45; lon++) {
        for(int lat = 36; lat < 42; lat++) {

                vPos.Add(xyzPos(lat, lon, 0));
                vPos.Add(xyzPos(lat, lon+1, 0));
                vPos.Add(xyzPos(lat+1, lon+1, 0));
                vPos.Add(xyzPos(lat+1, lon, 0));

                triList.Add(quadCount+2);
                triList.Add(quadCount+1);
                triList.Add(quadCount);

                triList.Add(quadCount+2);
                triList.Add(quadCount+0);
                triList.Add(quadCount+3);

                quadCount+=4;
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vPos.ToArray();
        //mesh.uv = newUV;
        mesh.triangles = triList.ToArray();


    }

    public Vector3 xyzPos(float lat, float lon, float altitude) {

        float earthRadius = 63.78137f;
        float z = -Mathf.Cos(lat * Mathf.Deg2Rad)*Mathf.Cos(lon * Mathf.Deg2Rad);
        float x = Mathf.Cos(lat * Mathf.Deg2Rad)*Mathf.Sin(lon * Mathf.Deg2Rad);
        float y = Mathf.Sin(lat * Mathf.Deg2Rad);
        Vector3 aircraftUp = new Vector3(x,y,z);
        Vector3 onEarthPos = aircraftUp * (earthRadius + altitude * 0.00033f);     
        return onEarthPos;

    }
}
