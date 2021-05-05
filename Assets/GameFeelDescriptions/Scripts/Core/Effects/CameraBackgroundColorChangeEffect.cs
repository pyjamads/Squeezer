using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace GameFeelDescriptions
{
    public class CameraBackgroundColorChangeEffect : ColorChangeEffect
    {
        public CameraBackgroundColorChangeEffect()
        {
            Description = "Camera Background Color change Effect allows you to change a camera background color field using easing.";
        }
        
        [Tooltip("No reference, makes the effect lookup a camera on the target or the main camera.")]
        public Camera cameraToModify;   
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new CameraBackgroundColorChangeEffect{cameraToModify = cameraToModify};
            cp.Init(origin, target, triggerData);

            //Handling the cameraToModify setup here, to be able to a better use CompareTo
            if(cp.cameraToModify == null)
            {
                if (cp.target == null)
                {
                    cp.cameraToModify = Camera.main;
//                    Debug.LogError("Set CameraToModify in the editor, with AppliesTo is set to EditorValue.");
//                    return null;
                }
                else
                {
                    cp.cameraToModify = target.GetComponent<Camera>();
                    if(cp.cameraToModify == null)
                    {
                        cp.cameraToModify = Camera.main;
//                    Debug.LogError("No camera attached to target.");
//                    return null;
                    }    
                }
            }
            
            return DeepCopy(cp, ignoreCooldown);
        }
        
        protected override void SetValue(GameObject target, Color value)
        {
            if (cameraToModify == null) return;
            
            cameraToModify.backgroundColor = value;
        }

        protected override Color GetValue(GameObject target)
        {
            if(cameraToModify == null) return Color.magenta;
            
            return cameraToModify.backgroundColor;
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is CameraBackgroundColorChangeEffect cam && cam.cameraToModify == cameraToModify;
        }

        protected override bool TickTween()
        {
            if(cameraToModify == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            //TODO: Add relative value change, which would remove the singleton need!!! 2021-05-05T17:49:45+02:00
            /*
             var easeFunc = GetEaseFunc();
             if (relative)
             {
                 var progress = elapsed / Duration;
                 var prevProgress = oldElapsed / Duration;
 
                 if (reverse)
                 {
                     progress = 1 - progress;
                     prevProgress = 1 - prevProgress;
                 }
 
                 
                 var prev = diffAmount * easeFunc.Invoke(prevProgress);
                 var current = diffAmount * easeFunc.Invoke(progress);
                 
                 //amount = end - start;
                 //current + (amount * easing(t1)) - (amount * - easing(t0));
                 SetValue(target, GetValue(target) + (reverse ? -1 : 1) * (current - prev));
             }
             else
             {
                 //@from  + (to - @from) * easing(t);
                 SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, easeFunc));    
             }
             */
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));

            return false;
        }
    }
}