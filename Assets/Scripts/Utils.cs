using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static bool PointOnLine2D(this Vector2 p, Vector2 a, Vector2 b, float t = 1E-03f)
    {
        // ensure points are collinear
        var zero = (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
        if (zero > t || zero < -t) return false;

        // check if x-coordinates are not equal
        if (a.x - b.x > t || b.x - a.x > t)
            // ensure x is between a.x & b.x (use tolerance)
            return a.x > b.x
                ? p.x + t > b.x && p.x - t < a.x
                : p.x + t > a.x && p.x - t < b.x;

        // ensure y is between a.y & b.y (use tolerance)
        return a.y > b.y
            ? p.y + t > b.y && p.y - t < a.y
            : p.y + t > a.y && p.y - t < b.y;
    }

    public static bool AngleCompare(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, float t = 1E-03f)
    {
        Vector2 dir1 = (a2 - a1).normalized;
        Vector2 dir2 = (b2 - b1).normalized;
        return Vector2.Distance(dir1, dir2) < t;
    }
}