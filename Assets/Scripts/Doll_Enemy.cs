using System.Collections;
using UnityEngine;

public class Doll_Enemy : Enemy
{
	public override void Die()
	{
		Collider[] bodyColliders = GetComponentsInChildren<Collider>();

		foreach (Collider col in bodyColliders)
		{
			Rigidbody rb = col.GetComponent<Rigidbody>();
			rb.isKinematic = false;
			Destroy(rb, 3);
			StartCoroutine(TurnOffCollider(col, 3.2f));
		}
	}

	private IEnumerator TurnOffCollider(Collider collider, float delay)
	{
		yield return new WaitForSeconds(delay);

		Destroy(collider);
	}
}