using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptureScreenshot : MonoBehaviour
{
    // Start is called before the first frame update
    int width=4096;
    int Height=2160;
    int count = 0;

    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("space"))
        {
            StartCoroutine(CapturePic());
        }


    }
    IEnumerator CapturePic()
    {
        byte[] Image_bytes;
        int resWidth = width;
        int resHeight = Height;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        yield return new WaitForEndOfFrame();
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        Image_bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Screenshot_" + count + ".png", Image_bytes);
        print("Screenshot captured");
        count++;
    }
    
       


}
