using System;
using UnityEngine;

namespace APIs.Particles
{
    [Serializable]
    public struct ParticleBase
    {
        //basic simulation
        public Vector3 position;
        public Vector3 velocity;
        public float age;
        public float lifetime;
        public int alive;
        public float size;

        //advanced simulation
        public float mass;
        public Vector3 direction;
        public Vector3 angle;
        public Vector3 angularVelocity;
        public Vector3 oldPosition;
        public Vector3 targetPosition;

        //rendering
        public float alpha;
        public Vector3 color;
        public float renderingSize;
        public Vector3 scale;
        public Vector3 pivot;
        public float texIndex;
        public Vector3 axisX;
        public Vector3 axisY;
        public Vector3 axisZ;

        //system
        public uint particleId;
        public uint seed;
        public float spawnCount;
        public float spawnTime;
        public uint particleIndexInStrip;
    }
}