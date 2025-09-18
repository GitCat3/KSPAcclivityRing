using UnityEngine;

namespace KSPAcclivityRing
{
    public class KSPAcclivityRing : PartModule
    {
        private Animation animator;
        private float verticalVelocityLerp = 0f;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Active")]
        [UI_Toggle(
        affectSymCounterparts = UI_Scene.All,
        scene = UI_Scene.All,
        controlEnabled = false,
        invertButton = false,
        requireFullControl = false,
        disabledText = "Deactivated",
        enabledText = "Activated"
            )]
        public bool activated;

        [KSPAction("Toggle Active")]
        public virtual void ToggleAcclivity(KSPActionParam param)
        {
            activated = !activated;
        }

        public override void OnStart(StartState state)
        {
            animator = part.FindModelComponent<Animation>();
            if (animator == null) return;
            animator["AcclivityUnglow"].time = animator["AcclivityUnglow"].length;
        }

        private Quaternion GetSurfaceRotation(global::Vessel vessel)
        {
            // This code was stolen from Kerbal Engineer Redux which derived this code from MechJeb2's implementation for getting the vessel's surface relative rotation.
            var centreOfMass = vessel.CoMD;
            var up = (centreOfMass - vessel.mainBody.position).normalized;
            var north = Vector3.ProjectOnPlane((vessel.mainBody.position + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius) - centreOfMass, up).normalized;

            return Quaternion.Inverse(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(vessel.transform.rotation) * Quaternion.LookRotation(north, up));
        }





        public void Update()
        {
            if (animator == null || vessel == null) return;
            if (activated)
            {
                var surfaceRotation = GetSurfaceRotation(vessel);
                var roll = surfaceRotation.eulerAngles.z > 180.0f ? 360.0f - surfaceRotation.eulerAngles.z : -surfaceRotation.eulerAngles.z;
                if (-90 < roll && roll < 90 && vessel.mainBody != null)
                {
                    var upDir = (vessel.transform.position - vessel.mainBody.position).normalized;
                    vessel.gravityMultiplier = 0;
                    if (FlightUIModeController.Instance.Mode == FlightUIMode.DOCKING)
                    {
                        var vesselUp = (vessel.transform.position - vessel.mainBody.position).normalized;
                        var targetSpeed = vessel.ctrlState.Z * -30;
                        verticalVelocityLerp = Mathf.MoveTowards(verticalVelocityLerp, targetSpeed, 10f * TimeWarp.fixedDeltaTime);
                        float currentVerticalSpeed = Vector3.Dot(vessel.obt_velocity, upDir);
                        vessel.ChangeWorldVelocity(-upDir * currentVerticalSpeed);
                        vessel.ChangeWorldVelocity(upDir * verticalVelocityLerp);
                    }
                    animator.Play("AcclivityGlow");
                }
                else { 
                    activated = false;
                }
            }
            else {
                vessel.gravityMultiplier = 1;
                animator.Play("AcclivityUnglow");
            }
        }
    }
}