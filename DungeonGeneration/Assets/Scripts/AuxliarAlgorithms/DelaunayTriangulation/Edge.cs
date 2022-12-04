using System;
using System.Collections.Generic;
using UnityEngine;

//Class for representing a edge
public class Edge : IEquatable<Edge>
{
    public Vertex U { get; set; }
    public Vertex V { get; set; }
    public float Distance { get; private set; }
    public bool IsBad { get; set; }

    public Edge()
    {

    }

    public Edge(Vertex u, Vertex v)
    {
        U = u;
        V = v;

        Distance = Vector2Int.Distance(u.position, v.position);
    }

    public static bool operator ==(Edge left, Edge right)
    {
        return (left.U == right.U || left.U == right.V)
            && (left.V == right.U || left.V == right.V);
    }

    public static bool operator !=(Edge left, Edge right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is Edge e)
        {
            return this == e;
        }

        return false;
    }

    public bool Equals(Edge e)
    {
        return this == e;
    }

    public override int GetHashCode()
    {
        return U.GetHashCode() ^ V.GetHashCode();
    }

    public static bool AlmostEqual(float x, float y)
    {
        return Mathf.Abs(x - y) <= float.Epsilon * Mathf.Abs(x + y) * 2
            || Mathf.Abs(x - y) < float.MinValue;
    }

    private static bool AlmostEqual(Vertex left, Vertex right)
    {
        return AlmostEqual(left.position.x, right.position.x) && AlmostEqual(left.position.y, right.position.y);
    }

    public static bool AlmostEqual(Edge left, Edge right)
    {
        return AlmostEqual(left.U, right.U) && AlmostEqual(left.V, right.V)
            || AlmostEqual(left.U, right.V) && AlmostEqual(left.V, right.U);
    }
}

