using System.Runtime.InteropServices;
using APIs.Shaders;
using UnityEngine;

namespace APIs.Graphic.Buffer
{
    public class Buffer
    {
        private readonly ComputeShader _bufferVectorConvertCs;
        private readonly int _bufferVectorConvertKernel;
        private ComputeShader _bufferMeshTopologyConvertCs;

        public Buffer()
        {
            _bufferVectorConvertCs = Resources.Load<ComputeShader>("BufferVectorConvert");
            _bufferVectorConvertKernel = _bufferVectorConvertCs.FindKernel("ConvertBuffer");
            _bufferMeshTopologyConvertCs = Resources.Load<ComputeShader>("BufferTopologyConvert");
        }

        public GraphicsBuffer ConvertVectorGraphicsBuffer(GraphicsBuffer buf, int inType, int outType)
        {
            // inType/outType: 1=float, 2=float2, 3=float3, 4=float4
            int count = buf.count;
            int stride = 4 * outType; // floatN: N*4 bytes

            var outBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, stride);

            // バッファセット
            switch (inType)
            {
                case 1: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_FloatBufferIn", buf); break;
                case 2: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float2BufferIn", buf); break;
                case 3: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float3BufferIn", buf); break;
                case 4: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float4BufferIn", buf); break;
            }

            switch (outType)
            {
                case 1: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_FloatBufferOut", outBuf); break;
                case 2: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float2BufferOut", outBuf); break;
                case 3: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float3BufferOut", outBuf); break;
                case 4: _bufferVectorConvertCs.SetBuffer(_bufferVectorConvertKernel, "_Float4BufferOut", outBuf); break;
            }

            // multi_compileキーワードセット
            string[] inKeywords = { "IN_FLOAT", "IN_FLOAT2", "IN_FLOAT3", "IN_FLOAT4" };
            string[] outKeywords = { "OUT_FLOAT", "OUT_FLOAT2", "OUT_FLOAT3", "OUT_FLOAT4" };
            for (int i = 0; i < 4; ++i)
            {
                _bufferVectorConvertCs.DisableKeyword(inKeywords[i]);
                _bufferVectorConvertCs.DisableKeyword(outKeywords[i]);
            }

            _bufferVectorConvertCs.EnableKeyword(inKeywords[inType - 1]);
            _bufferVectorConvertCs.EnableKeyword(outKeywords[outType - 1]);

            _bufferVectorConvertCs.Dispatch(_bufferVectorConvertKernel, (count + 63) / 64, 1, 1);


            return outBuf;
        }

        public GraphicsBuffer ConvertMeshTopology(GraphicsBuffer indexes, MeshTopology inTopology,
            MeshTopology outTopology)
        {
            if (inTopology == MeshTopology.Triangles && outTopology == MeshTopology.Lines)
            {
                GraphicsBuffer outBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, indexes.count * 2,
                    Marshal.SizeOf(typeof(int)));
                _bufferMeshTopologyConvertCs.AutoDispatch(0, indexes.count * 2, new[]
                {
                    ("_InBuffer", indexes),
                    ("_OutBuffer", outBuf)
                });

                return outBuf;
            }

            //return not supported exception
            throw new System.NotSupportedException(
                $"ConvertMeshTopology: {inTopology} to {outTopology} is not supported");
        }
    }
}