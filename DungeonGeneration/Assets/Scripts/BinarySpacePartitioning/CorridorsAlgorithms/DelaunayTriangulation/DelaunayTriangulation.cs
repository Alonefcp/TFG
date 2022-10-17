using System;
using System.Collections.Generic;
using UnityEngine;

//Bowyer-Watson algorithm for Delaunay Triangulation
public static class DelaunayTriangulation
{
    private static List<Vertex> vertices;
    private static List<Edge> edges;
    private static List<Triangle> triangles;

   public static List<Edge> Triangulate(List<Vertex> vertices)
   {
        edges = new List<Edge>();
        triangles = new List<Triangle>();
        DelaunayTriangulation.vertices = new List<Vertex>(vertices);

        float minX = DelaunayTriangulation.vertices[0].position.x;
        float minY = DelaunayTriangulation.vertices[0].position.y;
        float maxX = minX;
        float maxY = minY;

        foreach (var vertex in DelaunayTriangulation.vertices)
        {
            if (vertex.position.x < minX) minX = vertex.position.x;
            if (vertex.position.x > maxX) maxX = vertex.position.x;
            if (vertex.position.y < minY) minY = vertex.position.y;
            if (vertex.position.y > maxY) maxY = vertex.position.y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy) * 2;

        Vertex p1 = new Vertex(new Vector2(minX - 1, minY - 1));
        Vertex p2 = new Vertex(new Vector2(minX - 1, maxY + deltaMax));
        Vertex p3 = new Vertex(new Vector2(maxX + deltaMax, minY - 1));

        triangles.Add(new Triangle(p1, p2, p3));

        foreach (var vertex in DelaunayTriangulation.vertices)
        {
            List<Edge> polygon = new List<Edge>();

            foreach (var t in triangles)
            {
                if (t.CircumCircleContains(vertex.position))
                {
                    t.IsBad = true;
                    polygon.Add(new Edge(t.A, t.B));
                    polygon.Add(new Edge(t.B, t.C));
                    polygon.Add(new Edge(t.C, t.A));
                }
            }

            triangles.RemoveAll((Triangle t) => t.IsBad);

            for (int i = 0; i < polygon.Count; i++)
            {
                for (int j = i + 1; j < polygon.Count; j++)
                {
                    if (Edge.AlmostEqual(polygon[i], polygon[j]))
                    {
                        polygon[i].IsBad = true;
                        polygon[j].IsBad = true;
                    }
                }
            }

            polygon.RemoveAll((Edge e) => e.IsBad);

            foreach (var edge in polygon)
            {
                triangles.Add(new Triangle(edge.U, edge.V, vertex));
            }
        }

        triangles.RemoveAll((Triangle t) => t.ContainsVertex(p1.position) || t.ContainsVertex(p2.position) || t.ContainsVertex(p3.position));

        HashSet<Edge> edgeSet = new HashSet<Edge>();

        foreach (var t in triangles)
        {
            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);

            if (edgeSet.Add(ab))
            {
                edges.Add(ab);
            }

            if (edgeSet.Add(bc))
            {
                edges.Add(bc);
            }

            if (edgeSet.Add(ca))
            {
                edges.Add(ca);
            }
        }

        return edges;
   }
}
