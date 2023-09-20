using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveParticlesSimulator : MonoBehaviour
{
    public float RADIUS = 2.0f;
    public float SPEED = 0.5f;
    public float MIN_AMPLITUDE = 0.2f;

    private const float TEXTURE_WORLD_SIZE = 100.0f;
    private const int TEXTURE_DIMENSION = 512;
    private const int DOTS_DIMENSION = 2048;

    private RenderTexture particleDots;
    private RenderTexture heightfield;
    private RenderTexture upscaledParticleDots;
    private RenderTexture gradient;

    private Texture2D particleDots2;
    private Color[] pixels;

    private const int MAX_PARTICLES = 500000;

    private float timeStep;
    private long timeStepCounter = 0;

    private double currentTime = 0.0f;

    //Index of the next free wave particle slot.
    private int nextParticleIndex = 0;
    private WaveParticle[] waveParticles = new WaveParticle[MAX_PARTICLES];
    private Dictionary<long, LinkedList<WaveParticle>> subdivisionTimeTable = new Dictionary<long, LinkedList<WaveParticle>>();

    private GameObject[] spheres = new GameObject[MAX_PARTICLES];

    private List<WaveParticle> particlesToDeactivate = new List<WaveParticle>();

    private ComputeShader waveParticleHeightfieldShader;
    private int heightfieldKernel;
    private int downscaleKernel;

    private ComputeShader heightfieldFilterShader;
    private int filterKernel;

    private Vector2 heightfieldCenterPoint = new Vector2(0, 0);

    private ComputeShader clearTextureShader;

    private void Awake() {
        gradient = new RenderTexture(TEXTURE_DIMENSION, TEXTURE_DIMENSION, 32, RenderTextureFormat.ARGBFloat);
        gradient.enableRandomWrite = true;
        gradient.filterMode = FilterMode.Point;
        gradient.Create();

        particleDots = new RenderTexture(TEXTURE_DIMENSION, TEXTURE_DIMENSION, 32, RenderTextureFormat.ARGBFloat);
        particleDots.enableRandomWrite = true;
        particleDots.filterMode = FilterMode.Point;
        particleDots.Create();

        upscaledParticleDots = new RenderTexture(DOTS_DIMENSION, DOTS_DIMENSION, 32, RenderTextureFormat.ARGBFloat);
        upscaledParticleDots.enableRandomWrite = true;
        upscaledParticleDots.filterMode = FilterMode.Point;
        upscaledParticleDots.Create();

        heightfield = new RenderTexture(TEXTURE_DIMENSION, TEXTURE_DIMENSION, 32, RenderTextureFormat.ARGBFloat);
        heightfield.enableRandomWrite = true;
        heightfield.antiAliasing = 4;
        heightfield.filterMode = FilterMode.Point;
        heightfield.Create();

        particleDots2 = new Texture2D(TEXTURE_DIMENSION, TEXTURE_DIMENSION, TextureFormat.RGBAFloat, false);
        pixels = new Color[TEXTURE_DIMENSION * TEXTURE_DIMENSION];
        particleDots2.Apply();

        waveParticleHeightfieldShader = Resources.Load<ComputeShader>("shaders/WaveParticleHeightfield");
        heightfieldKernel = waveParticleHeightfieldShader.FindKernel("CSMain");
        downscaleKernel = waveParticleHeightfieldShader.FindKernel("Downscale");
        waveParticleHeightfieldShader.SetTexture(heightfieldKernel, "particleDots", upscaledParticleDots);
        waveParticleHeightfieldShader.SetFloat("textureWorldSize", TEXTURE_WORLD_SIZE);
        waveParticleHeightfieldShader.SetTexture(downscaleKernel, "downscaled", particleDots);
        waveParticleHeightfieldShader.SetTexture(downscaleKernel, "particleDots", upscaledParticleDots);
        waveParticleHeightfieldShader.SetInt("ratio", DOTS_DIMENSION / TEXTURE_DIMENSION);

        heightfieldFilterShader = Resources.Load<ComputeShader>("shaders/WaveParticleFilterX");
        filterKernel = heightfieldFilterShader.FindKernel("CSMain");

        heightfieldFilterShader.SetFloat("textureWorldSize", TEXTURE_WORLD_SIZE);
        heightfieldFilterShader.SetTexture(filterKernel, "particleDots", particleDots);
        heightfieldFilterShader.SetTexture(filterKernel, "heightfield", heightfield);
        heightfieldFilterShader.SetTexture(filterKernel, "gradientMap", gradient);
        heightfieldFilterShader.SetFloat("particleRadius", RADIUS);
        heightfieldFilterShader.SetInt("textureDimension", TEXTURE_DIMENSION);

        clearTextureShader = Resources.Load<ComputeShader>("shaders/ClearTexture");
        
    }

    void Start()
    {
        timeStep = Time.fixedDeltaTime;

        /*insertParticle(new WaveParticle(
            0.0f,
            new Vector2(332f, 333.8f),
            new Vector2(332.2f, 334f),
            3f,
            5f,
            0.2f,
            0.0001f
            ));*/
    }

    private void FixedUpdate() {
        timeStepCounter++;
        currentTime += Time.deltaTime;

        // subdivide particles at current time step
        subdivideParticles();

        generateHeightfield();
    }

    private void generateHeightfield() {
        //clear the previous timestep heightfield and dots
        RenderTexture rt = RenderTexture.active;
        //RenderTexture.active = heightfield;
        //GL.Clear(true, true, Color.black);
        
        RenderTexture.active = particleDots;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = rt;

        clearTexture(particleDots);
        clearTexture(upscaledParticleDots);

        //render wave particles as dots
        ComputeBuffer particlesBuffer = new ComputeBuffer(waveParticles.Length, System.Runtime.InteropServices.Marshal.SizeOf(waveParticles[0]));
        //ComputeBuffer particlesBuffer = new ComputeBuffer(waveParticles.Length, 44);
        //Debug.Log("size is: " + System.Runtime.InteropServices.Marshal.SizeOf(waveParticles[0]));
        particlesBuffer.SetData(waveParticles);

        /*for(int i = 0; i < MAX_PARTICLES; i++) {
            if (waveParticles[i].amplitude < 0) {
                Debug.Log("MANJI" + waveParticles[i].amplitude);
            }
        }*/
            
        waveParticleHeightfieldShader.SetFloat("currentTime", (float)currentTime);
        waveParticleHeightfieldShader.SetFloats("worldTextureCenterPoint", new float[] { heightfieldCenterPoint.x, heightfieldCenterPoint.y });
        waveParticleHeightfieldShader.SetBuffer(heightfieldKernel, "particles", particlesBuffer);

        waveParticleHeightfieldShader.Dispatch(heightfieldKernel, waveParticles.Length / 8, 1, 1);
        particlesBuffer.Dispose();

        waveParticleHeightfieldShader.Dispatch(downscaleKernel, TEXTURE_DIMENSION / 8, TEXTURE_DIMENSION / 8, 1);


        /*
        for (int i = 0; i < TEXTURE_DIMENSION * TEXTURE_DIMENSION; i++) {
            pixels[i] = new Color(0, 0, 0, 1);
        }

        for (int i = 0; i < waveParticles.Length; i++) {
            WaveParticle p = waveParticles[i];
            if (p.active != 1) {
                continue;
            }
            Vector2 particleWorldPos = p.getPosition((float)currentTime);
            Vector2 transformedCoords = TEXTURE_DIMENSION * (particleWorldPos - heightfieldCenterPoint + new Vector2(TEXTURE_WORLD_SIZE * 0.5f, TEXTURE_WORLD_SIZE * 0.5f)) / TEXTURE_WORLD_SIZE;

            int pX = TEXTURE_DIMENSION - (int)transformedCoords.x;
            int pY = TEXTURE_DIMENSION - (int)transformedCoords.y;

            if (pX < 0 || pY < 0 || pX > TEXTURE_DIMENSION -1 || pY > TEXTURE_DIMENSION - 1) {
                continue;
            }
            pixels[TEXTURE_DIMENSION * pY + pX] += new Color(p.amplitude, 0, 0, 0);
        }
        particleDots2.SetPixels(pixels);
        particleDots2.Apply();

        RenderTexture.active = particleDots;
        GL.PushMatrix();
        GL.LoadOrtho();
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), particleDots2);
        GL.PopMatrix();
        RenderTexture.active = null;
        */

        //filter the rendered dots in x and y directions
        heightfieldFilterShader.SetFloats("textureCenterWorldPos", new float[] { heightfieldCenterPoint.x, heightfieldCenterPoint.y });
        heightfieldFilterShader.Dispatch(filterKernel, TEXTURE_DIMENSION / 8, TEXTURE_DIMENSION / 8, 1);
    }

    private void clearTexture(Texture tex) {
        clearTextureShader.SetTexture(clearTextureShader.FindKernel("CSMain"), "input", tex);
        clearTextureShader.Dispatch(clearTextureShader.FindKernel("CSMain"), tex.width / 8, tex.height / 8, 1);
    }

    public Texture getHeightfield() {
        //return upscaledParticleDots;
        return heightfield;
        //return particleDots;
        //return gradient;
    }

    public Texture getGradient() {
        return gradient;
    }

    public float getHeightfieldWorldSize() {
        return TEXTURE_WORLD_SIZE;
    }

    public Vector2 getHeightfieldCenterPoint() {
        return heightfieldCenterPoint;
    }

    private void Update() {
        for(int i = 0; i < MAX_PARTICLES; i++) {
            if (spheres[i] == null) {
                continue;
            }
            WaveParticle particle = waveParticles[i];
            if (particle.active == 0) {
                spheres[i].SetActive(false);
                continue;
            }

            spheres[i].SetActive(true);

            Vector2 currentPosition = particle.getPosition((float)currentTime);
            spheres[i].transform.position = new Vector3(currentPosition.x, 0, currentPosition.y);
            float scale = Mathf.Max(Mathf.Abs(100f * particle.amplitude), 0.1f);
            if (particle.amplitude < 0) {
                spheres[i].GetComponent<Renderer>().sharedMaterial.color = Color.red;
                
            } else {
                spheres[i].GetComponent<Renderer>().sharedMaterial.color = Color.green;
            }
            spheres[i].transform.localScale = new Vector3(scale, scale, scale);
        }
    }


    private void subdivideParticles() {
        LinkedList<WaveParticle> particlesToSubdivide;
        if (subdivisionTimeTable.TryGetValue(timeStepCounter, out particlesToSubdivide)) {
            foreach(WaveParticle particle in particlesToSubdivide) {
                if (particle.active == 0) {
                    continue;
                }

                if (waveParticles[particle.index] != particle) {
                    //Debug.Log("AHA");
                    continue;
                }


                Vector2 direction = (particle.birthPosition - particle.origin).normalized;
                float l0 = (particle.birthPosition - particle.origin).magnitude;
                float d0 = l0 * particle.dispersionAngle;
                float dt = d0 + particle.dispersionAngle * particle.speed * (float)(currentTime - particle.birthTime);

                if (Mathf.Abs(particle.amplitude / 3.0f) >= MIN_AMPLITUDE) {
                    createAndAddSubdividedParticles(particle);
                }
                else {
                    particlesToDeactivate.Add(particle);
                }
            }

            subdivisionTimeTable.Remove(timeStepCounter);

            for(int i = 0; i < particlesToDeactivate.Count; i++) {
                WaveParticle p = particlesToDeactivate[i];
                waveParticles[p.index].active = 0;
            }
            particlesToDeactivate.Clear();
        }
    }
    
    private void createAndAddSubdividedParticles(WaveParticle parent) {
        if (parent.active == 0) {
            return;
        }
        Vector2 parentPosition = parent.getPosition((float)currentTime);
        WaveParticle child1 = new WaveParticle((float)currentTime,
            parent.origin,
            Utils.rotatePointAroundPivot(parentPosition, parent.origin, parent.dispersionAngle / 3.0f),
            parent.radius,
            parent.speed,
            parent.amplitude / 3.0f,
            parent.dispersionAngle / 3.0f);

        WaveParticle child2 = new WaveParticle((float)currentTime,
            parent.origin,
            Utils.rotatePointAroundPivot(parentPosition, parent.origin, -parent.dispersionAngle / 3.0f),
            parent.radius,
            parent.speed,
            parent.amplitude / 3.0f,
            parent.dispersionAngle / 3.0f);

        parent.dispersionAngle /= 3.0f;
        parent.amplitude /= 3.0f;

        // same for all 3 particles
        float d0 = (parent.birthPosition - parent.origin).magnitude * parent.dispersionAngle;
        float divisionTime = (0.5f * parent.radius - d0) / (parent.dispersionAngle * parent.speed) + parent.birthTime;
        long divisionTimeStep = (long)(divisionTime / timeStep + 0.5f);

        if (divisionTimeStep <= timeStepCounter) {
            divisionTimeStep = timeStepCounter + 1;
        }

        particlesToDeactivate.Add(parent);
        insertParticle(child1, divisionTimeStep);
        insertParticle(new WaveParticle(parent), divisionTimeStep);
        insertParticle(child2, divisionTimeStep);
    }

    public void addNewWaveParticle(Vector2 origin, Vector2 birthPosition, float amplitude, float dispersionAngle) {
        insertParticle(new WaveParticle((float)currentTime - 0.2f, origin, birthPosition, RADIUS, SPEED, amplitude, dispersionAngle));
    }

    private void insertParticle(WaveParticle particle) {
        float d0 = (particle.birthPosition - particle.origin).magnitude * particle.dispersionAngle;
        float timeToDivision = (0.5f * particle.radius - d0) / (particle.dispersionAngle * particle.speed) + particle.birthTime;
        long divisionTimeStep = (long)(timeToDivision / timeStep + 0.5f);

        // if the division should have already happened, do it in the next time step
        if (divisionTimeStep <= timeStepCounter) {
            divisionTimeStep = timeStepCounter + 1;
        }

        insertParticle(particle, divisionTimeStep);
    }

    private void insertParticle(WaveParticle particle, long divisionTimeStep) {
        waveParticles[nextParticleIndex] = particle;
        particle.index = nextParticleIndex;
        nextParticleIndex = (nextParticleIndex + 1) % waveParticles.Length;
        
        /*if (spheres[nextParticleIndex] == null) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere.GetComponent<SphereCollider>().enabled = false;

            spheres[nextParticleIndex] = sphere;
        }*/
        
        LinkedList<WaveParticle> listAtTimeStep;
        if (!subdivisionTimeTable.TryGetValue(divisionTimeStep, out listAtTimeStep)) {
            listAtTimeStep = new LinkedList<WaveParticle>();
            subdivisionTimeTable[divisionTimeStep] = listAtTimeStep;
        }
        listAtTimeStep.AddFirst(particle);
    }

    public void setHeightfieldCenterPoint(Vector2 center) {
        heightfieldCenterPoint = center;
    }

    public struct WaveParticle
    {
        public int index;
        public float birthTime;
        public Vector2 origin;
        public Vector2 birthPosition;
        public float radius;
        public float speed;
        public float amplitude;
        public float dispersionAngle;
        public int active;

        public WaveParticle(float birthTime, Vector2 origin, Vector2 birthPosition, float radius, float speed, float amplitude, float dispersionAngle) {
            this.birthTime = birthTime;
            this.origin = origin;
            this.birthPosition = birthPosition;
            this.radius = radius;
            this.speed = speed;
            this.amplitude = amplitude;
            this.dispersionAngle = dispersionAngle;
            this.active = 1;
            this.index = -1;
        }

        public WaveParticle(WaveParticle other) {
            this.birthTime = other.birthTime;
            this.origin = other.origin;
            this.birthPosition = other.birthPosition;
            this.radius = other.radius;
            this.speed = other.speed;
            this.amplitude = other.amplitude;
            this.dispersionAngle = other.dispersionAngle;
            this.active = 1;
            this.index = -1;
        }
        
        public Vector2 getPosition(float time) {
            Vector2 direction = (birthPosition - origin).normalized;
            return birthPosition + speed * direction * (float)(time - birthTime);
            
        }

        public override bool Equals(object obj) {
            return obj is WaveParticle && this == (WaveParticle)obj;
        }

        public override int GetHashCode() {
            int hash = 13;
            hash = (hash * 7) + birthTime.GetHashCode();
            hash = (hash * 7) + origin.GetHashCode();
            hash = (hash * 7) + birthPosition.GetHashCode();
            hash = (hash * 7) + amplitude.GetHashCode();
            hash = (hash * 7) + dispersionAngle.GetHashCode();

            return hash;
        }

        public static bool operator ==(WaveParticle x, WaveParticle y) {
            return x.birthTime == y.birthTime &&
                x.origin == y.origin &&
                x.birthPosition == y.birthPosition
                && x.amplitude == y.amplitude
                && x.dispersionAngle == y.dispersionAngle;
        }

        public static bool operator !=(WaveParticle x, WaveParticle y) {
            return !(x == y);
        }
    }
}
