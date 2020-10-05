using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class AudioClipModulationEffect : TweenEffect<float>
    {
        public AudioClipModulationEffect()
        {
            Description = "Audio Clip Modulation Effect allows you to a sound clip and adjust volume using easing.";
        }

        [Tooltip("AudioSource located on target, or origin.")]
        public AudioSource source;
        
        [Tooltip("Use to modulate pitch instead of the volume.")]
        public bool modulatePitch;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new AudioClipModulationEffect{source = source, modulatePitch = modulatePitch};
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }
        
        protected override void SetValue(GameObject target, float value)
        {
            if (modulatePitch)
            {
                source.pitch = value;
            }
            else
            {
                source.volume = value;
            }
        }

        protected override float GetValue(GameObject target)
        {
            return modulatePitch ? source.pitch : source.volume;
        }

        protected override float GetRelativeValue(float fromValue, float addValue)
        {
            return fromValue + addValue;
        }

        protected override float GetDifference(float fromValue, float toValue)
        {
            return toValue - fromValue;
        }

        protected override void ExecuteSetup()
        {
            if (target == null) return;
            
            if (source == null)
            {
                source = target.GetComponent<AudioSource>();
                if (source == null)
                {
                    source = origin.GetComponent<AudioSource>();
                    if (source == null)
                    {
                        Debug.LogError("No AudioSource attached to target or origin.");
                        return;    
                    }
                }
            }

            base.ExecuteSetup();
        }

        protected override bool TickTween()
        {
            if (source == null) return true;
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));

            return false;
        }
    }
}