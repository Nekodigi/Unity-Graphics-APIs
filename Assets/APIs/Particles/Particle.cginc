#ifndef PARTICLE_INCLUDED
#define PARTICLE_INCLUDED

#include "DIParticle.cginc"

RWStructuredBuffer<SParticle> _ParticleBuffer;
ConsumeStructuredBuffer<uint> _PooledParticleBuffer;
AppendStructuredBuffer<uint> _DeadParticleBuffer;
uint _ParticleBufferCount;

interface IParticle
{
    void set_particle(SParticle p);

    void update(float deltaTime);
};

#endif
