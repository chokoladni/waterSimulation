using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNoiseTexture : MonoBehaviour
{
    public static Texture2D GenerateTexture(int textureDimension)
    {
        var texture = new Texture2D(textureDimension, textureDimension, TextureFormat.RGB24, true);

        for(int i = 0; i < textureDimension; i++) {
            for (int j = 0; j < textureDimension; j++) {
                float r = Random.Range(0.0f, 1.0f);
                texture.SetPixel(i, j, new Color(r, r, r, 1.0f));
            }
        }
        texture.Apply();

        return texture;
    }
}
