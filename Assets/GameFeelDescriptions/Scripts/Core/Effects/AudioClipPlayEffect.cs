using System.Collections;
using System.Collections.Generic;
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

        [HideFieldIf("createAudioSource", false)]
        public float volume = 0.5f;
        
        [Header("AudioSource to use, otherwise it will be located on target, or origin.")]
        public AudioSource source;
        
        [Header("Clip to play, on the selected audioSource.")]
        public List<AudioClip> clips;

        [Header("Play one shot, instead of playing directly from the source itself.")]
        [DisableFieldIf("playLoop", true)]
        public bool playOneShot = true;

        [HideFieldIf("playOneShot", false)]
        public float pitchShiftAmount;
        
        [HideFieldIf("playOneShot", false)]
        public float pitchResetDelay = 1f;

        
        [DisableFieldIf("playOneShot", true)]
        [Header("Play the audio clip looping.")]
        public bool playLoop;

        [HideInInspector]
        public float currentPitch = 1;
        
        [HideInInspector]
        public float lastPitchChangeTime;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            
            var cp = new AudioClipPlayEffect
            {
                createAudioSource = createAudioSource,
                source = source, 
                clips = clips, 
                playOneShot = playOneShot, 
                volume = volume,
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
            
            cp.Init(origin, target, triggerData);
            
            
            return DeepCopy(cp, ignoreCooldown);
        }

        protected override void ExecuteSetup()
        {
            if (target == null)
            {
                target = Camera.main.gameObject;
            }
            
            if (source == null)
            {
                if (createAudioSource)
                {
                    source = target.AddComponent<AudioSource>();
                    source.volume = volume;
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
                if (clips != null && clips.Count > 0)
                {
                    source.clip = clips.GetRandomElement();
                }
            }

            source.loop = playLoop;

            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (source == null || clips == null || clips.Count == 0) return true;

            if (playOneShot)
            {
                source.pitch = currentPitch;
                source.PlayOneShot(clips.GetRandomElement());
            }
            else
            {
                source.Play();
            }

            //We're done
            return true;
        }
    }
}