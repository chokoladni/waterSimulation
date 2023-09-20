using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatPhysics : MonoBehaviour
{
    public GameObject underwaterObject;

    private WaterSurfaceIntersect waterIntersect;

    private Mesh underwaterMesh;

    private Rigidbody boatRigidbody;

    public float fluidRho = 1000.0f;

    //private float kinematicViscosity = 0.0000011088f; //for 16°C
    private float kinematicViscosity = 0.0000010533f; //for 18°C
    //private float kinematicViscosity = 0.0000010023f; //for 20°C

    public float Cpd1 = 10.0f;
    public float Cpd2 = 10.0f;
    public float fp = 0.5f;
    public float Csd1 = 10.0f;
    public float Csd2 = 10.0f;
    public float fs = 0.5f;
    public float accMax = 2.0f;
    public float p = 2.0f;

    //needed for slamming force
    private float[] triangleAreas;
    private float totalBoatArea;
    private Vector3[] triangleCenters;
    private DoubleBuffer<Vector3> velocityBuffer;
    private DoubleBuffer<float> submersionBuffer;

    private bool firstTime = true;
    // Start is called before the first frame update

    private float previousWaterHeight;
    private float currentWaterHeight;
    void Awake()
    {
        waterIntersect = new WaterSurfaceIntersect(this.gameObject);
        submersionBuffer = waterIntersect.getSubmersionBuffer();
        underwaterMesh = underwaterObject.GetComponent<MeshFilter>().mesh;
        boatRigidbody = gameObject.GetComponent<Rigidbody>();
        calculateTriangleAreasAndCenters();
        velocityBuffer = new DoubleBuffer<Vector3>(triangleCenters.Length);

        //TODO: modify the center of mass to a more realistic point
        boatRigidbody.centerOfMass -= new Vector3(0, 1, 0);
    }


    private void Start() {
        
    }

    private void calculateTriangleAreasAndCenters() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        int trianglesCount = triangles.Length / 3;
        triangleAreas = new float[trianglesCount];
        triangleCenters = new Vector3[trianglesCount];
        totalBoatArea = 0.0f;
        for(int i = 0; i < trianglesCount; i++) {
            Vector3 p1 = vertices[triangles[i * 3]];
            Vector3 p2 = vertices[triangles[i * 3 + 1]];
            Vector3 p3 = vertices[triangles[i * 3 + 2]];
            triangleAreas[i] = Utils.calculateTriangleArea(p1, p2, p3);
            totalBoatArea += triangleAreas[i];
            triangleCenters[i] = Utils.calculateTriangleCenter(p1, p2, p3);
        }
    }

    // Update is called once per frame
    void Update() {
        if (firstTime) {
            float h = WaterHeightSampler.getInstance().distanceToWater(transform.position);
           // transform.position -= new Vector3(0, h, 0);
            firstTime = false;
        }
    }

    private void FixedUpdate() {
        waterIntersect.displayUnderwaterMesh(underwaterMesh, "underwater");
        
        velocityBuffer.switchBuffers();
        calculateTriangleVelocities();

        previousWaterHeight = currentWaterHeight;
        currentWaterHeight = WaterHeightSampler.getInstance().getWaterHeightAtPoint(transform.position);

        float Cf = calculateResistanceCoefficient(boatRigidbody.velocity.magnitude, waterIntersect.getLength());
        waterIntersect.calculateUnderwaterTriangles();
        foreach (TriangleData triangle in waterIntersect.underwaterTriangles) {
            Vector3 totalTriangleForce = Vector3.zero;
            
            totalTriangleForce += calculateBuoyancyForce(triangle);
            totalTriangleForce += calculateViscousWaterResistanceForce(triangle, Cf);
            totalTriangleForce += calculatePressureDragForce(triangle);
            totalTriangleForce += calculateSlammingForce(triangle);

            boatRigidbody.AddForceAtPosition(totalTriangleForce, triangle.center);
        }

    }

    private void calculateTriangleVelocities() {
        Vector3[] currentBuffer = velocityBuffer.getCurrentBuffer();
        for(int i = 0; i < triangleCenters.Length; i++) {
            currentBuffer[i] = Utils.calculateObjectVelocityAtPoint(boatRigidbody, transform.TransformPoint(triangleCenters[i]));
        }
    }

    private Vector3 calculateBuoyancyForce(TriangleData triangleData) {
        Vector3 buoyancyForce = fluidRho * Physics.gravity.y * triangleData.depth * triangleData.area * triangleData.normal;
        buoyancyForce.x = 0.0f;
        buoyancyForce.z = 0.0f;

        //Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);
        //Debug.DrawRay(triangleData.center, buoyancyForce.normalized * -3f, Color.blue);

        return buoyancyForce;
    }

    private float calculateResistanceCoefficient(float velocity, float length) {
        float Re = velocity * length / kinematicViscosity;
        float Cf = 0.075f / (Mathf.Pow(Mathf.Log10(Re) - 2, 2));
        return Cf;
    }

    private Vector3 calculateViscousWaterResistanceForce(TriangleData triangleData, float Cf) {
        Vector3 n = triangleData.normal;
        Vector3 v = triangleData.velocity;
        Vector3 projectedVelocity = Vector3.Cross(n, (Vector3.Cross(v, n) / n.magnitude)) / n.magnitude; //projected onto the triangle plane
        Vector3 flowDirection = -projectedVelocity.normalized;

        Vector3 flowVelocity = flowDirection * v.magnitude;
        Vector3 resistance = 0.5f * fluidRho * Cf * triangleData.area * flowVelocity.magnitude * flowVelocity;

        if (float.IsNaN(resistance.x + resistance.y + resistance.z)) {
            Debug.Log("Viscous water resistance is NaN");
            return Vector3.zero;
        }

        //Debug.DrawRay(triangleData.center, resistance);

        return resistance;
    }
    
    private Vector3 calculatePressureDragForce(TriangleData triangleData) {
        float cosPhi = triangleData.cosPhi;
        float vi = triangleData.velocity.magnitude;

        Vector3 force;
        if (cosPhi >= 0) {
            force = -(Cpd1 * vi + Cpd2 * Mathf.Pow(vi, 2)) * Mathf.Pow(cosPhi, fp) * triangleData.normal * triangleData.area;
        } else {
            force = (Csd1 * vi + Csd2 * Mathf.Pow(vi, 2)) * Mathf.Pow(Mathf.Abs(cosPhi), fs) * triangleData.normal * triangleData.area;
        }

        if (float.IsNaN(force.x + force.y + force.z)) {
            Debug.Log("Pressure drag force is NaN");
            return Vector3.zero;
        }

        return force;
    }

    public float getWaterVelocity() {
        float velocity = (currentWaterHeight - previousWaterHeight) / Time.fixedDeltaTime;
        //Debug.Log("Water velocity: " + velocity);
        return velocity;
    }

    private Vector3 calculateSlammingForce(TriangleData triangleData) {
        int index = triangleData.originalTriangleIndex;

        Vector3 currentVelocity = velocityBuffer.getCurrentBuffer()[index];
        Vector3 previousVelocity = velocityBuffer.getPreviousBuffer()[index];

        if (triangleData.cosPhi < 0.0f) {
            return Vector3.zero;
        }


        float currentSubmergedArea = submersionBuffer.getCurrentBuffer()[index];
        float previousSubmergedArea = submersionBuffer.getPreviousBuffer()[index];

        float triangleArea = triangleAreas[index];
        float dVSweptLast = previousSubmergedArea * previousVelocity.magnitude;
        float dVSweptCurrent = currentSubmergedArea * currentVelocity.magnitude;

        float acc = (dVSweptCurrent - dVSweptLast) / (triangleArea * Time.fixedDeltaTime);

        //TODO: check whether currentVelocity or boatRigidbody.velocity should be used
        Vector3 stoppingForce = - boatRigidbody.mass * currentVelocity * 2.0f * triangleData.area / (totalBoatArea * Time.fixedDeltaTime);
        //Vector3 stoppingForce = boatRigidbody.mass * boatRigidbody.velocity * 2.0f * triangleData.area / totalBoatArea;

        Vector3 slammingForce = Mathf.Pow(Mathf.Clamp(acc / accMax, 0.0f, 1.0f), p) * triangleData.cosPhi * stoppingForce;

        /*if (index == 0) {
            Debug.DrawRay(triangleData.center, slammingForce);

            if (slammingForce.magnitude > 5) {
                Debug.Log("acc: " + acc + ", swept before: " + dVSweptLast + ", swept current: " + dVSweptCurrent + ", submerged: " + currentSubmergedArea + " / " + triangleArea);
                Debug.Log("velocity: " + currentVelocity.magnitude + ", prev velocity: " + previousVelocity.magnitude + ", boat velocity: " + boatRigidbody.velocity.magnitude);
            }
        }*/

        return slammingForce;
    }

    public WaterSurfaceIntersect getWaterSurfaceIntersect() {
        return waterIntersect;
    }

    public DoubleBuffer<Vector3> getVelocityBuffer() {
        return velocityBuffer;
    }
}
