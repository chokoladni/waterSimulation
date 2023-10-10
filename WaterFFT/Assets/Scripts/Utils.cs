using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector2 rotatePointAroundPivot(Vector2 pointToRotate, Vector2 pivot, float angle) {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        pointToRotate -= pivot;
        float newX = pointToRotate.x * cos - pointToRotate.y * sin;
        float newY = pointToRotate.x * sin + pointToRotate.y * cos;
        pointToRotate.x = newX;
        pointToRotate.y = newY;
        
        pointToRotate += pivot;

        return pointToRotate;
    }

    public static Vector2 rotateVector(Vector2 vec, float angle) {
        return rotatePointAroundPivot(vec, Vector2.zero, angle);
    }
    
    public static Vector3 calculateObjectVelocityAtPoint(Rigidbody body, Vector3 point) {
        return body.velocity + Vector3.Cross(body.angularVelocity, point - body.worldCenterOfMass);
    }

    public static float calculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3) {
        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p3, p1);

        return 0.5f * a * b * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad);
    }

    public static Vector3 calculateTriangleCenter(Vector3 p1, Vector3 p2, Vector3 p3) {
        return (p1 + p2 + p3) / 3.0f;
    }
}
