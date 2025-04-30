#include "Assets/APIs/Particles/DIParticle.cginc"
#include "Assets/APIs/Particles/Particle.cginc"


class Particle : IParticle
{
    SParticle p;
    AppendStructuredBuffer<uint> _deadParticleBuffer;

    //initializer
    void set_particle(SParticle _p)
    {
        p = _p;
    }

    void init(uint id, float3 position, float3 velocity, float duration)
    {
        p.particleId = id;
        p.position = position;
        p.velocity = velocity;
        p.lifetime = duration;
        p.age = 0;
        p.color = random3(id);
        p.alive = true;
    }

    void update(float deltaTime)
    {
        if (p.age < p.lifetime)
        {
            p.position += p.velocity * deltaTime;
            p.age += deltaTime;


            if (p.age >= p.lifetime)
            {
                p.alive = false;
                _DeadParticleBuffer.Append(p.particleId);
            }
        }
    }
};
