using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[SerializeField] protected int hp;

	public void GetHit(int damage, bool headshot)
	{
		if (hp <= 0) return;

		hp -= (headshot ? 2 : 1) * damage;

		if (hp <= 0) Die();
	}

	public virtual void Die()
	{
		Destroy(gameObject);
	}
}