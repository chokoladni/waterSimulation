using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: vidjeti radi li dobro submersionBuffer
public class WaterSurfaceIntersect {

    private Transform boatTransform; //transform of the boat whose intersection with water surface is being calculated
    
    Vector3[] boatVertices; //local positions of boat vertices
    Vector3[] boatVerticesGlobal; //global positions of boat vertices (added boatTransform.position)
    
    int[] boatTriangleIndices; //form triangles in threes

    float[] distanceToWater; //index is index of the vertex in boatVertices
    
    public List<TriangleData> underwaterTriangles = new List<TriangleData>();

    private Rigidbody boatRigidbody;

    //for calculating the length
    private float maxZ = float.MinValue;
    private float minZ = float.MaxValue;

    private DoubleBuffer<float> submersionBuffer;
    float[] currentSubmersionBuffer;

    public WaterSurfaceIntersect(GameObject boat) {
        boatTransform = boat.transform;

        Mesh boatMesh = boat.GetComponent<MeshFilter>().mesh;
        boatVertices = boatMesh.vertices;
        boatTriangleIndices = boatMesh.triangles;

        boatVerticesGlobal = new Vector3[boatVertices.Length];
        distanceToWater = new float[boatVertices.Length];
        boatRigidbody = boat.GetComponent<Rigidbody>();

        submersionBuffer = new DoubleBuffer<float>(boatMesh.triangles.Length / 3);
    }

    public void calculateUnderwaterTriangles() {
        underwaterTriangles.Clear();

        submersionBuffer.switchBuffers();
        currentSubmersionBuffer = submersionBuffer.getCurrentBuffer();

        for(int i = 0; i < boatVertices.Length; i++) {
            boatVerticesGlobal[i] = boatTransform.TransformPoint(boatVertices[i]);
            distanceToWater[i] = WaterHeightSampler.getInstance().distanceToWater(boatVerticesGlobal[i]);
        }

        processTriangles();

        
    }

    private void processTriangles() {
        maxZ = float.MinValue;
        minZ = float.MaxValue;

        int triangleCount = boatTriangleIndices.Length / 3;
        VertexData[] triangleVerticesData = new VertexData[3];
        for(int t = 0; t < triangleCount; t++) {
            for(int i = 0; i < 3; i++) {
                triangleVerticesData[i].index = i;
                triangleVerticesData[i].distance = distanceToWater[boatTriangleIndices[3 * t + i]];
                triangleVerticesData[i].globalVertexPos = boatVerticesGlobal[boatTriangleIndices[3 * t + i]];
            }

            currentSubmersionBuffer[t] = 0.0f;

            int underwaterCount = countUnderwaterVertices(triangleVerticesData);
            switch (underwaterCount) {
                case 3: {
                        TriangleData newTriangle = new TriangleData(triangleVerticesData[0].globalVertexPos,
                                                                    triangleVerticesData[1].globalVertexPos,
                                                                    triangleVerticesData[2].globalVertexPos,
                                                                    t,
                                                                    boatRigidbody);
                        underwaterTriangles.Add(newTriangle);
                        currentSubmersionBuffer[t] += newTriangle.area;
                        checkZCoordinates(triangleVerticesData[0].globalVertexPos,
                                          triangleVerticesData[1].globalVertexPos,
                                          triangleVerticesData[2].globalVertexPos);
                        break;
                    }
                case 2: {
                        addTrianglesForOneVertexAboveWater(triangleVerticesData, t);
                        break;
                    }
                case 1: {
                        addTrianglesForTwoVerticesAboveWater(triangleVerticesData, t);
                        break;
                    }
            }
        }
    }

    private void addTrianglesForOneVertexAboveWater(VertexData[] triangleVertexData, int originalTriangleIndex) {
        Array.Sort(triangleVertexData, (x, y) => { return y.distance.CompareTo(x.distance); });

        Vector3 H = triangleVertexData[0].globalVertexPos;
        Vector3 M = triangleVertexData[1].globalVertexPos;
        Vector3 L = triangleVertexData[2].globalVertexPos;

        float hH = triangleVertexData[0].distance;
        float hM = triangleVertexData[1].distance;
        float hL = triangleVertexData[2].distance;

        Vector3 MH = H - M;
        Vector3 LH = H - L;

        float tM = -hM / (hH - hM);
        float tL = -hL / (hH - hL);

        Vector3 IM = M + tM * MH;
        Vector3 IL = L + tL * LH;

        checkZCoordinates(M, IM, IL, L);

        //check the order of vertices H, M, L and add new triangles to match the orientation of original triangle
        bool direction = (triangleVertexData[0].index == (triangleVertexData[1].index + 1) % 3);
        TriangleData t1 = direction ? new TriangleData(M, IM, IL, originalTriangleIndex, boatRigidbody) : new TriangleData(M, IL, IM, originalTriangleIndex, boatRigidbody);
        TriangleData t2 = direction ? new TriangleData(M, IL, L, originalTriangleIndex, boatRigidbody) : new TriangleData(M, L, IL, originalTriangleIndex, boatRigidbody);
        underwaterTriangles.Add(t1);
        underwaterTriangles.Add(t2);
        currentSubmersionBuffer[originalTriangleIndex] += t1.area + t2.area;
    }

    private void addTrianglesForTwoVerticesAboveWater(VertexData[] triangleVertexData, int originalTriangleIndex) {
        Array.Sort(triangleVertexData, (x, y) => { return y.distance.CompareTo(x.distance); });

        Vector3 H = triangleVertexData[0].globalVertexPos;
        Vector3 M = triangleVertexData[1].globalVertexPos;
        Vector3 L = triangleVertexData[2].globalVertexPos;

        float hH = triangleVertexData[0].distance;
        float hM = triangleVertexData[1].distance;
        float hL = triangleVertexData[2].distance;

        Vector3 LM = M - L;
        Vector3 LH = H - L;

        float tM = -hL / (hM - hL);
        float tH = -hL / (hH - hL);

        Vector3 JH = L + tH * LH;
        Vector3 JM = L + tM * LM;

        checkZCoordinates(L, JM, JH);

        bool direction = (triangleVertexData[0].index == (triangleVertexData[1].index + 1) % 3);
        TriangleData t = direction ? new TriangleData(L, JM, JH, originalTriangleIndex, boatRigidbody) : new TriangleData(L, JH, JM, originalTriangleIndex, boatRigidbody);
        underwaterTriangles.Add(t);
        currentSubmersionBuffer[originalTriangleIndex] += t.area;
    }

    private int countUnderwaterVertices(VertexData[] verticesData) {
        int count = 0;
        foreach(VertexData data in verticesData) {
            if (data.distance < 0.0f) {
                count++;
            }
        }

        return count;
    }

    public void displayUnderwaterMesh(Mesh mesh, string name) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < underwaterTriangles.Count; i++) {
            Vector3 p1 = boatTransform.InverseTransformPoint(underwaterTriangles[i].p1);
            Vector3 p2 = boatTransform.InverseTransformPoint(underwaterTriangles[i].p2);
            Vector3 p3 = boatTransform.InverseTransformPoint(underwaterTriangles[i].p3);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }

        mesh.Clear();
        mesh.name = name;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void checkZCoordinates(params Vector3[] vertices) {
        foreach(Vector3 gVertex in vertices) {
            Vector3 vertex = boatTransform.InverseTransformPoint(gVertex);
            if (vertex.z > maxZ) {
                maxZ = vertex.z;
            }
            if (vertex.z < minZ) {
                minZ = vertex.z;
            }
        }
    }

    public float getLength() {
        float length = maxZ - minZ;
        if (length < 0.0f) {
            length = 0.0f;
        }

        return length;
    }

    public int getMaxTrianglesCount() {
        return 2 * boatTriangleIndices.Length / 3;
    }

    public DoubleBuffer<float> getSubmersionBuffer() {
        return submersionBuffer;
    }

    public List<TriangleData> GetTriangleDataList() {
        return underwaterTriangles;
    }

    private struct VertexData
    {
        public int index;
        public float distance;
        public Vector3 globalVertexPos;
    }
}
