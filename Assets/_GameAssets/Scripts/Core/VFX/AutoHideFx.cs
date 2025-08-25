using UnityEngine;
using System.Collections;
// using AssetKits.ParticleImage;
// using AssetKits.ParticleImage.Enumerations;

// Cartoon FX  - (c) 2015 Jean Moreno

// Automatically destructs an object when it has stopped emitting particles and when they have all disappeared from the screen.
// Check is performed every 0.5 seconds to not query the particle system's state every frame.
// (only deactivates the object if the OnlyDeactivate flag is set, automatically used with CFX Spawn System)

public class AutoHideFx : MonoBehaviour
{
	// If true, deactivate the object instead of destroying it
	public bool OnlyDeactivate;
	public float time;
	private Coroutine coroutine;
	protected ParticleSystem ps;
	// protected ParticleImage pi;
	
	protected virtual async void OnEnable()
	{
		// ps = this.GetComponentInChildren<ParticleSystem>();
		// if (!ps)
		// {
		// 	pi = this.GetComponentInChildren<ParticleImage>();
		// 	await UniTask.Delay((int) (pi.lifetime.constantMax * 1000),
		// 		pi.timeScale == TimeScale.Unscaled ?
		// 		DelayType.UnscaledDeltaTime : DelayType.DeltaTime);
		// 	OnDisable();
		// }
		// else
			coroutine = StartCoroutine("CheckIfAlive");
	}

	private void OnDisable()
	{
		if(OnlyDeactivate)
		{
			EffectController.HideEffect(name, transform);
		}
		else
			GameObject.Destroy(this.gameObject);
	}

	IEnumerator CheckIfAlive ()
	{
		if (time > 0)
		{
			yield return new WaitForSeconds(time);
			OnDisable();
			yield break;
		}
		
		while(true && ps != null)
		{
			yield return new WaitForSeconds(0.5f);
			if(!ps.IsAlive(true))
			{
				OnDisable();
				break;
			}
		}
	}
}
