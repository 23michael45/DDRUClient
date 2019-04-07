using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class RenderTextureSaver : MonoBehaviour
{
    RenderTexture _RenderTexture;
    Camera _Camera;


    public int _Width = 1024;
    public int _Height = 1024;

    // Start is called before the first frame update
    void Start()
    {
        _Camera = GetComponent<Camera>();
        _RenderTexture = new RenderTexture(_Width,_Height, 16, RenderTextureFormat.ARGB32);
        _RenderTexture.Create();
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void StartTakePhoto(string savePath)
    {
        StartCoroutine(TakePhoto(savePath));
    }

    IEnumerator TakePhoto(string savePath)
    {
        _Camera.targetTexture = _RenderTexture;
        yield return new WaitForEndOfFrame();


        RenderTexture.active = _RenderTexture;
        Texture2D tex = new Texture2D(_RenderTexture.width, _RenderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, _RenderTexture.width, _RenderTexture.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();
        
        System.IO.File.WriteAllBytes(savePath, bytes);
        Debug.Log("Saved to " + savePath);

        _Camera.targetTexture = null;


        yield return new WaitForEndOfFrame();

    }
}
