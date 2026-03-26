using UnityEngine;

// Spawnat la disparitia obstacolului lovit in Rush; creeaza o explozie de particule si se auto-distruge
public class ObstacleBreakVFX : MonoBehaviour
{
    [SerializeField] private Material particleMaterial;

    private void Awake()
    {
        // Adauga ParticleSystem la runtime (zero YAML complex; scriptul configureaza totul)
        var ps = gameObject.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.55f, 0.1f, 1f),
            new Color(1f, 0.9f, 0.2f, 1f)
        );
        main.maxParticles = 20;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.7f, 0.1f), 0f),
                new GradientColorKey(new Color(0.9f, 0.3f, 0.05f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));

        if (particleMaterial != null)
            ps.GetComponent<ParticleSystemRenderer>().material = particleMaterial;

        ps.Play();
    }
}
