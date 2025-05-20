using System;
using UnityEngine;

namespace APIs.Geometry
{
    public struct Primitive
    {
        public GraphicsBuffer Vertices;
        public GraphicsBuffer Indices;
        public MeshTopology Topology;
    }
}