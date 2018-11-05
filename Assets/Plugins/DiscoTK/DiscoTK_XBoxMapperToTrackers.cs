using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiscoTK
{
    public class DiscoTK_XBoxMapperToTrackers : MonoBehaviour
    {

        // Axes
        // Horizontal X
        // Vertical Z
        public Transform leftFoot;
        public Transform rightFoot;
        // footHorizontalOffsetFromCenterAxisInMetres is only for Debugging Offline mode
        public float footHorizontalOffsetFromCenterAxisInMetres = 0.2f;
        public float stepMaxLenghtInMetres = 0.6f;
        private float XBoxLHorizontal, XBoxRHorizontal;
        private float XBoxLVertical, XBoxRVertical;
        public bool isDebugOfflineEnabled = true;


        void Update()
        {
            XBoxLHorizontal = Input.GetAxis("Left Stick Horizontal");
            XBoxLVertical = -Input.GetAxis("Left Stick Vertical");
            XBoxRHorizontal = Input.GetAxis("Right Stick Horizontal");
            XBoxRVertical = -Input.GetAxis("Right Stick Vertical");

            if (isDebugOfflineEnabled)
            {
                leftFoot.localPosition = new Vector3(XBoxLHorizontal * (stepMaxLenghtInMetres - footHorizontalOffsetFromCenterAxisInMetres) - footHorizontalOffsetFromCenterAxisInMetres, 0.5f * stepMaxLenghtInMetres * Mathf.Sin(XBoxLVertical * Mathf.PI), XBoxLVertical * stepMaxLenghtInMetres);
                rightFoot.localPosition = new Vector3(XBoxRHorizontal * (stepMaxLenghtInMetres - footHorizontalOffsetFromCenterAxisInMetres) + footHorizontalOffsetFromCenterAxisInMetres, 0.5f * stepMaxLenghtInMetres * Mathf.Sin(XBoxRVertical * Mathf.PI), XBoxRVertical * stepMaxLenghtInMetres);
            }
            else
            {
                leftFoot.localPosition = new Vector3(XBoxLHorizontal * stepMaxLenghtInMetres, 0.5f * stepMaxLenghtInMetres * Mathf.Sin(XBoxLVertical * Mathf.PI), XBoxLVertical * stepMaxLenghtInMetres);
                rightFoot.localPosition = new Vector3(XBoxRHorizontal * stepMaxLenghtInMetres, 0.5f * stepMaxLenghtInMetres * Mathf.Sin(XBoxRVertical * Mathf.PI), XBoxRVertical * stepMaxLenghtInMetres);
            }
        }
    }
}
