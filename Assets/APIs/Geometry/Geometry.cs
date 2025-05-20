using System;
using System.Runtime.InteropServices;
using APIs.Shaders;
using UnityEngine;

namespace APIs.Geometry
{
    public static class Geometry
    {
        private static ComputeShader _computeShader;

        private static ComputeShader ComputeShader =>
            _computeShader ??= Resources.Load<ComputeShader>("Geometry");

        private static int? _calcCentroidsKernelID;

        private static int CalcCentroidsKernelID =>
            _calcCentroidsKernelID ??= ComputeShader.FindKernel("CalcCentroids");

        public static GraphicsBuffer CreateBuffersForCentroids(int indices)
        {
            GraphicsBuffer centroids = new GraphicsBuffer(GraphicsBuffer.Target.Structured, indices / 3,
                Marshal.SizeOf(typeof(Vector3)));
            return centroids;
        }

        public static void CalcCentroids(
            GraphicsBuffer positions,
            GraphicsBuffer indices,
            GraphicsBuffer centroids)
        {
            ComputeShader.SetBuffers(CalcCentroidsKernelID, ("_Vertices", positions),
                ("_Centroids", centroids),
                ("_Indices", indices));
            ComputeShader.AutoDispatch(CalcCentroidsKernelID, centroids.count);
        }
    }
}