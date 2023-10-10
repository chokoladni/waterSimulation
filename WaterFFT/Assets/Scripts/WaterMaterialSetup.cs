using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMaterialSetup : MonoBehaviour
{

    public Material waterMaterial;
    private WaveParticlesSimulator particleSimulator;
    
    void Start()
    {
        HeightMapGenerator heightmapGen = FindObjectOfType<HeightMapGenerator>();
        waterMaterial.SetTexture("_DisplacementMap", heightmapGen.getDisplacementMap());
        waterMaterial.SetTexture("_NormalMap", heightmapGen.getNormalMap());

        particleSimulator = FindObjectOfType<WaveParticlesSimulator>();

        waterMaterial.SetTexture("_BoatHeightfield", particleSimulator.getHeightfield());
        waterMaterial.SetInt("_BoatHeightfieldDimension", particleSimulator.getHeightfield().width);
        waterMaterial.SetFloat("_BoatHeightfieldWorldSize", particleSimulator.getHeightfieldWorldSize());
        waterMaterial.SetTexture("_BoatGradient", particleSimulator.getGradient());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        waterMaterial.SetVector("_BoatHeightfieldCenterPos", particleSimulator.getHeightfieldCenterPoint());
    }
}
