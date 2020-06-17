using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class AudioClipPlayEffect : GameFeelEffect
    {
        public AudioClipPlayEffect()
        {
            Description = "Play Audio clip Effect allows you to a sound clip and adjust volume using easing.";
        }

        [Header("Create an AudioSource on the target.")]
        public bool createAudioSource;
        
        [Header("AudioSource to use, otherwise it will be located on target, or origin.")]
        public AudioSource source;
        
        [Header("Clip to play, on the selected audioSource.")]
        public AudioClip clip;

        [Header("Play one shot, instead of playing directly from the source itself.")]
        [HideFieldIf("playLoop", true)]
        public bool playOneShot;

        [HideFieldIf("playOneShot", false)]
        public float pitchShiftAmount;
        
        [HideFieldIf("playOneShot", false)]
        public float pitchResetDelay = 1f;

        
        [HideFieldIf("playOneShot", true)]
        [Header("Play the audio clip looping.")]
        public bool playLoop;

        [HideInInspector]
        public float currentPitch = 1;
        
        [HideInInspector]
        public float lastPitchChangeTime;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            
            var cp = new AudioClipPlayEffect
            {
                createAudioSource = createAudioSource, 
                source = source, 
                clip = clip, 
                playOneShot = playOneShot, 
                playLoop = playLoop,
                pitchShiftAmount = pitchShiftAmount,
                pitchResetDelay = pitchResetDelay
            };

            //Reset the pitch if it's been too long since the last play time.
            if (Time.unscaledTime - lastPitchChangeTime > pitchResetDelay)
            {
                cp.currentPitch = currentPitch = 1f;
            }
            else
            {
                cp.currentPitch = pitchShiftAmount != 0 ? currentPitch + pitchShiftAmount : currentPitch;
            }
            
            lastPitchChangeTime = Time.unscaledTime;
            
            cp.Init(origin, target, unscaledTime, interactionDirection);
            
            
            return DeepCopy(cp);
        }

        protected override void ExecuteSetup()
        {
            if (source == null)
            {
                if (createAudioSource)
                {
                    source = target.AddComponent<AudioSource>();
                }
                else
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
            }

            if (playOneShot == false)
            {
                if (clip != null)
                {
                    source.clip = clip;
                }
            }

            source.loop = playLoop;

            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (source == null || clip == null) return true;

            if (playOneShot)
            {
                source.pitch = currentPitch;
                source.PlayOneShot(clip);
            }
            else
            {
                source.Play();
            }

            return false;
        }

//        public override void OverrideEffect(GameFeelEffect next)
//        {
//            if (source == null || clip == null) return;
//            
//            //TODO: save time in playback, and resume from that point in time?
//            if (playOneShot == false)
//            {
//                var time = source.time;
//                var effect = next as AudioClipPlayEffect;
//                effect.source = source;
//            }
//            
//            throw new System.NotImplementedException();
//        }
    }
}