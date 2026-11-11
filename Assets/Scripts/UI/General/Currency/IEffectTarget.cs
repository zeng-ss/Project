using UnityEngine;

public interface IEffectTarget
{
    Transform GetTransform();

    void PlayParticle();
}
