using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapDisplay
{
    static Texture2D texture;
    public static Color[] colors;
    public static float[] color_powers;
    public static GameObject display;

    public static void ShowHeightMap(float[,] hmap)
    {
        texture = new Texture2D(hmap.GetLength(1), hmap.GetLength(0));
        // texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Mirror;
        Color[] pixs = new Color[texture.width * texture.height];
        Debug.Log(colors.Length + " " + color_powers.Length);
        for (int i = 0; i < texture.height; i++)
        {
            for (int j = 0; j < texture.width; j++)
            {
                Color c = colors[0];
                for (int x = 1; x < colors.Length && hmap[i, j] > color_powers[x - 1]; x++)
                {
                    c =  Color.Lerp(colors[x - 1], colors[x], (hmap[i, j] - color_powers[x - 1]) /
                        (color_powers[x] - color_powers[x - 1]));
                }
                pixs[i * texture.width + j] = c;
            }
        }
        texture.SetPixels(pixs);
        texture.Apply();
        
        display.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
    }
}
