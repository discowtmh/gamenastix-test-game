using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace DiscoTK
{
    public class DiscoTK_MoveInPlace : VRTK_MoveInPlace
    {
        [Header("Trackers Settings")]
        [Tooltip("Left Foot Tracker, Right Foot Tracker, Pelvis Tracker")]
		[SerializeField] private Transform trackerLeftFoot;
		[SerializeField] private Transform trackerRightFoot;
		[SerializeField] private GameObject trackerPelvis;

        [Tooltip("Check if your pelvis tracker is mounted on the back.")]
		[SerializeField] private bool isBackpackPelvisTracker;
        [Tooltip("Check if you want to move backwards also.")]
		[SerializeField] private bool isBackwardMovementEnabled;

        private Vector3 userUpVector;
        private int forwardMovementSign;

        /// <summary>
        /// Options for testing if a play space fall is valid.
        /// </summary>
        /// <param name="HeadsetAndControllers">Track both headset and controllers for movement calculations.</param>
        /// <param name="ControllersOnly">Track only the controllers for movement calculations.</param>
        /// <param name="HeadsetOnly">Track only headset for movement caluclations.</param>
        /// <param name="TrackersOnly">Track only headset for movement caluclations.</param>
        public enum MyControlOptions
        {
            HeadsetAndControllers,
            ControllersOnly,
            HeadsetOnly,
            TrackersOnly
        }

        /// <summary>
        /// Options for which method is used to determine player direction while moving.
        /// </summary>
        /// <param name="Gaze">Player will always move in the direction they are currently looking.</param>
        /// <param name="ControllerRotation">Player will move in the direction that the controllers are pointing (averaged).</param>
        /// <param name="DumbDecoupling">Player will move in the direction they were first looking when they engaged Move In Place.</param>
        /// <param name="SmartDecoupling">Player will move in the direction they are looking only if their headset point the same direction as their controllers.</param>
        /// <param name="EngageControllerRotationOnly">Player will move in the direction that the controller with the engage button pressed is pointing.</param>
        /// <param name="LeftControllerRotationOnly">Player will move in the direction that the left controller is pointing.</param>
        /// <param name="RightControllerRotationOnly">Player will move in the direction that the right controller is pointing.</param>
        /// <param name="PelvisTrackerRotation">Player will move in the direction that the pelvis tracker is pointing.</param>
        public enum MyDirectionalMethod
        {
            Gaze,
            ControllerRotation,
            DumbDecoupling,
            SmartDecoupling,
            EngageControllerRotationOnly,
            LeftControllerRotationOnly,
            RightControllerRotationOnly,
            PelvisTrackerRotation,
            OneConstantDirection
        }

        [Header("Trackers Control Settings")]
        [Tooltip("Select which trackables are used to determine movement.")]
		[SerializeField] private MyControlOptions myControlOptions = MyControlOptions.TrackersOnly;
        [Tooltip("How the user's movement direction will be determined.  The Gaze method tends to lead to the least motion sickness.  Smart decoupling is still a Work In Progress.")]
		[SerializeField] private MyDirectionalMethod myDirectionMethod = MyDirectionalMethod.PelvisTrackerRotation;

        [Header("Trackers Advanced Settings")]
		[SerializeField] private float sensitivityHorizontal = 0.001f;
        [Tooltip("Threshold for forward and backward movement. Used when isBackwardMovementEnabled is checked.")]
        [SerializeField] private float forwardBackwardThreshold = 0.1f;
        [Tooltip("Axis that triggers the movement.")]
		[SerializeField] private bool isAxisYTheTriggerAxis = true;

        protected Dictionary<Transform, float> previousTriggerAxisPositions;
        protected bool activeMovement;

        /// <summary>
        /// Set the control options and modify the trackables to match.
        /// </summary>
        /// <param name="givenControlOptions">The control options to set the current control options to.</param>
        public void SetControlOptions(MyControlOptions givenControlOptions)
        {
            myControlOptions = givenControlOptions;
            trackedObjects.Clear();

            if (controllerLeftHand != null && controllerRightHand != null && (myControlOptions.Equals(MyControlOptions.HeadsetAndControllers) || myControlOptions.Equals(MyControlOptions.ControllersOnly)))
            {
                trackedObjects.Add(VRTK_DeviceFinder.GetActualController(controllerLeftHand).transform);
                trackedObjects.Add(VRTK_DeviceFinder.GetActualController(controllerRightHand).transform);
            }

            if (headset != null && (myControlOptions.Equals(ControlOptions.HeadsetAndControllers) || myControlOptions.Equals(MyControlOptions.HeadsetOnly)))
            {
                trackedObjects.Add(headset.transform);
            }

            if (trackerLeftFoot != null && trackerRightFoot != null && trackerPelvis != null && (myControlOptions.Equals(MyControlOptions.TrackersOnly)))
            {
                trackedObjects.Add(trackerLeftFoot.transform);
                trackedObjects.Add(trackerRightFoot.transform);
            }
        }

		public void OnEnablePublic()
		{
			OnEnable();
		}

        protected override void OnEnable()
        {
            trackedObjects = new List<Transform>();
            movementList = new Dictionary<Transform, List<float>>();
            //base.previousYPositions = new Dictionary<Transform, float>();
            previousTriggerAxisPositions = new Dictionary<Transform, float>();
            initalGaze = Vector3.zero;
            direction = Vector3.zero;
            previousDirection = Vector3.zero;
            averagePeriod = 60;
            currentSpeed = 0f;
            active = false;

            activeMovement = false;
            previousEngageButton = engageButton;

            bodyPhysics = (bodyPhysics != null ? bodyPhysics : GetComponentInChildren<VRTK_BodyPhysics>());
            controllerLeftHand = VRTK_DeviceFinder.GetControllerLeftHand();
            controllerRightHand = VRTK_DeviceFinder.GetControllerRightHand();

            SetControllerListeners(controllerLeftHand, leftController, ref leftSubscribed);
            SetControllerListeners(controllerRightHand, rightController, ref rightSubscribed);

            headset = VRTK_DeviceFinder.HeadsetTransform();

            SetControlOptions(myControlOptions);

            playArea = VRTK_DeviceFinder.PlayAreaTransform();

            for (int i = 0; i < trackedObjects.Count; i++)
            {
                Transform trackedObj = trackedObjects[i];
                movementList.Add(trackedObj, new List<float>());
                if (isAxisYTheTriggerAxis)
                    previousTriggerAxisPositions.Add(trackedObj, trackedObj.transform.localPosition.y);
                else
                    previousTriggerAxisPositions.Add(trackedObj, trackedObj.transform.localPosition.z);
            }
            if (playArea == null)
            {
                VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.SDK_OBJECT_NOT_FOUND, "PlayArea", "Boundaries SDK"));
            }

            userUpVector = trackerPelvis.transform.up;
        }

        // Player Movement
        protected override void FixedUpdate()
        {
            HandleFalling();

            if ((CalculateListAverage() / trackedObjects.Count) > sensitivityHorizontal)
            {
                activeMovement = true;
            }
            else
            {
                activeMovement = false;
            }


            // If Move In Place is currently engaged.
            if (MovementActivated() && !currentlyFalling)
            {
                // Move speed includes only trackers
                float speed = Mathf.Clamp(((speedScale * 350) * (CalculateListAverage() / trackedObjects.Count)), 0f, maxSpeed);
                previousDirection = direction;
                direction = SetDirection();
                // Update our current speed.
                currentSpeed = speed;
            }
            else if (currentSpeed > 0f)
            {
                currentSpeed -= (currentlyFalling ? fallingDeceleration : deceleration);
            }
            else
            {
                currentSpeed = 0f;
                direction = Vector3.zero;
                previousDirection = Vector3.zero;
            }

            SetDeltaTransformData();


            if (isBackwardMovementEnabled)
            {
                if ((isBackpackPelvisTracker ? 1 : -1) * Vector3.Dot(userUpVector, trackerPelvis.transform.forward) > forwardBackwardThreshold)
                {
                    print("Going forward");
                    forwardMovementSign = 1;
                }
                else if ((isBackpackPelvisTracker ? 1 : -1) * Vector3.Dot(userUpVector, trackerPelvis.transform.forward) < -forwardBackwardThreshold)
                {
                    print("Going backwards");
                    forwardMovementSign = -1;
                }
            }
            else
            {
                forwardMovementSign = 1;
            }

            //MovePlayArea(-direction, currentSpeed);
            MovePlayArea(-1 * forwardMovementSign * (isBackpackPelvisTracker ? 1 : -1) * direction, currentSpeed);
        }

        // Movement activation
        protected override bool MovementActivated()
        {
            return (active || (engageButton == VRTK_ControllerEvents.ButtonAlias.Undefined && activeMovement));
        }

        protected override float CalculateListAverage()
        {
            float listAverage = 0;

            for (int i = 0; i < trackedObjects.Count; i++)
            {
                Transform trackedObj = trackedObjects[i];
                // Get the amount of Y movement that's occured since the last update.
                float deltaYPosition;
                if (isAxisYTheTriggerAxis)
                    deltaYPosition = Mathf.Abs(previousTriggerAxisPositions[trackedObj] - trackedObj.transform.localPosition.y);
                else
                    deltaYPosition = Mathf.Abs(previousTriggerAxisPositions[trackedObj] - trackedObj.transform.localPosition.z);

                // Convenience code.
                List<float> trackedObjList = movementList[trackedObj];

                // Cap off the speed.
                if (deltaYPosition > sensitivity)
                {
                    trackedObjList.Add(sensitivity);
                }
                else
                {
                    trackedObjList.Add(deltaYPosition);
                }

                // Keep our tracking list at m_averagePeriod number of elements.
                if (trackedObjList.Count > averagePeriod)
                {
                    trackedObjList.RemoveAt(0);
                }

                // Average out the current tracker's list.
                float sum = 0;
                for (int j = 0; j < trackedObjList.Count; j++)
                {
                    float diffrences = trackedObjList[j];
                    sum += diffrences;
                }
                float avg = sum / averagePeriod;

                // Add the average to the the list average.
                listAverage += avg;
            }

            return listAverage;
        }

        protected override Vector3 SetDirection()
        {
            Vector3 returnDirection = Vector3.zero;

            // if we're doing controller rotation movement
            if (myDirectionMethod.Equals(MyDirectionalMethod.ControllerRotation))
            {
                Vector3 calculatedControllerDirection = DetermineAverageControllerRotation() * Vector3.forward;
                returnDirection = CalculateControllerRotationDirection(calculatedControllerDirection);
            }
            // if we're doing left controller only rotation movement
            else if (myDirectionMethod.Equals(MyDirectionalMethod.LeftControllerRotationOnly))
            {
                Vector3 calculatedControllerDirection = (controllerLeftHand != null ? controllerLeftHand.transform.rotation : Quaternion.identity) * Vector3.forward;
                returnDirection = CalculateControllerRotationDirection(calculatedControllerDirection);
            }
            // if we're doing right controller only rotation movement
            else if (myDirectionMethod.Equals(MyDirectionalMethod.RightControllerRotationOnly))
            {
                Vector3 calculatedControllerDirection = (controllerRightHand != null ? controllerRightHand.transform.rotation : Quaternion.identity) * Vector3.forward;
                returnDirection = CalculateControllerRotationDirection(calculatedControllerDirection);
            }
            // if we're doing engaged controller only rotation movement
            else if (myDirectionMethod.Equals(MyDirectionalMethod.EngageControllerRotationOnly))
            {
                Vector3 calculatedControllerDirection = (engagedController != null ? engagedController.scriptAlias.transform.rotation : Quaternion.identity) * Vector3.forward;
                returnDirection = CalculateControllerRotationDirection(calculatedControllerDirection);
            }
            // Otherwise if we're just doing Gaze movement, always set the direction to where we're looking.
            else if (myDirectionMethod.Equals(MyDirectionalMethod.Gaze))
            {
                returnDirection = (new Vector3(headset.transform.forward.x, 0, headset.transform.forward.z));
            }
            // Pelvis rotation.
            else if (myDirectionMethod.Equals(MyDirectionalMethod.PelvisTrackerRotation))
            {
                returnDirection = (new Vector3(trackerPelvis.transform.forward.x, 0, trackerPelvis.transform.forward.z));
            }
            // One direction rotation.
            else if (myDirectionMethod.Equals(MyDirectionalMethod.OneConstantDirection))
            {
                returnDirection = (new Vector3(0, 0, -1));
            }

            return returnDirection;
        }

        protected override void SetDeltaTransformData()
        {
            for (int i = 0; i < trackedObjects.Count; i++)
            {
                Transform trackedObj = trackedObjects[i];
                // Get delta postions and rotations
                if(isAxisYTheTriggerAxis)
                    previousTriggerAxisPositions[trackedObj] = trackedObj.transform.localPosition.y;
                else
                    previousTriggerAxisPositions[trackedObj] = trackedObj.transform.localPosition.z;
            }
        }

        public override Vector3 GetMovementDirection()
        {
            return direction;
        }

        public override float GetSpeed()
        {
            return currentSpeed;
        }
    }
}
