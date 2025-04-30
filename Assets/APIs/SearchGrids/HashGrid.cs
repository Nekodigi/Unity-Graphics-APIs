using UnityEngine;

namespace APIs.SearchGrids
{
    public class HashGrid
    {
        private readonly float _cellSize;

        private ComputeShader _computeShader;

        private int _hashDataKernelID;
        private int _clearOffsetKernelID;
        private int _calculateOffsetKernelID;
        private int _bitonicSortKernelID;

        private ComputeBuffer _particleBuffer;
        private int _count;
        public ComputeBuffer IndexBuffer { get; private set; }
        public ComputeBuffer CellIndexBuffer { get; private set; }
        public ComputeBuffer CellOffsetBuffer { get; private set; }

        public HashGrid(float cellSize, ComputeShader computeShader, ComputeBuffer particleBuffer)
        {
            _cellSize = cellSize;
            _computeShader = computeShader;
            _particleBuffer = particleBuffer;
            _count = particleBuffer.count;
            InitHashGrid();
        }

        public void Update()
        {
            ClearOffsetBuffer();
            DispatchKernel(_computeShader, _hashDataKernelID, _count);
            Sort();
            CalculateOffsets();
        }

        public void OnDestroy()
        {
            ReleaseBuffers();
        }

        public void InitHashGrid()
        {
            _hashDataKernelID = _computeShader.FindKernel("HashData");
            _clearOffsetKernelID = _computeShader.FindKernel("ClearCellOffsets");
            _calculateOffsetKernelID = _computeShader.FindKernel("CalculateCellOffsets");
            _bitonicSortKernelID = _computeShader.FindKernel("BitonicSort");
            _computeShader.SetFloat("_SearchGridCellSize", _cellSize);

            InitHashGridBuffers();
            InitDynamicHashGrid();
        }

        public void DispatchKernel(ComputeShader compute, int kernel, int sizeX)
        {
            uint x, y, z;
            compute.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            Vector3Int vector3Int = new Vector3Int((int)x, (int)y, (int)z);
            int numGroupsX = Mathf.CeilToInt(sizeX / (float)vector3Int.x);
            compute.Dispatch(kernel, numGroupsX, 1, 1);
        }

        public virtual void ReleaseBuffers()
        {
            if (IndexBuffer != null)
                IndexBuffer.Release();
            if (CellIndexBuffer != null)
                CellIndexBuffer.Release();
            if (CellOffsetBuffer != null)
                CellOffsetBuffer.Release();
        }

        public void ClearOffsetBuffer()
        {
            DispatchKernel(_computeShader, _clearOffsetKernelID, _count);
        }

        public void Sort()
        {
            for (var k = 2; k <= _count; k <<= 1)
            {
                _computeShader.SetInt("k", k);
                for (var j = k >> 1; j > 0; j >>= 1)
                {
                    _computeShader.SetInt("j", j);
                    DispatchKernel(_computeShader, _bitonicSortKernelID, _count);
                }
            }
        }

        public void CalculateOffsets()
        {
            DispatchKernel(_computeShader, _calculateOffsetKernelID, _count);
        }

        private void InitHashGridBuffers()
        {
            int BUFFER_SIZE = System.Runtime.InteropServices.Marshal.SizeOf(typeof(uint));
            IndexBuffer = new ComputeBuffer((int)_count, BUFFER_SIZE);
            CellIndexBuffer = new ComputeBuffer((int)_count, BUFFER_SIZE);
            CellOffsetBuffer = new ComputeBuffer((int)_count, BUFFER_SIZE);

            uint[] indexArray = new uint[_count];
            for (uint i = 0; i < indexArray.Length; i++) indexArray[i] = i;
            IndexBuffer.SetData(indexArray);
        }

        private void InitDynamicHashGrid()
        {
            _computeShader.SetFloat("_SearchGridCellSize", _cellSize);
            _computeShader.SetInt("_ParticleBufferCount", _count);
            _computeShader.SetBuffer(_hashDataKernelID, "_ParticleBuffer", _particleBuffer);
            _computeShader.SetBuffer(_hashDataKernelID, "_IndexBuffer", IndexBuffer);
            _computeShader.SetBuffer(_hashDataKernelID, "_CellIndexBuffer", CellIndexBuffer);
            _computeShader.SetBuffer(_hashDataKernelID, "_OffsetsBuffer", CellOffsetBuffer);

            _computeShader.SetBuffer(_calculateOffsetKernelID, "_IndexBuffer", IndexBuffer);
            _computeShader.SetBuffer(_calculateOffsetKernelID, "_CellIndexBuffer", CellIndexBuffer);
            _computeShader.SetBuffer(_calculateOffsetKernelID, "_OffsetsBuffer", CellOffsetBuffer);
            
            _computeShader.SetBuffer(_bitonicSortKernelID, "_IndexBuffer", IndexBuffer);
            _computeShader.SetBuffer(_bitonicSortKernelID, "_CellIndexBuffer", CellIndexBuffer);

            _computeShader.SetBuffer(_clearOffsetKernelID, "_OffsetsBuffer", CellOffsetBuffer);
        }
    }
}