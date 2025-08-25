using UnityEngine;

public class ParticleForcePlayTime : MonoBehaviour
{
    [SerializeField] private ParticleSystem fx;
    [SerializeField] private float timeForce;
    
    private void OnEnable()
    {
        fx.Simulate(timeForce, true, true, false);
    }
}