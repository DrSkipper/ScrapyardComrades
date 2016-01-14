using UnityEngine;
using System.Collections;

//TODO - Uncomment refererences to CameraController
public class PerlinShake : MonoBehaviour
{
	public float Duration = 0.5f;
	public float Speed = 1.0f;
	public float Magnitude = 0.1f;
	
    void Start()
    {
        //_cameraController = this.GetComponent<CameraController>();
    }

	public void PlayShake(float duration = 0.5f, float speed = 1.0f, float magnitude = 0.1f)
    {
		StopAllCoroutines();
        this.Duration = duration;
        this.Speed = speed;
        this.Magnitude = magnitude;
		StartCoroutine("Shake");
	}
	
	IEnumerator Shake()
    {
		float elapsed = 0.0f;
		float randomStart = Random.Range(-1000.0f, 1000.0f);
		
		while (elapsed < this.Duration) {
			
			elapsed += Time.deltaTime;			
			
			float percentComplete = elapsed / this.Duration;			
			
			// We want to reduce the shake from full power to 0 starting half way through
			float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);
			
			// Calculate the noise parameter starting randomly and going as fast as speed allows
			float alpha = randomStart + this.Speed * percentComplete;
			
			// map noise to [-1, 1]
			//float x = Util.Noise.GetNoise(alpha, 0.0f, 0.0f) * 2.0f - 1.0f;
			//float y = Util.Noise.GetNoise(0.0f, alpha, 0.0f) * 2.0f - 1.0f;
            float x = Mathf.PerlinNoise(alpha, 0.0f) * 2.0f - 1.0f;
            float y = Mathf.PerlinNoise(0.0f, alpha) * 2.0f - 1.0f;

            x *= this.Magnitude * damper;
			y *= this.Magnitude * damper;
			
			//_cameraController.OffsetPosition = new Vector2(x, y);
			
			yield return null;
		}

        //_cameraController.OffsetPosition = Vector2.zero;
    }

    /**
     * Private
     */
    //private CameraController _cameraController;
}
