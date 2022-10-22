using System;
using System.Collections.Generic;
using UnityEngine;


public class Vertex : IEquatable<Vertex>
{
    public Vector3 position { get; private set; }

    public Vertex()
    {

    }

    public Vertex(Vector3 position)
    {
        this.position = position;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vertex v)
        {
            return position == v.position;
        }

        return false;
    }

    public bool Equals(Vertex other)
    {
        return position == other.position;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}


