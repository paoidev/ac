using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles

{
	public class opencloseDoor1 : MonoBehaviour, IInteractable
    {

		public Animator openandclose1;
		public bool open;
		public Transform Player;
        public GameObject fridgeText;

        void Start()
		{
			open = false;
		}


		IEnumerator opening()
		{
			print("you are opening the door");
			openandclose1.Play("Opening 1");
			open = true;
            yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			openandclose1.Play("Closing 1");
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
            return "Open the fridge";
        }
    }
}