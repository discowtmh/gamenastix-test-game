﻿using UnityEngine;
using VRTK;
using VRTK.Examples;

public class RealGun_Net : VRTK_InteractableObject
{
	[Space]
	public float bulletSpeed = 200f;
	public float bulletForce;
	public float bulletLife = 5f;

	[SerializeField] private AudioClip shot;
	[SerializeField] private GameObject bullet_prefab;
	[SerializeField] private AudioSource audi;
	[SerializeField] private GameObject trigger;
	[SerializeField] private RealGun_Slide_Net slide;
	[SerializeField] private RealGun_SafetySwitch_Net safetySwitch;

	[SerializeField] private Rigidbody slideRigidbody;
	[SerializeField] private Collider slideCollider;
	[SerializeField] private Rigidbody safetySwitchRigidbody;
	[SerializeField] private Collider safetySwitchCollider;

	private VRTK_ControllerEvents controllerEvents;

	private float minTriggerRotation = -10f;
	private float maxTriggerRotation = 45f;

	[SerializeField]
	Transform shootFromHere;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();

		if (controllerEvents)
		{
			var pressure = (maxTriggerRotation * controllerEvents.GetTriggerAxis()) - minTriggerRotation;
			trigger.transform.localEulerAngles = new Vector3(0f, pressure, 0f);
		}
		else
		{
			trigger.transform.localEulerAngles = new Vector3(0f, minTriggerRotation, 0f);
		}
	}

	private void ToggleCollision(Rigidbody objRB, Collider objCol, bool state)
	{
		objRB.isKinematic = state;
		objCol.isTrigger = state;
	}

	private void ToggleSlide(bool state)
	{
		if (!state)
		{
			slide.ForceStopInteracting();
		}
		slide.enabled = state;
		slide.isGrabbable = state;
		ToggleCollision(slideRigidbody, slideCollider, state);
	}

	private void ToggleSafetySwitch(bool state)
	{
		if (!state)
		{
			safetySwitch.ForceStopInteracting();
		}
		ToggleCollision(safetySwitchRigidbody, safetySwitchCollider, state);
	}

	public override void Grabbed(VRTK_InteractGrab currentGrabbingObject)
	{
		base.Grabbed(currentGrabbingObject);

		controllerEvents = currentGrabbingObject.GetComponent<VRTK_ControllerEvents>();

		ToggleSlide(true);
		ToggleSafetySwitch(true);

		//Limit hands grabbing when picked up
		if (VRTK_DeviceFinder.GetControllerHand(currentGrabbingObject.controllerEvents.gameObject) == SDK_BaseController.ControllerHand.Left)
		{
			allowedTouchControllers = AllowedController.LeftOnly;
			allowedUseControllers = AllowedController.LeftOnly;
			slide.allowedGrabControllers = AllowedController.RightOnly;
			safetySwitch.allowedGrabControllers = AllowedController.RightOnly;
		}
		else if (VRTK_DeviceFinder.GetControllerHand(currentGrabbingObject.controllerEvents.gameObject) == SDK_BaseController.ControllerHand.Right)
		{
			allowedTouchControllers = AllowedController.RightOnly;
			allowedUseControllers = AllowedController.RightOnly;
			slide.allowedGrabControllers = AllowedController.LeftOnly;
			safetySwitch.allowedGrabControllers = AllowedController.LeftOnly;
		}
	}

	public override void Ungrabbed(VRTK_InteractGrab previousGrabbingObject)
	{
		base.Ungrabbed(previousGrabbingObject);

		ToggleSlide(false);
		ToggleSafetySwitch(false);

		//Unlimit hands
		allowedTouchControllers = AllowedController.Both;
		allowedUseControllers = AllowedController.Both;
		slide.allowedGrabControllers = AllowedController.Both;
		safetySwitch.allowedGrabControllers = AllowedController.Both;

		controllerEvents = null;
	}

	public override void StartUsing(VRTK_InteractUse currentUsingObject)
	{
		base.StartUsing(currentUsingObject);
		if (safetySwitch.safetyOff)
		{
			if (slide.Fire())
			{
				FireBullet();
			}
			else
			{
				audi.Play();
			}

			VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(controllerEvents.gameObject), 0.63f, 0.2f, 0.01f);
		}
		else
		{
			VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(controllerEvents.gameObject), 0.08f, 0.1f, 0.01f);
		}
	}

	private void FireBullet()
	{
		GameObject bullet = null;
			bullet = Instantiate(bullet_prefab, shootFromHere.position, shootFromHere.rotation, null);

		bullet.GetComponent<BulletScript_Net>().SetIsHitEnabled(true);
		bullet.GetComponent<Rigidbody>().velocity = (shootFromHere.forward * bulletForce);
	}
}