using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaLeader : MonoBehaviour
{
    public float viewDistance;
    public GameObject visuableKey;
    public GameObject vcamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void merge()
    {
        /*MeshFilter[] meshFilters = visuableKey.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)                                  //遍历
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;                   //将共享mesh，赋值
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix; //本地坐标转矩阵，赋值
        }
        Mesh newMesh = new Mesh();                                  //声明一个新网格对象
        newMesh.CombineMeshes(combineInstances);                    //将combineInstances数组传入函数
        visuableKey.gameObject.AddComponent<MeshFilter>().sharedMesh = newMesh; //给当前空物体，添加网格组件；将合并后的网格，给到自身网格
        */
    }

    // Update is called once per frame
    void Update()
    {
        vcamera.transform.localPosition = new Vector3(0, 0, viewDistance) / 5;
        Vector2 camera2DPos = new Vector2(
            vcamera.transform.position.x,
            vcamera.transform.position.z);
        Vector2 leaderPos = new Vector2(transform.position.x, transform.position.z);
        if (Vector2.Distance(camera2DPos, leaderPos) > viewDistance)
        {
            visuableKey.SetActive(false);
        }
        else
        {
            visuableKey.SetActive(true);
        }
    }
}
