using NUnit.Framework;
using UnityEngine;

namespace Examples.CLIBased.Sum
{
    public class Sum
    {
        private ComputeShader _computeShader;
        private GraphicsBuffer _inputBuffer;
        private GraphicsBuffer _outputBuffer;
        private int _kernelIndex;
        private const int THREAD_NUM = 3;
        private const int MAX_COUNT = 6;
        private int[] _inputData;
        private int[] _outputData;

        
        [Test]
        public void SumTest()
        {
            Debug.Log("TEST START");
            _computeShader = Resources.Load<ComputeShader>("APIs/Sum/ThreadSum");
            _inputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, MAX_COUNT, sizeof(int));
            _outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(int));
            _kernelIndex = _computeShader.FindKernel("Sum");
            _computeShader.SetBuffer(_kernelIndex, "_InputBuffer", _inputBuffer);
            _computeShader.SetBuffer(_kernelIndex, "_OutputBuffer", _outputBuffer);
            _computeShader.SetInt("_InputBufferCount", MAX_COUNT);
            _inputData = new int[MAX_COUNT];
            
            for (int i = 0; i < MAX_COUNT; i++)
            {
                _inputData[i] = i;
            }
            _inputBuffer.SetData(_inputData);
            _outputData = new int[1];
            _outputBuffer.SetData(_outputData);
            
            _computeShader.Dispatch(_kernelIndex, 1, 1, 1);
            
            //debug log output
            _outputBuffer.GetData(_outputData);
            Debug.Log("Sum: " + _outputData[0]);
        }
    }
}