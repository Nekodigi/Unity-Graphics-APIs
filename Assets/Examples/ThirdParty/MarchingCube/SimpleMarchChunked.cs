using System;
using Common.DebugUtils;
using UnityEngine;

public class SimpleMarchChunked : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Vector3Int numPointsPerAxis = new Vector3Int(12, 24, 6);
    [Range(2, 100)] public int chunkResolution = 4;
    ChunkVisualizer[] _chunkVisualizers;
    private ComputeShader _simpleValuesCs;

    void Start()
    {
        DestroyChildren(); //It shoudn't be needed.
        _simpleValuesCs = (ComputeShader)Resources.Load("SimpleValues");
        _chunkVisualizers = new ChunkVisualizer[numPointsPerAxis.x * numPointsPerAxis.y * numPointsPerAxis.z];
        var chunkPerAxis = new Vector3Int(Mathf.CeilToInt(numPointsPerAxis.x / (float)chunkResolution),
            Mathf.CeilToInt(numPointsPerAxis.y / (float)chunkResolution),
            Mathf.CeilToInt(numPointsPerAxis.z / (float)chunkResolution));
        for (int i = 0; i < chunkPerAxis.x; i++)
        {
            for (int j = 0; j < chunkPerAxis.y; j++)
            {
                for (int k = 0; k < chunkPerAxis.z; k++)
                {
                    ComputeBuffer valuesBuffer = new ComputeBuffer(chunkResolution * chunkResolution * chunkResolution,
                        sizeof(float));
                    // var numPoints = chunkResolution * chunkResolution * chunkResolution;
                    // float[] values = new float[numPoints];
                    // for (int n = 0; n < numPoints; n++)
                    // {
                    //     Vector3 pos = new Vector3(n % chunkResolution, (n / chunkResolution) % chunkResolution,
                    //         n / (chunkResolution * chunkResolution));
                    //     float value = Mathf.Sin(pos.x * 0.5f + Time.time) * Mathf.Sin(pos.y * 0.5f) *
                    //                   Mathf.Sin(pos.z * 0.5f) -
                    //                   0.2f;
                    //     values[n] = value;
                    // }
                    //
                    // valuesBuffer.SetData(values);
                    _simpleValuesCs.SetFloat("_Time", Time.time);
                    _simpleValuesCs.SetBuffer(0, "_ValuesBuffer", valuesBuffer);
                    _simpleValuesCs.SetInt("_Resolution", chunkResolution);
                    _simpleValuesCs.Dispatch(0, chunkResolution, chunkResolution, chunkResolution);

                    var chunk = Instantiate(chunkPrefab, transform);
                    //translate
                    chunk.transform.localPosition = new Vector3(i * chunkResolution, j * chunkResolution,
                        k * chunkResolution);
                    var chunkVisualizer = chunk.GetComponent<ChunkVisualizer>();
                    if (chunkVisualizer == null)
                    {
                        Debug.LogError("Chunk prefab does not have a ChunkVisualizer component.");
                        return;
                    }

                    chunkVisualizer.Init(chunkResolution, 0f, valuesBuffer);
                    _chunkVisualizers[i * chunkPerAxis.y * chunkPerAxis.z + j * chunkPerAxis.z + k] = chunkVisualizer;
                }
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < _chunkVisualizers.Length; i++)
        {
            //nullcheck
            if (_chunkVisualizers[i] == null || _chunkVisualizers[i].ValuesBuffer == null)
            {
                continue;
            }

            _simpleValuesCs.SetFloat("_Time", Time.time);
            _simpleValuesCs.SetBuffer(0, "_ValuesBuffer", _chunkVisualizers[i].ValuesBuffer);
            _simpleValuesCs.SetInt("_Resolution", chunkResolution);
            _simpleValuesCs.Dispatch(0, chunkResolution, chunkResolution, chunkResolution);
            // var numPoints = chunkResolution * chunkResolution * chunkResolution;
            // float[] values = new float[numPoints];
            // for (int n = 0; n < numPoints; n++)
            // {
            //     Vector3 pos = new Vector3(n % chunkResolution, (n / chunkResolution) % chunkResolution,
            //         n / (chunkResolution * chunkResolution));
            //     float value = Mathf.Sin(pos.x * 0.5f + Time.time) * Mathf.Sin(pos.y * 0.5f) *
            //                   Mathf.Sin(pos.z * 0.5f) -
            //                   0.2f;
            //     values[n] = value;
            // }
            //
            // _chunkVisualizers[i].ValuesBuffer.SetData(values);
        }
    }

    //It didn't work
    private void OnDestroy()
    {
        DestroyChildren();
    }


    private void DestroyChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject); //TODO Immidiate is not best way.
        }
    }

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }
}