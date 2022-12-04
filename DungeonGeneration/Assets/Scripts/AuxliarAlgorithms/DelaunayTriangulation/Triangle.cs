using System;
using UnityEngine;

//Class for representing a triangle
public class Triangle : IEquatable<Triangle>
{
    public Vertex A { get; set; }
    public Vertex B { get; set; }
    public Vertex C { get; set; }
    public bool IsBad { get; set; }

    public Triangle()
    {

    }

    public Triangle(Vertex a, Vertex b, Vertex c)
    {
        A = a;
        B = b;
        C = c;
    }

    public bool ContainsVertex(Vector2Int v)
    {
        return Vector2Int.Distance(v, A.position) < 0.01f
            || Vector2Int.Distance(v, B.position) < 0.01f
            || Vector2Int.Distance(v, C.position) < 0.01f;
    }

    public bool CircumCircleContains(Vector2Int v)
    {
        Vector2Int a = A.position;
        Vector2Int b = B.position;
        Vector2Int c = C.position;

        float ab = a.sqrMagnitude;
        float cd = b.sqrMagnitude;
        float ef = c.sqrMagnitude;

        float circumX = (ab * (c.y - b.y) + cd * (a.y - c.y) + ef * (b.y - a.y)) / (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y));
        float circumY = (ab * (c.x - b.x) + cd * (a.x - c.x) + ef * (b.x - a.x)) / (a.y * (c.x - b.x) + b.y * (a.x - c.x) + c.y * (b.x - a.x));

        Vector2Int circum = new Vector2Int((int)(circumX / 2.0f), (int)(circumY / 2.0f));
        float circumRadius = Vector3.SqrMagnitude(new Vector3((a - circum).x, (a - circum).y));
        float dist = Vector3.SqrMagnitude(new Vector3((v - circum).x, (v - circum).y));
        return dist <= circumRadius;
    }

    public static bool operator ==(Triangle left, Triangle right)
    {
        return (left.A == right.A || left.A == right.B || left.A == right.C)
            && (left.B == right.A || left.B == right.B || left.B == right.C)
            && (left.C == right.A || left.C == right.B || left.C == right.C);
    }

    public static bool operator !=(Triangle left, Triangle right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is Triangle t)
        {
            return this == t;
        }

        return false;
    }

    public bool Equals(Triangle t)
    {
        return this == t;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
    }
}
