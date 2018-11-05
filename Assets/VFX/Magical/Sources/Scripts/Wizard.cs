using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MagicalFX
{
	public class Wizard : MonoBehaviour
	{
		public GameObject[] Skills;
		public int Index;
		public AudioClip spellAudioClip;
		public bool Showtime;
		public float Delay = 1;
		public float RandomSize = 10;
		public bool RandomSkill = false;

		private Vector3 positionLook;
		private float timeTemp;
	
		public GameObject shotSpawnPoint;

		private bool trigger;
		private FX_Position fx;
		private AudioSource magicAudio;

		Ray shootRay;

		void Start (){
			timeTemp = Time.time;
			fx = Skills [Index].GetComponent<FX_Position> ();
			magicAudio = GetComponent<AudioSource> ();
		}

		void Update (){
//			if fire on the controller or on the mouse
			Debug.DrawRay (shotSpawnPoint.transform.position, shotSpawnPoint.transform.forward, Color.green);

			if (Input.GetButtonDown ("Fire1")) {	
				Aim ();
				Deploy ();
			} 
		}

		void TestShoot(){
			Aim ();
			Deploy ();
		}

		void Deploy (){
			if (Index >= Skills.Length || Index < 0)
				Index = 0;

			if (fx) {
				if (fx.Mode == SpawnMode.Static) {
					Place (Skills [Index]);	
				}
				if (fx.Mode == SpawnMode.OnDirection) {
					PlaceDirection (Skills [Index]);	
				}	
				
			} else {
				Shoot (Skills [Index]);	
			}
		}
	
		void Aim (){
			int layerMask = 1 << 11;
			shootRay.origin = shotSpawnPoint.transform.position;
			shootRay.direction = shotSpawnPoint.transform.forward;

			RaycastHit hit;

			if (Physics.Raycast (shootRay, out hit, 800, layerMask)) {
				Debug.Log ("Shot: " + hit.collider.name);
				positionLook = hit.point;
			}
		}
	
		void Shoot (GameObject skill){
			GameObject sk = (GameObject)GameObject.Instantiate (skill, shotSpawnPoint.transform.position + (Vector3.up * 0.5f) + shotSpawnPoint.transform.forward, skill.transform.rotation);
			sk.transform.forward = (positionLook - shotSpawnPoint.transform.position).normalized;
			magicAudio.PlayOneShot (spellAudioClip);
			Debug.Log ("Shoot");
			//GameObject.Destroy (sk, 3);
		}

		void Place (GameObject skill){
			GameObject sk = (GameObject)GameObject.Instantiate (skill, positionLook, skill.transform.rotation);
			Debug.Log ("Place");
			//GameObject.Destroy (sk, 3);
		}

		void PlaceDirection (GameObject skill){
			GameObject sk = (GameObject)GameObject.Instantiate (skill, shotSpawnPoint.transform.position + shotSpawnPoint.transform.forward, skill.transform.rotation);
			FX_Position fx = sk.GetComponent<FX_Position> ();
			if (fx.Mode == SpawnMode.OnDirection)
				fx.transform.forward = shotSpawnPoint.transform.forward;
			magicAudio.PlayOneShot (spellAudioClip);
			Debug.Log ("PlaceDirection");
			//GameObject.Destroy (sk, 3);
		}
	
	}
}
