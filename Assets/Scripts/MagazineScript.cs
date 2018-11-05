using UnityEngine;
using System.Collections;

namespace VRTK.Examples
{
    public class MagazineScript : VRTK_InteractableObject
    {
        public GameObject pistol;
		private AudioSource magAudio;
		public AudioClip magClipIn;
		[SerializeField]
		private int ammoAmount;

        void Start()
        {
			magAudio = this.GetComponent<AudioSource>();
        }

        public void OnTriggerEnter(Collider other)
        {
			if (other.GetComponentInChildren<RealGun_Slide>())
            {
				other.transform.GetChild (0).GetComponent<RealGun_Slide_Net>().FullAmo();
				StartCoroutine (PlayAudioAndWait(magAudio));
            }
        }

		private IEnumerator PlayAudioAndWait(AudioSource source){
			source.PlayOneShot(magClipIn, 1f);
			yield return new WaitForSeconds (1f);
			Destroy(gameObject);
		}

		public int GetAmmoAmount(){
			return ammoAmount;
		}

		public void SetAmmoAmount(int value){
			ammoAmount = value;
		}
    }
}
