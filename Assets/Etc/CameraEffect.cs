using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    public int screen_width = 1080;
    public float DOF = 0.2f;
    public float DOF_speed = 0.02f;
    public Material ssao, dof;
    RenderTexture buffer1, buffer2;
    public float _dof = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        int scW = Screen.width;
        int scH = Screen.height;
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        Screen.SetResolution(screen_width, screen_width * scH / scW, Screen.fullScreenMode);
    }

    // Update is called once per frame
    void Update()
    {
        _dof = look.lerp(_dof, DOF, DOF_speed, 0, look.NO_MAX);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int scW = Screen.width;
        int scH = Screen.height;
        
        if (!buffer1 || !buffer1.IsCreated()) buffer1 = new RenderTexture(scW, scH, 0);
        if (!buffer2 || !buffer2.IsCreated()) buffer2 = new RenderTexture(scW, scH, 0);

        ssao.SetFloat("_Seed", Random.Range(0f, 1f));
        Graphics.Blit(source, buffer1, ssao);

        dof.SetFloat("_Seed", Random.Range(0f, 1f));
        dof.SetFloat("_Scale", _dof);
        dof.SetFloat("_HV", 1);
        Graphics.Blit(buffer1, buffer2, dof);
        dof.SetFloat("_HV", 0);
        Graphics.Blit(buffer2, destination, dof);
    }
}
