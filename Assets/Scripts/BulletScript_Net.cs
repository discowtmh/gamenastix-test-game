using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BulletScript_Net : MonoBehaviour
{
	[SerializeField]
	protected AudioSource audioSource;
	[SerializeField]
	protected AudioClip[] start_clips;
	[SerializeField]
	protected AudioClip[] hit_clips;
	[SerializeField]
	protected float lifetime;
	[SerializeField]
	protected bool SendRbVelocity = false;

	public int m_HitDamage = 20;
	public bool isHitEnabled = false;
	public GameObject[] bloodPrefabArr;
	protected GameObject bloodInstance;

	float dyingTimer;

	void Start()
	{
		audioSource.clip = start_clips[Random.Range(0, start_clips.Length)];
		audioSource.Play();
		Destroy(gameObject, lifetime);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (isHitEnabled)
		{
			if (collision.gameObject.tag == "Enemy")
			{
				isHitEnabled = false;

				collision.collider.GetComponentInParent<Enemy>().GetHit(20, collision.gameObject.name.Contains("head"));

				ContactPoint contact = collision.contacts[0];
				Vector3 pos = contact.point;

				if (bloodPrefabArr.Length > 0)
				{
					bloodInstance = Instantiate(bloodPrefabArr[Random.Range(0, bloodPrefabArr.Length)], pos, new Quaternion()) as GameObject;
				}

				Destroy(bloodInstance, 4.0f);

				audioSource.transform.parent = null;
				Destroy(audioSource.gameObject, 1);

				StartCoroutine(SlowlyDie());
			}
		}
	}

	public void SetIsHitEnabled(bool b)
	{
		isHitEnabled = b;
	}

	IEnumerator SlowlyDie()
	{
		while (transform.localScale.x > 0.12f)
		{
			float dieOver = 0.5f;

			dyingTimer += Time.deltaTime / dieOver;

			transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, dyingTimer);

			yield return 0;
		}

		Destroy(gameObject);
	}
}