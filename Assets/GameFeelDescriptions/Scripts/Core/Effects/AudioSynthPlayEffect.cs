using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    // NOTE: To use this effect, download usfxr from https://github.com/zeh/usfxr and uncomment the class.
    //TODO: implement a proper interface + fix the AudioListener issue 25/05/2020
    public class AudioSynthPlayEffect : GameFeelEffect
    {
        #region Attribute Definitions
        // Attribute Classes is defined here, so people don't accidentally use them for other stuff!
        /// <summary>
        /// Make a [generate and play] button, next to the dropdown! 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class SynthGenerateFromBaseButtonAttribute : PropertyAttribute { }
        
        /// <summary>
        /// Make a [mutate and play] and [play] button, next to the text field! 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class MutateSynthParametersButtonAttribute : PropertyAttribute { }

        /// <summary>
        /// Make a [copy to SynthParameters (with tooltip)] and [play] button next to each entry!
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class CopyToSynthParametersButtonAttribute : PropertyAttribute { }
        
        #endregion
        
        public enum SynthBaseSounds
        {
            PickupCoin,
            LaserShoot,
            Explosion,
            PowerUp,
            HitHurt,
            Jump,
            BlipSelect,
            Random,
        }
        
        public AudioSynthPlayEffect()
        {
            Description = "Audio Synth Effect, use for finding the sound you want, generate actual sound clips using the Editor window.";
        }

        [Header("The base sound style type, for generating sound when ")]
        //[HideFieldIf("synthParameters", "", negate = true)]
        [SynthGenerateFromBaseButton]
        public SynthBaseSounds soundGeneratorBase;
	
        [Space(-5, order = 0), 
         Header ("While undefined, the synth will generate a random sound with the given base.", order = 1), 
         Space(-10, order = 2),
         Header("The synth parameters used to generate the sound.", order = 3)]
        [MutateSynthParametersButton]
        public string synthParameters;

        [Header("Uses the synthParameters as a base, and mutates the sound before playing.")]
        public bool mutateOnExecution;

        [Header("Helper list, that stores the latest sounds generated.")]
        [CopyToSynthParametersButton]
        public List<string> latestSynthParameters;

        private SfxrSynth synth;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var nextParameterSet = synthParameters;
            
            if (string.IsNullOrWhiteSpace(nextParameterSet))
            {
                nextParameterSet = GenerateSynthParameters();
            }
            
            var cp = new AudioSynthPlayEffect
            {
                soundGeneratorBase = soundGeneratorBase, 
                synthParameters = nextParameterSet,
                mutateOnExecution = mutateOnExecution,

                //We don't need the latestParameters to be copied.
                //latestSynthParameters = latestSynthParameters
            };
            
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }
        
        public void PlaySound()
        {
            if (synth == null)
            {
                synth = new SfxrSynth();
                synth.parameters.SetSettingsString(synthParameters);
            }
            
            synth.Play();
        }
        
        public void LoadSynthParameters(bool playSound = false)
        {
            if (synth == null)
            {
                synth = new SfxrSynth();
            }
            
            synth.parameters.SetSettingsString(synthParameters);
		
            if (playSound)
            {
                PlaySound();
            }
        }

        public string GenerateSynthParameters(bool playSound = false)
        {
            if (synth == null)
            {
                synth = new SfxrSynth();
            }
            
            switch (soundGeneratorBase)
            {
                case SynthBaseSounds.PickupCoin:
                    synth.parameters.GeneratePickupCoin();
                    break;
                case SynthBaseSounds.LaserShoot:
                    synth.parameters.GenerateLaserShoot();
                    break;
                case SynthBaseSounds.Explosion:
                    synth.parameters.GenerateExplosion();
                    break;
                case SynthBaseSounds.PowerUp:
                    synth.parameters.GeneratePowerup();
                    break;
                case SynthBaseSounds.HitHurt:
                    synth.parameters.GenerateHitHurt();
                    break;
                case SynthBaseSounds.Jump:
                    synth.parameters.GenerateJump();
                    break;
                case SynthBaseSounds.BlipSelect:
                    synth.parameters.GenerateBlipSelect();
                    break;
                case SynthBaseSounds.Random:
                    synth.parameters.Randomize();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
		
            var synthParams = synth.parameters.GetSettingsString();

            if (latestSynthParameters == null || latestSynthParameters.Count == 0)
            {
                latestSynthParameters = new List<string>{synthParams};
            }
            else
            {
                latestSynthParameters.Insert(0, synthParams);    
            }

            if (playSound)
            {
                PlaySound();
            }

            return synthParams;
        }

        public string MutateSynthParameters(bool playSound = false, float mutationAmount = 0.05f)
        {
            if (string.IsNullOrEmpty(synthParameters)) return "";
            
            if (synth == null)
            {
                synth = new SfxrSynth();
            }

            synth.parameters.SetSettingsString(synthParameters);
            synth.parameters.Mutate(mutationAmount);
            
            var synthParams = synth.parameters.GetSettingsString();

            if (latestSynthParameters == null || latestSynthParameters.Count == 0)
            {
                latestSynthParameters = new List<string>{synthParams};
            }
            else
            {
                latestSynthParameters.Insert(0, synthParams);    
            }
            
            if (playSound)
            {
                PlaySound();
            }

            return synthParams;
        }
        
//        protected override void SetValue(GameObject target, float value)
//        {
//            if (modulatePitch)
//            {
//                source.pitch = value;
//            }
//            else
//            {
//                source.volume = value;
//            }
//        }
//
//        protected override float GetValue(GameObject target)
//        {
//            return modulatePitch ? source.pitch : source.volume;
//        }
//
//        protected override float GetRelativeValue(float fromValue, float addValue)
//        {
//            return fromValue + addValue;
//        }
//
//        protected override float GetDifference(float fromValue, float toValue)
//        {
//            return toValue - fromValue;
//        }

        protected override void ExecuteSetup()
        {
            LoadSynthParameters();

            if (mutateOnExecution)
            {
                MutateSynthParameters();
            }
            
//            if (source == null)
//            {
//                source = target.GetComponent<AudioSource>();
//                if (source == null)
//                {
//                    source = origin.GetComponent<AudioSource>();
//                    if (source == null)
//                    {
//                        Debug.LogError("No AudioSource attached to target or origin.");
//                        return;    
//                    }
//                }
//            }

            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            PlaySound();

            return false;
        }

//        public override IEnumerator ExecuteTween(GameObject target, float start, float duration, float end, Vector3? interactionDirection, bool unscaledTime)
//        {
//            if (source == null) yield break;
//
//            yield return new TweenFloat().Tween(value => SetValue(target, value), start, duration, end, GetEaseFunc(), unscaledTime);
//        }
    }
}