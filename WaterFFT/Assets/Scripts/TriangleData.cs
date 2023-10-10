using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TriangleData
{
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public Vector3 center;
    public Vector3 normal;
    public Vector3 velocity;
    public float cosPhi;

    public float area;
    public float depth;

    public int originalTriangleIndex;

    public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3, int originalTriangleIndex, Rigidbody boatRigidbody) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.originalTriangleIndex = originalTriangleIndex;

        this.center = (p1 + p2 + p3) / 3.0f;
        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        this.area = Utils.calculateTriangleArea(p1, p2, p3);
        this.depth = -WaterHeightSampler.getInstance().distanceToWater(center);

        this.velocity = Utils.calculateObjectVelocityAtPoint(boatRigidbody, center);
        this.cosPhi = Vector3.Dot(this.velocity.normalized, this.normal);
    }

}
