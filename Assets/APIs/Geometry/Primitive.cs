using System;
using System.Runtime.InteropServices;
using APIs.Shaders;
using JetBrains.Annotations;
using UnityEngine;

namespace APIs.Geometry
{
    public static class Primitives
    {
        private static Mesh _cubeMesh;

        public static Mesh CubeMesh
        {
            get
            {
                if (_cubeMesh == null)
                    _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                return _cubeMesh;
            }
        }
    }

    public struct Primitive
    {
        public GraphicsBuffer Vertices;
        [CanBeNull] public GraphicsBuffer Indices;
        public int? VerticesPerInstance;
        public MeshTopology Topology;
    }
}