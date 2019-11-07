using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class Pics : MonoBehaviour
{

    // Training output directory
    static string trainingDir = "C:\\Users\\emh6lxs\\Documents\\Room Corner Generator\\training4";

    Camera camera;
    static int i = getCurrentTrainingIteration(trainingDir) * 2;
    Vector3[] curGroundPoints;

    // Training data point count
    int trainingDataPoints = 1000000;

    // Global Room points and dimensions
    Vector3 notXZGround = new Vector3(0, 0, 2.6196f);
    Vector3 xZGround = new Vector3(10.04f, 0, 2.6196f);
    Vector3 xNotZGround = new Vector3(10.04f, 0, -10.004f);
    Vector3 notXNotZGround = new Vector3(0, 0, -10.004f);
    static float ceilingHeight = 7.983f;

    // 3d center points of walls
    static Vector3 degree0Center = new Vector3(10.04f, ceilingHeight / 2, -4f);
    static Vector3 degree90Center = new Vector3(5.02f, ceilingHeight / 2, -10.004f);
    static Vector3 degree180Center = new Vector3(0, ceilingHeight / 2, -4f);
    static Vector3 degree270Center = new Vector3(5.02f, ceilingHeight / 2, 2.5f);
    Vector3[] wallCenterPoints = new Vector3[] { degree0Center, degree90Center, degree180Center, degree270Center };

    Vector3 defaultCamPos = new Vector3(0.22f, 2.713f / 2, 2.5f);
    float camDefaultFieldOfView = 115.5f;
    float defaultCamYRotation = 240f; // Default Y rotation when staring directly at X wall
    float maxXZCamPositionJitter = 0.5f; // World coord distance
    float maxYCamPositionJitter = 2.713f / 2; // Jitter between ceiling and floor
    float maxZCamRotationJitter = 15f; // Degrees
    float maxXCamRotationJitter = 30f; // Degrees
    float maxFieldOfViewJitter = 30f;

    System.Random random = new System.Random();

    static int getCurrentTrainingIteration(string trainingDir)
    {
        var latestFiles = (from f in new DirectoryInfo(trainingDir + "\\labels").GetFiles()
                               orderby f.LastWriteTime descending
                               select f);

        if (latestFiles.Count() == 0)
        {
            return 0;
        }
        int curIter = int.Parse(latestFiles.First().Name.Split('.')[0]) + 1;
        Debug.Log("Resuming from: " + curIter);
        return curIter;
    }

    // Randomly reposition camera within "sensible" bounds,
    // and return the left / right ground corner world points
    Vector3[] shakeCamera()
    {
        var xShake = (float)(maxXZCamPositionJitter * random.NextDouble());
        var zShake = (float)(maxXZCamPositionJitter * random.NextDouble());
        var yShake = (float)(maxYCamPositionJitter * random.NextDouble());
        var rotationYShake = (float)(360 * random.NextDouble());
        var rotationXShake = (float)(maxXCamRotationJitter * random.NextDouble()) * ((random.NextDouble() < 0.5) ? -1 : 1);
        var rotationZShake = (float)(maxZCamRotationJitter * random.NextDouble()) * ((random.NextDouble() < 0.5) ? -1 : 1);
        var fieldOfViewShake = (float)(maxFieldOfViewJitter * random.NextDouble()) * ((random.NextDouble() < 0.5) ? -1 : 1);

        camera.transform.localPosition = new Vector3(defaultCamPos.x + xShake, defaultCamPos.y + yShake, defaultCamPos.z + zShake);
        camera.transform.localRotation = Quaternion.Euler(rotationXShake, defaultCamYRotation + rotationYShake, rotationZShake);
        camera.fieldOfView = camDefaultFieldOfView + fieldOfViewShake;

        var toReturn = new Vector3[2];

        // Find the wall whose center point is closest to camera viewport's center point
        var minIndex = 0;
        var minDistance = float.MaxValue;

        // Project the camera forward through the wall and grab that point
        var cameraFocusWorldPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10));
        for (var w = 0; w < wallCenterPoints.Length; w++)
        {
            var wDistance = Vector3.Distance(cameraFocusWorldPoint, wallCenterPoints[w]);
            if (wDistance < minDistance)
            {
                minDistance = wDistance;
                minIndex = w;
            }
        }

        // If XZ Wall
        if (minIndex == 0)
        {
            toReturn[0] = xZGround;
            toReturn[1] = xNotZGround;
        }
        // If XnotZ wall
        else if (minIndex == 1)
        {
            toReturn[0] = xNotZGround;
            toReturn[1] = notXNotZGround;
        }
        // If notXnotZ wall
        else if (minIndex == 2)
        {
            toReturn[0] = notXNotZGround;
            toReturn[1] = notXZGround;
        }
        else
        {
            toReturn[0] = notXZGround;
            toReturn[1] = xZGround;
        }

        return toReturn;
    }

    void takeScreenshot()
    {
        var resWidth = camera.pixelWidth;
        var resHeight = camera.pixelHeight;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = trainingDir + "\\imgs\\" + ((i-1)/2) + ".png";
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    void generateRandomDataPoint(Vector3[] groundCornerWorldPoints)
    {
        var imgLeftCeiling = camera.WorldToViewportPoint(new Vector3(groundCornerWorldPoints[0].x, ceilingHeight, groundCornerWorldPoints[0].z));
        var imgRightCeiling = camera.WorldToViewportPoint(new Vector3(groundCornerWorldPoints[1].x, ceilingHeight, groundCornerWorldPoints[1].z));
        var imgLeftGround = camera.WorldToViewportPoint(groundCornerWorldPoints[0]);
        var imgRightGround = camera.WorldToViewportPoint(groundCornerWorldPoints[1]);

        // Save wall points relative to camera's 2d space
        string json = "[" + "[" + imgLeftCeiling.x + "," + imgLeftCeiling.y + "]," +
            "[" + imgRightCeiling.x + "," + imgRightCeiling.y + "]," +
            "[" + imgLeftGround.x + "," + imgLeftGround.y + "]," +
            "[" + imgRightGround.x + "," + imgRightGround.y + "]]";

        System.IO.File.WriteAllText(trainingDir + "\\labels\\" + ((i-1)/2) + ".json", json);

        // Save camera snapshot
        takeScreenshot();
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        camera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (i*2 < trainingDataPoints)
        {
            if (i % 2 == 0)
            {
                curGroundPoints = shakeCamera();
            }
            else
            {
                generateRandomDataPoint(curGroundPoints);
            }
        }
        i++;
    }
}
