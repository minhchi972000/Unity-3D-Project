using UnityEngine;

namespace FarmValley.Player
{
    /// <summary>
    /// Simple procedural animation for the player character.
    /// Handles idle bobbing, walk/run leg and arm swinging, and squash-stretch.
    /// Works with the placeholder primitive-based character model.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        [Header("Animation Settings")]
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float walkBobFrequency = 8f;
        [SerializeField] private float limbSwingAngle = 25f;
        [SerializeField] private float runLimbSwingAngle = 40f;

        private Transform visualRoot;
        private Transform leftArm, rightArm, leftLeg, rightLeg;
        private Vector3 initialVisualPos;
        private float animTime;

        private void Start()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            // Find visual root and limbs
            var visualSetup = GetComponent<PlayerVisualSetup>();
            if (visualSetup != null)
            {
                visualRoot = visualSetup.GetVisualRoot();
            }

            if (visualRoot == null)
            {
                visualRoot = transform.Find("PlayerVisual");
            }

            if (visualRoot != null)
            {
                initialVisualPos = visualRoot.localPosition;
                leftArm = visualRoot.Find("LeftArm");
                rightArm = visualRoot.Find("RightArm");
                leftLeg = visualRoot.Find("LeftLeg");
                rightLeg = visualRoot.Find("RightLeg");
            }
        }

        private void Update()
        {
            if (visualRoot == null || playerController == null) return;

            animTime += Time.deltaTime;

            if (playerController.IsMoving)
            {
                AnimateWalking();
            }
            else
            {
                AnimateIdle();
            }
        }

        private void AnimateIdle()
        {
            // Gentle bobbing
            float bob = Mathf.Sin(animTime * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            visualRoot.localPosition = initialVisualPos + new Vector3(0, bob, 0);

            // Reset limb rotations smoothly
            ResetLimb(leftArm);
            ResetLimb(rightArm);
            ResetLimb(leftLeg);
            ResetLimb(rightLeg);
        }

        private void AnimateWalking()
        {
            // Faster bobbing while walking
            float freq = playerController.IsRunning ? walkBobFrequency * 1.3f : walkBobFrequency;
            float bob = Mathf.Abs(Mathf.Sin(animTime * freq * Mathf.PI)) * bobAmplitude * 1.5f;
            visualRoot.localPosition = initialVisualPos + new Vector3(0, bob, 0);

            // Swing limbs
            float swingAngle = playerController.IsRunning ? runLimbSwingAngle : limbSwingAngle;
            float swing = Mathf.Sin(animTime * freq * Mathf.PI) * swingAngle;

            // Arms swing opposite to legs
            if (leftArm != null)
                leftArm.localRotation = Quaternion.Euler(swing, 0, 0);
            if (rightArm != null)
                rightArm.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Euler(-swing * 0.8f, 0, 0);
            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Euler(swing * 0.8f, 0, 0);
        }

        private void ResetLimb(Transform limb)
        {
            if (limb == null) return;
            limb.localRotation = Quaternion.Slerp(limb.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }
}
