using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRTK.Examples;

public class MagazineStateDisplay : MonoBehaviour
{
	[SerializeField]
	Color red;
	[SerializeField]
	Color green;
	[SerializeField]
	Color yellow;
	[SerializeField]
	Text ammo_text;
	[SerializeField]
	Image ammoIcon_image;

	Color standardColor;
	Coroutine currentCoroutine;

	private void Start()
	{
		RealGun_Slide_Net.inst.AmmoNumberChanged += RealGun_Slide_AmmoNumberChanged;
		standardColor = ammo_text.color;
	}

	private void Update()
	{
		transform.LookAt(Camera.main.transform);

		ammo_text.text = RealGun_Slide_Net.inst.curr.ToString();
	}

	private void RealGun_Slide_AmmoNumberChanged(int newNumber)
	{
		ammo_text.text = newNumber.ToString();

		if (newNumber == 0)
		{
			if(currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
			}

			currentCoroutine = StartCoroutine(Lighten(red));
		}
		else if(newNumber == RealGun_Slide_Net.inst.curr)
		{
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
			}

			currentCoroutine = StartCoroutine(Lighten(green));
		}
		else
		{
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
			}

			currentCoroutine = StartCoroutine(Lighten(yellow));
		}
	}

	private IEnumerator Lighten(Color c)
	{
		ammo_text.color = c;
		ammoIcon_image.color = c;

		while (ammo_text.color != standardColor)
		{
			ammo_text.color = Color.Lerp(ammo_text.color, standardColor, 2 * Time.deltaTime);
			ammoIcon_image.color = Color.Lerp(ammoIcon_image.color, standardColor, 2 * Time.deltaTime);

			yield return 0;
		}
	}
}