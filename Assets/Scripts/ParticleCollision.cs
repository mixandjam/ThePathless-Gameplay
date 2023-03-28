using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    [SerializeField] Renderer[] characterRenderers;
    [SerializeField] ParticleSystem characterParticle;
    [SerializeField] ArrowSystem arrowSystem;
    [SerializeField] bool isEnergy;
    int amount = 0;

    private void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void Update()
    {
        if (isEnergy)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[part.particleCount];
            part.GetParticles(particles);

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].remainingLifetime > 0f && Vector3.Distance(particles[i].position, transform.position) < 1f)
                {
                    particles[i].remainingLifetime = 0f;
                    particles[i].startSize = 0;

                    if (amount < 1)
                    {
                        amount++;

                        foreach (Renderer renderer in characterRenderers)
                        {
                            renderer.material.DOFloat(1, "_Alpha", .2f).OnComplete(() => Complete(renderer));
                        }

                        characterParticle.Play();
                    }

                }
            }

            part.SetParticles(particles, particles.Length);

        }
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.layer == 7)
            return;

        if (isEnergy)
            return;

        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        arrowSystem.TargetHit(collisionEvents[0].velocity);
    }

    void Complete(Renderer renderer)
    {
        renderer.material.DOFloat(0, "_Alpha", .3f).OnComplete(() => amount = 0);
    }

}
