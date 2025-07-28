using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float startfov=75f;
    float startheight=16;
    float startwidth=9;
    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        float aspectRatio=(float)Screen.height/(float)Screen.width;

        while (aspectRatio >startheight / startwidth)
        {
            startheight += 2;
            startfov += 7;
        }
        cam.fieldOfView=startfov; 
    }
}
