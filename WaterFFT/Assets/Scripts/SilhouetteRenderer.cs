using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilhouetteRenderer : MonoBehaviour
{
    public Transform transformToTrack;
    public BoatPhysics boatPhysics;
    public Vector3 offset;

    private RenderTexture silhouette;
    private RenderTexture waveEffects;
    private RenderTexture boundaries;
    private RenderTexture distributedEffects;
    private Texture2D distributedEffectsTexture;

    private List<RenderTexture> downscaleTextures = new List<RenderTexture>(Mathf.RoundToInt(Mathf.Log(DIMENSION, 2)));
    private List<RenderTexture> upscaleTextures = new List<RenderTexture>(Mathf.RoundToInt(Mathf.Log(DIMENSION, 2)));

    private Camera renderCamera;
    private float textureWorldSize;

    private const int DIMENSION = 32;

    private List<TriangleData> underwaterTriangles;
    private DoubleBuffer<Vector3> velocityBuffer;

    private ComputeShader waveEffectsShader;
    private int waveEffectsKernel;
    private List<float> magnitudes;
    private List<Vector3> centers;
    private List<float> depths;

    private const float VOLUME_EFFECT_SCALER = 10000;

    private ComputeShader boundariesShader;
    private int boundariesKernel;

    private ComputeShader downscaleShader;
    private int downscaleKernel;
    private ComputeShader upscaleShader;
    private int upscaleKernel;

    private ComputeShader blueCopyShader;
    private int blueCopyKernel;

    private int maxTriangles;

    private WaveParticlesSimulator particleSimulator;

    private ComputeShader clearAlphaShader;
    private int clearAlphaKernel;

    private double currentTime = 0.5; // used in debug code (commented in fixed update)

    private void Awake() {
        // create the render textures
        silhouette = createRenderTexture(DIMENSION, DIMENSION);
        waveEffects = createRenderTexture(DIMENSION, DIMENSION);
        boundaries = createRenderTexture(DIMENSION, DIMENSION);
        distributedEffects = createRenderTexture(DIMENSION, DIMENSION);
        int size = DIMENSION / 2;
        while(size >= 1) {
            RenderTexture downscale = createRenderTexture(size, size);
            RenderTexture upscale = createRenderTexture(size * 2, size * 2);
            // add the downscale textures in descending order by size
            downscaleTextures.Add(downscale);
            // add upscale textures in ascending order by size
            upscaleTextures.Insert(0, upscale);
            size /= 2;
        }

        distributedEffectsTexture = new Texture2D(DIMENSION, DIMENSION, TextureFormat.RGBAFloat, false);

        // set up the camera
        renderCamera = GetComponent<Camera>();
        renderCamera.targetTexture = silhouette;
        renderCamera.enabled = false;
        textureWorldSize = renderCamera.orthographicSize * 2.0f;
        //maybe set the used shader here: camera.SetReplacementShader();

        // load wave effects shader and set constant parameters
        waveEffectsShader = Resources.Load<ComputeShader>("shaders/WaveEffects");
        waveEffectsKernel = waveEffectsShader.FindKernel("CSMain");
        waveEffectsShader.SetFloat("worldTextureDimension", textureWorldSize);
        waveEffectsShader.SetTexture(waveEffectsKernel, "waveEffectsTexture", waveEffects);

        // load boundaries shader and set constant parameters
        boundariesShader = Resources.Load<ComputeShader>("shaders/Boundaries");
        boundariesKernel = boundariesShader.FindKernel("CSMain");
        boundariesShader.SetTexture(boundariesKernel, "waveEffectsTexture", waveEffects);
        boundariesShader.SetTexture(boundariesKernel, "boundariesTexture", boundaries);

        // load upscale and downscale shaders
        upscaleShader = Resources.Load<ComputeShader>("shaders/WaveEffectsUpscale");
        upscaleKernel = boundariesShader.FindKernel("CSMain");
        downscaleShader = Resources.Load<ComputeShader>("shaders/WaveEffectsDownscale");
        downscaleKernel = boundariesShader.FindKernel("CSMain");

        blueCopyShader = Resources.Load<ComputeShader>("shaders/BlueChannelCopy");
        blueCopyKernel = blueCopyShader.FindKernel("CSMain");
        blueCopyShader.SetTexture(blueCopyKernel, "input", upscaleTextures[upscaleTextures.Count -1]);
        blueCopyShader.SetTexture(blueCopyKernel, "blueToCopy", waveEffects);
        blueCopyShader.SetTexture(blueCopyKernel, "output", distributedEffects);

        clearAlphaShader = Resources.Load<ComputeShader>("shaders/ClearAlpha");
        clearAlphaKernel = clearAlphaShader.FindKernel("CSMain");
    }

    private RenderTexture createRenderTexture(int width, int height) {
        RenderTexture texture = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBFloat);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.Create();

        return texture;
    }

    private void Start() {
        underwaterTriangles = boatPhysics.getWaterSurfaceIntersect().GetTriangleDataList();
        velocityBuffer = boatPhysics.getVelocityBuffer();
        maxTriangles = boatPhysics.getWaterSurfaceIntersect().getMaxTrianglesCount();
        magnitudes = new List<float>(maxTriangles);
        centers = new List<Vector3>(maxTriangles);
        depths = new List<float>(maxTriangles);

        particleSimulator = FindObjectOfType<WaveParticlesSimulator>();
    }

    void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        // move the camera to the offset position of the tracked boat
        transform.position = transformToTrack.position + offset;
        transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        particleSimulator.setHeightfieldCenterPoint(getTextureCenterWorldPoint());
 
        // render the silhouette using the camera and copy it to wave effects
        renderCamera.Render();
        clearAlpha(silhouette);
        Graphics.CopyTexture(silhouette, waveEffects);

        // call the wave effects shader only if there are underwater triangles
        if (underwaterTriangles.Count > 0) {
            // write the wave effects to the silhouette
            writeWaveEffects();
        }

        Graphics.CopyTexture(waveEffects, boundaries);
        // if there are no underwater triangles, boundaries are empty so there's no need to dispatch
        if (underwaterTriangles.Count > 0) {
            // determine outer boundary directions (and preserve wave effects)
            boundariesShader.Dispatch(boundariesKernel, DIMENSION / 8, DIMENSION / 8, 1);
        }

        // distribute the indirect wave effects
        distributeWaveEffects();

        // copy the final upscaled texture to final texture and copy the direct wave effects to its blue channel
        blueCopyShader.Dispatch(blueCopyKernel, distributedEffects.width / 8, distributedEffects.height / 8, 1);

        // finally, read the distributed wave effects texture and instantiate wave particles at corresponding positions
        //generateWaveParticles();

        //DEBUG CODE (generating wave particles every 0.5s)
        if (currentTime > 0.0) {
            currentTime = 0;
            generateWaveParticles();
        }
    }

    private void generateWaveParticles() {
        // read the distributed effects render texture
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = distributedEffects;
        distributedEffectsTexture.ReadPixels(new Rect(0, 0, DIMENSION, DIMENSION), 0, 0);
        distributedEffectsTexture.Apply();
        RenderTexture.active = rt;
        Color[] pixels = distributedEffectsTexture.GetPixels();

        float w = textureWorldSize / DIMENSION;
        for(int y = 0; y < DIMENSION; y++) {
            for(int x = 0; x < DIMENSION; x++) {
                int index = y * DIMENSION + x;

                Color currentColor = pixels[index];

                if (currentColor.b > 1e-3) {
                    Vector2 birthPosition = transformPixelCoordinatesToWorldPosition(x, y);
                    particleSimulator.addNewWaveParticle(birthPosition + new Vector2(0.05f, 0.05f), birthPosition, currentColor.b / VOLUME_EFFECT_SCALER, Mathf.PI * 2);
                }

                if (Mathf.Abs(currentColor.a) > 1e-4) {
                    Vector2 birthPosition = transformPixelCoordinatesToWorldPosition(x, y);
                    Vector2 direction = Utils.rotateVector(new Vector2(currentColor.r, currentColor.g), -transform.eulerAngles.y * Mathf.Deg2Rad);

                    float dispersionAngle = 0;
                    int anglesCount = 0;
                    for(int i = -1; i <= 1; i++) {
                        for(int j = -1; j <= 1; j++) {
                            int neighbourIndex = (y + i) * DIMENSION + x + j;
                            if (!(0 <= neighbourIndex && neighbourIndex < pixels.Length)) {
                                continue;
                            }
                            if (i == 0 && j == 0) {
                                continue;
                            }
                            Color neighbour = pixels[neighbourIndex];

                            Vector2 neighbourDir = Utils.rotateVector(new Vector2(neighbour.r, neighbour.g), -transform.eulerAngles.y * Mathf.Deg2Rad);
                            if (neighbourDir.magnitude > 10e-4) {
                                dispersionAngle += Mathf.Deg2Rad * Vector2.Angle(direction, neighbourDir);
                                anglesCount++;
                            }
                        }
                    }
                    dispersionAngle /= anglesCount;
                    dispersionAngle *= 2;


                    if (dispersionAngle < 0.01f) {
                        dispersionAngle = 0.01f;
                    }
                    float l = w / dispersionAngle;

                    Vector2 origin = birthPosition - l * direction;

                    //Debug.Log("birthPos: (" + birthPosition.x + ", " + birthPosition.y + "), origin: " + origin + " dispersionAngle: " + dispersionAngle);

                    particleSimulator.addNewWaveParticle(origin, birthPosition, currentColor.a / VOLUME_EFFECT_SCALER, dispersionAngle);
                }
            }
        }
    }

    private Vector2 transformPixelCoordinatesToWorldPosition(int x, int y) {
        Vector2 transformedCoords = new Vector2( textureWorldSize * (x - DIMENSION / 2) / DIMENSION, textureWorldSize * (y - DIMENSION / 2) / DIMENSION);
        //transformedCoords.x += renderCamera.transform.position.x;
        //transformedCoords.y += renderCamera.transform.position.z;
        transformedCoords += getTextureCenterWorldPoint();
        transformedCoords = Utils.rotatePointAroundPivot(transformedCoords, getTextureCenterWorldPoint(), -transform.eulerAngles.y * Mathf.Deg2Rad);
        return transformedCoords;
    }

    public RenderTexture getSilhouette() {
        return silhouette;
        //return boundaries;
        //return distributedEffects;
        //return waveEffects;
    }

    private void distributeWaveEffects() {
        /*
        D - downscale
        U - upscale
        B - boundaries
        16 - dimensions
        (1) - index in corresponding list

        (-)   (0)  (1)  (2)  (3)  (4)
        B32 → D16 → D8 → D4 → D2 → D1
                                   ↙
            U32 ← U16 ← U8 ← U4 ← U2
            (4)   (3)  (2)  (1)  (0)
        */
        for (int i = 0; i < downscaleTextures.Count; i++) {
            RenderTexture input = (i == 0) ? boundaries : downscaleTextures[i - 1];
            RenderTexture output = downscaleTextures[i];
            downscaleShader.SetTexture(downscaleKernel, "input", input);
            downscaleShader.SetTexture(downscaleKernel, "output", output);

            downscaleShader.Dispatch(downscaleKernel, output.width, output.height, 1);
        }
        
        for(int i = 0; i < upscaleTextures.Count; i++) {
            // samesize is the downscaled texture with the same dimensions as current output
            RenderTexture samesize = (i == upscaleTextures.Count - 1) ? boundaries : downscaleTextures[downscaleTextures.Count - 2 - i];
            // downscaled is the texture that is being upscaled
            RenderTexture downscaled = (i == 0) ? downscaleTextures[downscaleTextures.Count -1] : upscaleTextures[i - 1];
            RenderTexture output = upscaleTextures[i];

            Debug.Assert(samesize.width == output.width, "Samesize: " + samesize.width + ", upscaled: " + output.width);
            Debug.Assert(output.width == downscaled.width * 2, "Downscaled: " + downscaled.width + ", upscaled: " + output.width);

            upscaleShader.SetTexture(upscaleKernel, "samesizeInput", samesize);
            upscaleShader.SetTexture(upscaleKernel, "downscaledInput", downscaled);
            upscaleShader.SetTexture(upscaleKernel, "output", output);

            upscaleShader.Dispatch(upscaleKernel, samesize.width, samesize.height, 1);
        }
    }

    private void writeWaveEffects() {
        ComputeBuffer centersBuffer = new ComputeBuffer(underwaterTriangles.Count, 3 * sizeof(float));
        ComputeBuffer magnitudesBuffer = new ComputeBuffer(underwaterTriangles.Count, sizeof(float));
        ComputeBuffer depthsBuffer = new ComputeBuffer(underwaterTriangles.Count, sizeof(float));
        setUpWaveEffectsData(centersBuffer, magnitudesBuffer, depthsBuffer);

        waveEffectsShader.SetFloats("textureCenterPoint", new float[] { transform.position.x, transform.position.y, transform.position.z });
        waveEffectsShader.SetBuffer(waveEffectsKernel, "triangleCenters", centersBuffer);
        waveEffectsShader.SetBuffer(waveEffectsKernel, "waveEffects", magnitudesBuffer);
        waveEffectsShader.SetBuffer(waveEffectsKernel, "centerDepths", depthsBuffer);

        waveEffectsShader.Dispatch(waveEffectsKernel, underwaterTriangles.Count, 1, 1);

        centersBuffer.Dispose();
        magnitudesBuffer.Dispose();
        depthsBuffer.Dispose();
    }

    private void setUpWaveEffectsData(ComputeBuffer centerBuffer, ComputeBuffer magnitudesBuffer, ComputeBuffer depthsBuffer) {
        magnitudes.Clear();
        centers.Clear();
        depths.Clear();
        for(int i = 0; i < underwaterTriangles.Count; i++) {
            TriangleData triangle = underwaterTriangles[i];
            int index = triangle.originalTriangleIndex;

            Vector3 currentVelocity = velocityBuffer.getCurrentBuffer()[index];

            Vector3 waterVelocity = new Vector3(0, boatPhysics.getWaterVelocity(), 0);

            float volumeEffect = triangle.area * Vector3.Dot(triangle.velocity - waterVelocity, triangle.normal) * Time.fixedDeltaTime;
            volumeEffect *= VOLUME_EFFECT_SCALER;
            Vector2 rotatedCenterXZ = Utils.rotatePointAroundPivot(new Vector2(triangle.center.x, triangle.center.z), getTextureCenterWorldPoint(), transform.eulerAngles.y* Mathf.Deg2Rad);
            Vector3 rotatedCenter = new Vector3(rotatedCenterXZ.x, triangle.center.y, rotatedCenterXZ.y);
            centers.Add(rotatedCenter);
            magnitudes.Add(volumeEffect);
            depths.Add(triangle.depth);
        }

        centerBuffer.SetData<Vector3>(centers);
        magnitudesBuffer.SetData<float>(magnitudes);
        depthsBuffer.SetData<float>(depths);

        /*Debug.Log("_____________");
        foreach(float magnitude in depths) {
            Debug.Log("depth: " + magnitude);
        }
        Debug.Log("_____________");*/
    }

    private void clearAlpha(RenderTexture src) {
        clearAlphaShader.SetTexture(clearAlphaKernel, "src", src);
        clearAlphaShader.Dispatch(clearAlphaKernel, Mathf.CeilToInt((float)src.width / 8), Mathf.CeilToInt((float)src.height / 8), 1);
    }

    public Vector2 getTextureCenterWorldPoint() {
        return new Vector2(transform.position.x, transform.position.z);
    }
}
