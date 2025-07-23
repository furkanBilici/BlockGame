using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float startfov=70f;
    float startheight=16;
    float startwidth=9;
    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        float aspectRatio=(float)Screen.height/(float)Screen.width;

        while (aspectRatio >startheight / startwidth)
        {
            startheight += 2;
            startfov += 5;
        }
        cam.fieldOfView=startfov; 
    }
}
