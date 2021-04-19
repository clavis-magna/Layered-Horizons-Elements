/* create country outlines as map mapped to sphere
 * country outline point data from R-Studio'
 * ----
 * will only draw regions/countries that are added to the countryName array
 * these musty exactly match the region field in the data set
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class createOutlines : MonoBehaviour
{
    // file name for data set for map polygons
    // set in inspector
    // This file must be in the StreamingAssets folder
    public string filename = "";

    // parent object for outlines to sit under
    // to keep the heirachy tidy in the editor 
    public GameObject outlineParent;

    // mercator projection or globe 
    public bool drawToGlobe = true;

    // these are mostly public in order to see whats going on
    // in the inpsector
    // should not need to change in the inspector
    public outlineData[] theOutlineData;
    public Vector3[] linePoints;
    public float lastGroup = 0;
    public string lastRegion = "";

    public Material lineMaterial;
    LineRenderer theLineRenderer;

    public string[] countryName;

    // flag to check if you just want everything to be drawn
    // with the current line drawing method this is quite laggy
    public bool drawWholeWorld = false;

    // wait a fraction of a second on play before loading data
    void Start()
    {
        Invoke("loadData", 0.1f);
    }

    // load in and draw the map
    private void loadData()
    {
        //file path
        string dataFilePath = Path.Combine(Application.streamingAssetsPath, filename);

        int count = 0;

        if (File.Exists(dataFilePath))
        {
            string dataAsJson = File.ReadAllText(dataFilePath);
            theOutlineData = JsonHelper.FromJson<outlineData>(dataAsJson);
            print("data length: " + theOutlineData.Length);

            linePoints = new Vector3[theOutlineData.Length];
            bool drawThisOne = false;

            for (int i = 0; i < theOutlineData.Length; i++)
            {
                float thisLat = theOutlineData[i].lat;
                float thisLon = theOutlineData[i].lon;
                float thisGroup = theOutlineData[i].group;

                float radius = 200;

                if (lastGroup != thisGroup && lastGroup != 0 && drawThisOne)
                {
                    GameObject newGameObject = new GameObject(lastRegion);
                    newGameObject.transform.parent = outlineParent.transform;
                    theLineRenderer = newGameObject.gameObject.AddComponent<LineRenderer>() as LineRenderer;
                    theLineRenderer.material = lineMaterial;
                    theLineRenderer.SetWidth(0.1f, 0.1f);
                    theLineRenderer.positionCount = count;
                    theLineRenderer.SetPositions(linePoints);
                    count = 0;
                }

                float x;
                float y;
                float z;

                float scaleX = 100;
                float scaleY = 200;

                if (drawToGlobe)
                {
                    // map lat lon to XYZ on globe
                    float phi = (90 - thisLat) * (Mathf.PI / 180);
                    float theta = (thisLon + 180) * (Mathf.PI / 180);
                    z = -((radius) * Mathf.Sin(phi) * Mathf.Cos(theta));
                    x = ((radius) * Mathf.Sin(phi) * Mathf.Sin(theta));
                    y = ((radius) * Mathf.Cos(phi));
                }
                else
                {
                    x = (scaleX * thisLon / 180) - 180;
                    y = (scaleY * thisLat / 360);
                    z = 0;
                }

                Vector3 thisPos = new Vector3(x, y, z);

                if (System.Array.IndexOf(countryName, theOutlineData[i].region) != -1 || drawWholeWorld)
                {
                    linePoints[count] = thisPos;
                    //print(thisLat + " " + thisLon + " " + thisGroup + " " + theOutlineData[i].region);
                    drawThisOne = true;
                    count++;
                }
                else
                {
                    drawThisOne = false;
                }

                lastGroup = thisGroup;
                lastRegion = theOutlineData[i].region;

            }

            print("done");

        }
    }


}