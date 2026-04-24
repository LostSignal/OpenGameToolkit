//-----------------------------------------------------------------------
// <copyright file="MoveInteractable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace HavenXR
{
    using System;
    using OGT;
    using UnityEngine;

    public class MoveInteractable : Interactable
    {
        #pragma warning disable 0649
        [Tooltip("If null, it will move this object.")]
        [SerializeField] private Transform objectToMove;

        [EnumFlag]
        [SerializeField] private Axis movementAxis;
        #pragma warning restore 0649

        private Vector3 previousHitVector = InvalidVector;

        // cached values
        [NonSerialized]
        private float minimumPixelMovementSquared;

        [Flags]
        public enum Axis
        {
            X = 1 << 1,
            Y = 1 << 2,
            Z = 1 << 3,
        }

        protected override void Awake()
        {
            base.Awake();

            // If objectToMove is not spcified, then default it to this object
            if (this.objectToMove == null)
            {
                this.objectToMove = this.transform;
            }

            this.minimumPixelMovementSquared = OGT.Input.GetMinimumPixelMovementSquared();
        }

        protected override void OnInput(OGT.Input input, Collider collider, Camera camera)
        {
            // if (this.previousHitVector == InvalidVector && input.InputState == InputState.Pressed)
            // {
            //     if (collider.Raycast(camera.ScreenPointToRay(input.CurrentPosition), out RaycastHit previousHit, float.MaxValue))
            //     {
            //         this.previousHitVector = (previousHit.point - collider.transform.position).normalized;
            //         return;
            //     }
            // }
            // else if (this.previousHitVector == InvalidVector || Time.deltaTime == 0.0f)
            // {
            //     return;
            // }
            //
            // Transform colliderTransform = collider.transform;
            // Vector3 colliderPosition = colliderTransform.position;
            //
            // var moveVector = Vector3.zero;
            // moveVector += GetMoveVector(Axis.X, new Plane(colliderPosition, colliderPosition + colliderTransform.up, colliderPosition + colliderTransform.forward));
            // moveVector += GetMoveVector(Axis.Y, new Plane(colliderPosition, colliderPosition + colliderTransform.forward, colliderPosition + colliderTransform.right));
            // moveVector += GetMoveVector(Axis.Z, new Plane(colliderPosition, colliderPosition + colliderTransform.up, colliderPosition + colliderTransform.right));
            //
            // // Translate this movevector into worldspace and apply it to our this.objectToMove
            // var worldMoveVector = this.transform.localToWorldMatrix.MultiplyVector(moveVector);
            // this.objectToMove.transform.position += worldMoveVector;
            //
            // // this.previousHitVector = input.InputState == InputState.Released ? InvalidVector : currentHitVector;
            //
            // Vector3 GetMoveVector(Axis axis, Plane plane)
            // {
            //     Ray inputRay = camera.ScreenPointToRay(input.CurrentPosition);
            //
            //     float enter;
            //     if (plane.Raycast(inputRay, out enter) == false)
            //     {
            //         // if we didnt' hit the plane
            //         return Vector3.zero;
            //     }
            //
            //     // calculating the current hit vector
            //     Vector3 hitPoint = inputRay.GetPoint(enter);
            //     Vector3 currentHitVector = (hitPoint - collider.transform.position).normalized;
            //
            //     float cosTheta = Vector3.Dot(currentHitVector, this.previousHitVector);
            //     Vector3 cross = Vector3.Cross(this.previousHitVector, currentHitVector);
            //     float theta = 0.0f;
            //
            //     // NOTE [bgish]: Sometimes, if cosTheta is too close to 1, but slightly off (by 1 bit), it turns costTheta into a NaN and breaks every
            //     //               00111111 10000000 00000000 00000000 - C# thinks it's 1.00000000 and this is fine (0x3F800000)
            //     //               00111111 10000000 00000000 00000001 - C# thinks it's 1.00000000 and this is bad  (0x3F800001)
            //     //               Lost.Logger.Log("0x3F800000 = " + Mathf.Acos(BitConverter.ToSingle(BitConverter.GetBytes(0x3F800000), 0)));  // 0x3F800000 = 1
            //     //               Lost.Logger.Log("0x3F800001 = " + Mathf.Acos(BitConverter.ToSingle(BitConverter.GetBytes(0x3F800001), 0)));  // 0x3F800001 = NaN
            //     // if (cosTheta < 0.999999f)
            //     // {
            //     //     bool didFingerMove = (input.PreviousPosition - input.CurrentPosition).sqrMagnitude > minimumPixelMovementSquared;
            //     //
            //     //     if (didFingerMove)
            //     //     {
            //     //         Vector3 crossVector;
            //     //
            //     //         if (this.rotationalAxis == Axis.X)
            //     //         {
            //     //             crossVector = collider.transform.right;
            //     //         }
            //     //         else if (this.rotationalAxis == Axis.Y)
            //     //         {
            //     //             crossVector = collider.transform.up;
            //     //         }
            //     //         else if (this.rotationalAxis == Axis.Z)
            //     //         {
            //     //             crossVector = collider.transform.forward;
            //     //         }
            //     //         else
            //     //         {
            //     //             Lost.Logger.LogErrorFormat("CircleInteractable found unknown rotationalAxis {0}", this.rotationalAxis);
            //     //             crossVector = Vector3.zero;
            //     //         }
            //     //
            //     //         theta = Mathf.Acos(cosTheta) * 180.0f / Mathf.PI * (Vector3.Dot(cross, crossVector) > 0 ? 1 : -1);
            //     //     }
            //     // }
            //
            //     return Vector3.zero;
            // }
        }
    }
}
