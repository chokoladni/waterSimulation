using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterHeightSampler : MonoBehaviour
{
    private static WaterHeightSampler instance;
    private static HeightMapGenerator heightMapGenerator;

    private WaterHeightSampler() { }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        heightMapGenerator = FindObjectOfType<HeightMapGenerator>();
    }

    public static WaterHeightSampler getInstance() {
        return instance;
    }


    public float distanceToWater(Vector3 position) {
        //return position.y - 0.0f; //trenutno je voda ravna na visini 0.0f
        if (heightMapGenerator == null) {
            return position.y;
        }
        return position.y - heightMapGenerator.getHeightAtPoint(position.x, position.z);
    }

    public float getWaterHeightAtPoint(Vector3 position) {
        if (heightMapGenerator != null) {
            return heightMapGenerator.getHeightAtPoint(position.x, position.z);
        } else {
            return 0;
        }
    }
}
