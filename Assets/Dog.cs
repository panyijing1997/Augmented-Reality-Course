using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.Calib3dModule;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using Vuforia;
using OpenCVForUnity.ImgprocModule;

public class Dog : MonoBehaviour
{
    // The Vuforia camera
    public Camera cam;

    // Four corner points that need to be projected into image space
    public GameObject corner1;
    public GameObject corner2;
    public GameObject corner3;
    public GameObject corner4;

    // OpenCV matrix for drawing camera video feed and debug info
    private Mat camImageMat = null;

    // Camera intrinsics - depending on your camera, you might have to calibrate fx and fy
    public float fx = 650;
    public float fy = 650;
    public float cx = 320;
    public float cy = 240;

    // OpenCV matrix that should contain array of four image points
    private MatOfPoint2f imagePoints;
    private Mat texMat;

    private Renderer rd;
    public Texture2D tex;
    void Start()
    {

        // OpenCV matrices need to be allocated first to indicate size
        imagePoints = new MatOfPoint2f();
        imagePoints.alloc(4);

        rd = GetComponents<SkinnedMeshRenderer>()[0];
        /*
        corner1 = GameObject.Find("corner1");
        corner2 = GameObject.Find("corner2");
        corner3 = GameObject.Find("corner2");
        corner4 = GameObject.Find("corner2");
        */
    }

    void Update()
    {
        // Camera image from Vuforia        
        Image camImg = CameraDevice.Instance.GetCameraImage(PIXEL_FORMAT.RGBA8888);

        if (camImg != null && camImg.Height > 0)
        {
            if (camImageMat == null)
            {
                // Vuforia seems to enforce a resolution of width=640px for any camera
                Debug.Log("rows: " + camImg.Height + ", cols: " + camImg.Width);
                camImageMat = new Mat(camImg.Height, camImg.Width, CvType.CV_8UC4);
            }

            // Put Vuforia camera feed pixels into OpenCV display matrix
            camImageMat.put(0, 0, camImg.Pixels);


            // DEBUG TEST: In OpenCV, we operate in screen coordinates (pixels),
            // and we know the resolution of the Vuforia camera
            // Here, we draw a red circle in screen space using OpenCV
            //Imgproc.circle(camImageMat, new Point(300, 200), 20, new Scalar(255, 0, 0, 128));


            //---- <THIS IS WHERE THE CORNER PROJECTION BEGINS> ---- 

            // Get corner's position in world coordinates
            Vector3 worldPnt1 = corner1.transform.position;
            Vector3 worldPnt2 = corner2.transform.position;
            Vector3 worldPnt3 = corner3.transform.position;
            Vector3 worldPnt4 = corner4.transform.position;

            // Matrix that goes from world to the camera coordinate system
            Matrix4x4 Rt = cam.transform.worldToLocalMatrix;

            // Camera intrinsics
            Matrix4x4 A = Matrix4x4.identity;
            A.m00 = fx;
            A.m11 = fy;
            A.m02 = cx;
            A.m12 = cy;
            //see cheat sheet

            Matrix4x4 worldToImage = A * Rt;

            Vector3 hUV1 = worldToImage.MultiplyPoint3x4(worldPnt1);
            Vector3 hUV2 = worldToImage.MultiplyPoint3x4(worldPnt2);
            Vector3 hUV3 = worldToImage.MultiplyPoint3x4(worldPnt3);
            Vector3 hUV4 = worldToImage.MultiplyPoint3x4(worldPnt4);

            // Remember that we dealing with homogeneous coordinates.
            // Here we normalize them to get Image coordinates
            Vector2 uv1 = new Vector2(hUV1.x, hUV1.y) / hUV1.z;
            Vector2 uv2 = new Vector2(hUV2.x, hUV2.y) / hUV2.z;
            Vector2 uv3 = new Vector2(hUV3.x, hUV3.y) / hUV3.z;
            Vector2 uv4 = new Vector2(hUV4.x, hUV4.y) / hUV4.z;

            // We flip the v-coordinate of our image points to make the Unity (Vuforia) data compatible with OpenCV
            // Remember that in OpenCV the (0,0) pos is in the top left corner in contrast to the bottom left corner
            float maxV = camImg.Height - 1; // The -1 is because pixel coordinates are 0-indexed
            imagePoints.put(0, 0, uv1.x, maxV - uv1.y);
            imagePoints.put(1, 0, uv2.x, maxV - uv2.y);
            imagePoints.put(2, 0, uv3.x, maxV - uv3.y);
            imagePoints.put(3, 0, uv4.x, maxV - uv4.y);

            Point imgPnt1 = new Point(imagePoints.get(0, 0));
            Point imgPnt2 = new Point(imagePoints.get(1, 0));
            Point imgPnt3 = new Point(imagePoints.get(2, 0));
            Point imgPnt4 = new Point(imagePoints.get(3, 0));

            //For debug. Show if impPnti found the right position in img coordinate
            Imgproc.circle(camImageMat, imgPnt1, 10, new Scalar(255, 0, 0, 200), 5);
            Imgproc.circle(camImageMat, imgPnt2, 20, new Scalar(255, 255, 0, 255), 5);
            Imgproc.circle(camImageMat, imgPnt3, 30, new Scalar(0, 255, 0, 255), 5);
            Imgproc.circle(camImageMat, imgPnt4, 40, new Scalar(0, 0, 255, 255), 4);


            MatOfPoint2f unwarpPoints;
            unwarpPoints = new MatOfPoint2f();
            unwarpPoints.alloc(4);
            //according to the resolution
            unwarpPoints.put(0, 0, 0, 0);
            unwarpPoints.put(1, 0, 0, 442);
            unwarpPoints.put(2, 0, 442, 442);
            unwarpPoints.put(3, 0, 442, 0);
            //compute homography matrix

            Mat H = Calib3d.findHomography(imagePoints, unwarpPoints);
            Mat Hinv = H.inv();
            Mat dst = new Mat(442, 442, CvType.CV_8UC4);
            texMat = MatDisplay.LoadRGBATexture("/models/dog_tex.png");
            Imgproc.warpPerspective(texMat, dst, Hinv, new Size(442, 442));

            MatDisplay.MatToTexture(texMat, ref tex);
            rd.material.mainTexture = tex;
            //Debug.Log(imgPnt2);
            //Debug.Log(imgPnt2);
            //---- </THIS IS WHERE THE CORNER PROJECTION ENDS> ---- 
            // Display the Mat that includes video feed and debug points
            // Do not forget to disable Vuforia's video background and change your aspect ratio to 4:3!
            MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);



            //---- MATCH INTRINSICS OF REAL CAMERA AND PROJECTION MATRIX OF VIRTUAL CAMERA ----            
            // See lecture slides for why this formular works.
            cam.fieldOfView = 2 * Mathf.Atan(camImg.Height * 0.5f / fy) * Mathf.Rad2Deg;

        }
    }
}
