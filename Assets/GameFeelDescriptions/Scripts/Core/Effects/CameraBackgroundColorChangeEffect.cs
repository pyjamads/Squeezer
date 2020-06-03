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
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new CameraBackgroundColorChangeEffect{cameraToModify = cameraToModify};
            cp.Init(origin, target, unscaledTime, interactionDirection);

            //Handling the cameraToModify setup here, to be able to a better use CompareTo
            if(cp.cameraToModify == null)
            {
                if (cp.target == null)
                {
                    cp.cameraToModify = Camera.main;
//                    Debug.LogError("Set CameraToModify in the editor, with AppliesTo is set to EditorValue.");
//                    return null;
                }
                
                cp.cameraToModify = target.GetComponent<Camera>();
                if(cp.cameraToModify == null)
                {
                    cp.cameraToModify = Camera.main;
//                    Debug.LogError("No camera attached to target.");
//                    return null;
                }
            }
            
            return DeepCopy(cp);
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
            if(cameraToModify == null) return true;
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / duration, end, GetEaseFunc()));

            return false;
        }
    }
}