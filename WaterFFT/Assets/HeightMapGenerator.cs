using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapGenerator : MonoBehaviour
{
    public Material heightMapMaterial;
    public Material heightAndNormalMaterial;

    public int N = 256;
    public int L = 1000;
    public float windSpeed = 40;
    public Vector2 windDirection = Vector2.right;
    public float amplitude = 4.0f;
    public float timeScale = 1.0f;
    public float time = 100.0f;
    public bool choppyWaves = true;

    [Range(0.1f, 10.0f)]
    public float normalMapStrength = 1.0f;

    public GameObject[] buoys;
    
    
    private RenderTexture h0k;
    private RenderTexture h0minusk;
    private RenderTexture twiddleFactors;
    private RenderTexture timeComponents;
    private RenderTexture choppyX;
    private RenderTexture choppyZ;
    private TimeComponentsGenerator timeComponentsGen;

    private RenderTexture pingpong0;
    private RenderTexture pingpong1;
    private RenderTexture displacement;
    private RenderTexture normalMap;
    private Texture2D displacementTexture;

    private ComputeShader butterflyShader;
    private ComputeShader inverseShader;
    private ComputeShader normalMapShader;

    private int stages;

    private void Awake() {
        butterflyShader = Resources.Load<ComputeShader>("shaders/ButterflyCalc");
        inverseShader = Resources.Load<ComputeShader>("shaders/Inverse");
        normalMapShader = Resources.Load<ComputeShader>("shaders/NormalMapCalc");

        windDirection = windDirection.normalized;

        StartingComponents startCompsGen = new StartingComponents(N);
        startCompsGen.GenerateComponents(L, amplitude * 10000, windSpeed * 10000, windDirection);
        RenderTexture h0k = startCompsGen.GetH0k();
        RenderTexture h0minusk = startCompsGen.GetH0minusk();

        TwiddleFactorsGenerator twiddleFactorsGen = new TwiddleFactorsGenerator();
        twiddleFactors = twiddleFactorsGen.Generate(N);

        timeComponentsGen = new TimeComponentsGenerator(h0k, h0minusk, N, L);
        timeComponents = timeComponentsGen.GetComponents();
        choppyX = timeComponentsGen.GetChoppyX();
        choppyZ = timeComponentsGen.GetChoppyZ();

        pingpong0 = CreateRenderTexture(N);
        pingpong1 = CreateRenderTexture(N);
        displacement = CreateRenderTexture(N);
        displacementTexture = new Texture2D(N, N, TextureFormat.RGBAFloat, false);
        normalMap = CreateRenderTexture(N);
        stages = (int)Mathf.Log(N, 2);
    }

    void Start()
    {
        heightMapMaterial.SetTexture("_UnlitColorMap", FindObjectOfType<SilhouetteRenderer>().getSilhouette());
        //heightMapMaterial.SetTexture("_UnlitColorMap", FindObjectOfType<WaveParticlesSimulator>().getHeightfield());
        heightAndNormalMaterial.SetTexture("_DisplacementMap", displacement);
        //heightMapMaterial.SetTexture("_UnlitColorMap", displacement);

        /*foreach(string name in heightAndNormalMaterial.GetTexturePropertyNames()) {
            Debug.Log(name);
        }
        Debug.Log("====================================");*/
    }

    private RenderTexture CreateRenderTexture(int size) {
        RenderTexture texture = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat);
        texture.enableRandomWrite = true;
        texture.Create();

        return texture;
    }

    public RenderTexture getDisplacementMap() {
        return displacement;
    }

    public RenderTexture getNormalMap() {
        return normalMap;
    }

    void Update()
    {
        time += Time.deltaTime * timeScale;
        timeComponentsGen.UpdateComponents(time);

        executeFFT(timeComponents, Channel.GREEN);

        if (choppyWaves) {
            executeFFT(choppyX, Channel.RED);

            executeFFT(choppyZ, Channel.BLUE);
        }

        CalculateNormals();

        foreach(GameObject buoy in buoys) {
            float x = buoy.transform.position.x;
            float z = buoy.transform.position.z;
            float y = getHeightAtPoint(x, z);
            buoy.transform.position = new Vector3(x, y, z);
        }

        //int time1 = System.DateTime.UtcNow.Millisecond;
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = displacement;
        displacementTexture.ReadPixels(new Rect(0, 0, displacement.width, displacement.height), 0, 0);
        displacementTexture.Apply();
        //int time2 = System.DateTime.UtcNow.Millisecond;
        //Debug.Log("delta time: " + (time2 - time1));
        RenderTexture.active = rt;
    }

    public float getHeightAtPoint(float x, float z) {
        return displacementTexture.GetPixelBilinear(x/L, z/L).g;
    }

    private void CalculateNormals() {
        int normalHandle = normalMapShader.FindKernel("CSMain");
        normalMapShader.SetInt("N", N);
        normalMapShader.SetFloat("strength", normalMapStrength);
        normalMapShader.SetTexture(normalHandle, "heightMap", displacement);
        normalMapShader.SetTexture(normalHandle, "normalMap", normalMap);
        normalMapShader.Dispatch(normalHandle, N / 16, N / 16, 1);
    }

    //executes the fft with currently bound textures (spectre needs to be bound to pingpong0)
    private void executeFFT(RenderTexture components, Channel channel) {
        Graphics.CopyTexture(components, pingpong0);

        int butterflyHandle = butterflyShader.FindKernel("CSMain");
        butterflyShader.SetTexture(butterflyHandle, "pingpong0", pingpong0);
        butterflyShader.SetTexture(butterflyHandle, "pingpong1", pingpong1);
        butterflyShader.SetTexture(butterflyHandle, "twiddleFactors", twiddleFactors);

        int pingpong = 0;

        //horizontal 1D FFT
        for (int i = 0; i < stages; i++) {
            butterflyShader.SetInt("pingpong", pingpong);
            butterflyShader.SetInt("stage", i);
            butterflyShader.SetInt("direction", 0); //0 == horizontal
            butterflyShader.Dispatch(butterflyHandle, N / 16, N / 16, 1);

            pingpong = 1 - pingpong;
        }

        //vertical 1D FFT
        for (int j = 0; j < stages; j++) {
            butterflyShader.SetInt("pingpong", pingpong);
            butterflyShader.SetInt("stage", j);
            butterflyShader.SetInt("direction", 1); //1 == vertical
            butterflyShader.Dispatch(butterflyHandle, N / 16, N / 16, 1);

            pingpong = 1 - pingpong;
        }

        int inverseHandle = inverseShader.FindKernel("CSMain");
        inverseShader.SetInt("N", N);
        inverseShader.SetInt("pingpong", pingpong);
        inverseShader.SetInt("output_channel", (int)channel);
        inverseShader.SetTexture(inverseHandle, "pingpong0", pingpong0);
        inverseShader.SetTexture(inverseHandle, "pingpong1", pingpong1);
        inverseShader.SetTexture(inverseHandle, "displacement", displacement);
        inverseShader.Dispatch(inverseHandle, N / 16, N / 16, 1);
    }

    private enum Channel {
        RED = 0,
        GREEN = 1,
        BLUE = 2,
        ALPHA = 3
    }
}
