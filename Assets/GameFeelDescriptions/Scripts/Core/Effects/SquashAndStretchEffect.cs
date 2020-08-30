using System;
using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    //TODO: make a squash and strech continuous effect!
    //NOTE: This is actually just a squash/stretch effect, not squash and stretch (use flipDirection to switch between the two)
    //Consider making two separate effects squash and stretch.
    
    public class SquashAndStretchEffect : DurationalGameFeelEffect
    {   
        public SquashAndStretchEffect()
        {
            Description = "Squash and Stretch Effect allows you to scale an object using easing.";
        }

        [Tooltip("Flip the squash and stretch axis.")]
        public bool Stretch;

        [Tooltip("How much squashing to apply, Zero means non [0-1[")]
        [Range(0f, (1f-float.Epsilon))]
        public float Amount;

        public EasingHelper.EaseType easeIn;
        
        public EasingHelper.EaseType easeOut;

        public bool lockLocalPosition;
        
        public bool resetSizeAfterEffect;
        public Vector3 resetSize;
        
        private Vector3 modified;
        private Vector3 original;

        private Vector3 maxOffset;
        private Vector3 currentOffset;

        private Vector3 collisionPos;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new SquashAndStretchEffect();
            cp.Stretch = Stretch;
            cp.Amount = Amount;
            cp.easeIn = easeIn;
            cp.easeOut = easeOut;
            cp.resetSizeAfterEffect = resetSizeAfterEffect;
            cp.lockLocalPosition = lockLocalPosition;
            cp.resetSize = resetSize;
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is SquashAndStretchEffect && base.CompareTo(other);
        }

        protected void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;
            
            target.transform.localScale = value;
        }

        protected Vector3 GetSquashedValue(Vector3 fromValue, Vector3 normal, bool stretch)
        {
            //NOTE: Would probably be better to choose which perpendicular normal to use when "flipping". 20/02/2020
            //TODO: maybe make this volume/area dependent, instead of this simple, add the amount to the other directions. 20/02/2020
            
            /* From Shatter effect!
             * var qbrt = Mathf.Pow(AmountOfPieces, 1f / 3f);
             * var scale = target.transform.localScale / qbrt;
             */
//            var volume = fromValue.x * fromValue.y * fromValue.z;
//            var k = 1f - (volume * amount) / (fromValue.x * fromValue.y * fromValue.z);
            
            var squashDirection = new Vector3(fromValue.x * Mathf.Abs(normal.x),fromValue.y * Mathf.Abs(normal.y),fromValue.z * Mathf.Abs(normal.z));
            
            if (stretch)
            {
                //squashDirection *= 1f + (1f - Amount);
                //Make it longer in the normal direction. Amount = [0,1[ where 0 means non.
                squashDirection *= 1f + Amount;
            }
            else
            {
                //Make it shorter in the normal direction. Amount = [0,1[ where 0 means non.
                squashDirection *= 1f - Amount;  
            }

            var stretchDirections = new Vector3(fromValue.x * (1f-Mathf.Abs(normal.x)),fromValue.y * (1f-Mathf.Abs(normal.y)),fromValue.z * (1f-Mathf.Abs(normal.z)));
            
            if (stretch)
            {
                //Make it slimmer in two other directions. Amount = [0,1[ where 0 means non.
                stretchDirections *= (1f - Amount) / 2f;
            }
            else
            {
                //Make it wider in the two other directions. Amount = [0,1[ where 0 means non.
                stretchDirections *= 1f + Amount / 2f;    
            }

            return squashDirection + stretchDirections;
        }

        protected override void ExecuteSetup()
        {
            if (target == null) return;
            
            if (interactionDirection == null)
            {
                Debug.LogError("interactionDirection needs to be provided for SquashAndStretch to work.");
                return;
            }

            //Calculate direction to squash!
            var forward = target.transform.forward;
            var up = target.transform.up;
            var right = target.transform.right;
            
            var forwardAmount = Vector3.Dot(forward, interactionDirection.Value);
            var backAmount = Vector3.Dot(-forward, interactionDirection.Value);
            var upAmount = Vector3.Dot(up, interactionDirection.Value);
            var downAmount = Vector3.Dot(-up, interactionDirection.Value);
            var rightAmount = Vector3.Dot(right, interactionDirection.Value);
            var leftAmount = Vector3.Dot(-right, interactionDirection.Value);

            var max = Mathf.Max(forwardAmount, backAmount, upAmount, downAmount, rightAmount, leftAmount);
            var maxDir = Vector3.zero;

            if (Math.Abs(max - forwardAmount) < float.Epsilon) maxDir = Vector3.forward;
            else if (Math.Abs(max - backAmount) < float.Epsilon) maxDir = Vector3.back;
            else if (Math.Abs(max - upAmount) < float.Epsilon) maxDir = Vector3.up;
            else if (Math.Abs(max - downAmount) < float.Epsilon) maxDir = Vector3.down;
            else if (Math.Abs(max - rightAmount) < float.Epsilon) maxDir = Vector3.right;
            else if (Math.Abs(max - leftAmount) < float.Epsilon) maxDir = Vector3.left;

            
            //Get squashed scale and original scale. (Why do we use the lossy scale here, because of the general direction of things??)
            modified = GetSquashedValue(target.transform.lossyScale, maxDir, Stretch);
            original = target.transform.localScale;
            
            //Position offset, should be a little less than half the distance of the scale amount.
            var localOffsetAmount = 0.49f;
            var offsetAmount = (Mathf.Abs(maxDir.Dot(original) - maxDir.Dot(modified)) * localOffsetAmount);
            maxOffset = Stretch ? maxDir * offsetAmount : maxDir * (-1f * offsetAmount);
            currentOffset = Vector3.zero;
            //Offset the collider slightly, because we just collided
            collisionPos = target.transform.localPosition - maxOffset * 0.1f;
        }
        
        protected override bool ExecuteTick()
        {
            if (target == null) return true;
            
            if (interactionDirection == null)
            {
                Debug.LogError("interactionDirection needs to be provided for SquashAndStretch to work.");
                return true;
            }

            //For squash effect, spend a 5th of the time on "squashing", and the rest on recovering.
            if (elapsed <= Duration / 5f)
            {
                SetValue(target, TweenHelper.Interpolate(original, Mathf.Clamp01(elapsed / (Duration / 5f)), modified, EasingHelper.Ease(easeIn)));

                if (lockLocalPosition)
                {
                    //Remove the current offset, calculate the new offset, and add that.
                    //target.transform.localPosition -= currentOffset;
                    currentOffset = TweenHelper.Interpolate(Vector3.zero, Mathf.Clamp01(elapsed / (Duration / 5f)), maxOffset, EasingHelper.Ease(easeIn));
                    //target.transform.localPosition += currentOffset;
                    target.transform.localPosition = collisionPos + currentOffset;   
                }
            }
            else
            {
                SetValue(target, TweenHelper.Interpolate(modified, Mathf.Clamp01((elapsed - (Duration / 5f)) / (Duration * (4/5f))), original, EasingHelper.Ease(easeOut)));

                if (lockLocalPosition)
                {
                    //Remove the current offset, calculate the new offset, and add that.
                    //target.transform.localPosition -= currentOffset;
                    currentOffset = TweenHelper.Interpolate(maxOffset,
                        Mathf.Clamp01((elapsed - (Duration / 5f)) / (Duration * (4 / 5f))), Vector3.zero,
                        EasingHelper.Ease(easeOut));
                    //target.transform.localPosition += currentOffset;
                    //On our way out, just add a tiny bit extra separation.
                    target.transform.localPosition = collisionPos + currentOffset;
                }
            }

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;
            
            if (resetSizeAfterEffect)
            {
                SetValue(target, resetSize);
            }
            else
            {
                SetValue(target, original);
            }
            
            base.ExecuteCleanUp();
        }
    }
}