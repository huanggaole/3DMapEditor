using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceTypes { GRASS_BLOCK, WATER, DUST, ROAD, BRIDGE };
public class MapSurfaceData
{
    public float height; // 地表高度
    public SurfaceTypes type; // 地表类型
    public int placeDirection = 0; // 地表方向，0 - 0deg；1 - 90deg；2 - 180deg；3 - 270deg，通常默认0即可
    public MapObjectData mapObjectData = null; // 地面物体，用于生成地表建筑或树木，占地面积可以大于一格，描述一个完整物体
}
