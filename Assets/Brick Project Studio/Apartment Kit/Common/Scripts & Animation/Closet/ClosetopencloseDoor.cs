using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles

{
	public class ClosetopencloseDoor : MonoBehaviour, IInteractable
    {

		public Animator Closetopenandclose;
		public bool open;
		public Transform Player;

		void Start()
		{
			open = false;
		}

		IEnumerator opening()
		{
			print("you are opening the door");
			Closetopenandclose.Play("Opening");
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			Closetopenandclose.Play("Closing");
			open = false;
			yield return new WaitForSeconds(.5f);
		}

        public void Interact()
        {
            if (!Player) return;
            float dist = Vector3.Distance(Player.position, transform.position);
            if (dist >= 15) return;

            StartCoroutine(open ? closing() : opening());
        }

        public string GetDescription()
        {
            return "Open the door";
        }

    }
}