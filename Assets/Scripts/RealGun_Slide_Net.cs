namespace VRTK.Examples
{
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.Serialization;

	public class RealGun_Slide_Net : VRTK_InteractableObject
	{
		public static RealGun_Slide_Net inst;

		public delegate void ChangingAmmoNumber(int newNumber);
		public event ChangingAmmoNumber AmmoNumberChanged;

		private float restPosition;
		private float fireTimer = 0f;
		private float fireDistance = 0.05f;
		private float boltSpeed = 0.3f;
		public int curr = 16;
		[FormerlySerializedAs("maxAmmo")]
		public int limit;

		protected override void Awake()
		{
			base.Awake();

			if(inst == null)
			{
				inst = this;
			}
			else
			{
				Destroy(gameObject);
			}

			restPosition = transform.localPosition.z;
		}

		private void Start()
		{
			limit = 32;

			if (SceneManager.GetActiveScene().name.Contains("Tutorial"))
			{
				curr = 0;

				return;
			}

			curr = limit;
		}

		protected override void Update()
		{
			base.Update();
			if (transform.localPosition.z >= restPosition)
			{
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, restPosition);
			}

			if (fireTimer == 0 && transform.localPosition.z < restPosition && !IsGrabbed() && curr > 1)
			{
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + boltSpeed);
			}

			if (fireTimer > 0 && curr > 0)
			{
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - boltSpeed);
				fireTimer -= boltSpeed;
			}

			if (fireTimer < 0)
			{
				fireTimer = 0;
			}
		}

		public bool Fire()
		{
			bool didFire = false;

			if (curr > 0)
			{
				fireTimer = fireDistance;
				//curr--;

				didFire = true;
			}

			AmmoNumberChanged(curr);

			return didFire;
		}

		public void FullAmo()
		{
			curr = limit;

			AmmoNumberChanged(curr);
		}
	}
}
