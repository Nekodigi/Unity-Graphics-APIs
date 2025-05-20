using UnityEngine;

namespace APIs.Shaders
{
    public static class Extensions
    {
        public static void AutoDispatch(this UnityEngine.ComputeShader computeShader, int kernelID, int numElementsX)
        {
            computeShader.GetKernelThreadGroupSizes(kernelID, out uint threadNumX, out uint threadNumY,
                out uint threadNumZ);
            var threadGroupsX = Mathf.CeilToInt(numElementsX / (float)threadNumX);
            computeShader.Dispatch(kernelID, threadGroupsX, 1, 1);
        }

        public static void AutoDispatch(this UnityEngine.ComputeShader computeShader, int kernelID, int numElementsX,
            (string name, GraphicsBuffer buffer)[] buffers)
        {
            foreach (var (name, buffer) in buffers)
            {
                computeShader.SetBuffer(kernelID, name, buffer);
            }

            AutoDispatch(computeShader, kernelID, numElementsX);
        }

        public static void AutoDispatch(this UnityEngine.ComputeShader computeShader, int kernelID, int numElementsX,
            int numElementsY, int numElementsZ)
        {
            computeShader.GetKernelThreadGroupSizes(kernelID, out uint threadNumX, out uint threadNumY,
                out uint threadNumZ);
            var threadGroupsX = Mathf.CeilToInt(numElementsX / (float)threadNumX);
            var threadGroupsY = Mathf.CeilToInt(numElementsY / (float)threadNumY);
            var threadGroupsZ = Mathf.CeilToInt(numElementsZ / (float)threadNumZ);
            computeShader.Dispatch(kernelID, threadGroupsX, threadGroupsY, threadGroupsZ);
        }

        public static void AutoDispatch(this UnityEngine.ComputeShader computeShader, int kernelID, int numElementsX,
            int numElementsY, int numElementsZ, (string name, GraphicsBuffer buffer)[] buffers)
        {
            foreach (var (name, buffer) in buffers)
            {
                computeShader.SetBuffer(kernelID, name, buffer);
            }

            AutoDispatch(computeShader, kernelID, numElementsX, numElementsY, numElementsZ);
        }
    }
}