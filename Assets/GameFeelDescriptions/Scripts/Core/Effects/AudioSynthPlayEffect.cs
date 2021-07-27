using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    // NOTE: To use this effect, download usfxr from https://github.com/zeh/usfxr and uncomment the class.
    //TODO: implement a proper interface + fix the AudioListener issue 25/05/2020
    public class AudioSynthPlayEffect : DurationalGameFeelEffect //Maybe use durational here instead !!!
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
            GenerateSynthParameters(false, Random.Range(1, 10));
        }

        [Header("The base sound style type, for generating sound when ")]
        //[HideFieldIf("synthParameters", "", negate = true)]
        [SynthGenerateFromBaseButton]
        public SynthBaseSounds soundGeneratorBase = EnumExtensions.GetRandomValue<SynthBaseSounds>();
	
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

        public float GetSoundLength()
        {
            if (synth == null || synth.parameters == null) return -1f;
            
            //Envelope length
            // _envelopeLength0 = p.attackTime * p.attackTime * 100000.0f;
            // _envelopeLength1 = p.sustainTime * p.sustainTime * 100000.0f;
            // _envelopeLength2 = p.decayTime * p.decayTime * 100000.0f + 10f;
            // _envelopeLength = _envelopeLength0;
            // _envelopeFullLength = (uint)(_envelopeLength0 + _envelopeLength1 + _envelopeLength2);
            var length = synth.parameters.attackTime * synth.parameters.attackTime * 100000.0f;
            length += synth.parameters.sustainTime * synth.parameters.sustainTime * 100000.0f;
            length += synth.parameters.decayTime * synth.parameters.decayTime * 100000.0f + 10f;
            
            //We use bit depth 16 and sampleRate 44100 
            // uint soundLength = _envelopeFullLength;
            // if (__bitDepth == 16) soundLength *= 2;
            // if (__sampleRate == 22050) soundLength /= 2;
            length *= 2;
            
            //Block alignment and bytes per second
            // uint blockAlign = __bitDepth / 8;
            // uint bytesPerSec = __sampleRate * blockAlign;
            var bytesPerSec = 44100f * 2f;
            
            //length [0 - 300010] / bytesPerSecond 88200 => 0s-3,40s
            //Convert to seconds!
            return length / bytesPerSec;
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
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
            
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
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
            Duration = GetSoundLength();

            if (playSound)
            {
                PlaySound();
            }
        }

        public string GenerateSynthParameters(bool playSound = false, int intensity = 1)
        {
            intensity = Mathf.Clamp(intensity, 1, 10);
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
            
            //Set all generated sounds to low volume.
            synth.parameters.masterVolume = 0.05f;
		
            //Adjust for severity
            synth.parameters.masterVolume += 0.005f * intensity;
            
            //TODO: adjust more parameters for severity 2020-08-13
            //synth.parameters. = 0.1f * severity;
            
            Duration = GetSoundLength();
            
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

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean(amount))
            {
                mutateOnExecution = !mutateOnExecution;
            }
            
            if (RandomExtensions.Boolean(amount))
            {
               var cooldownAmount = RandomExtensions.MutationAmount(amount);
               Cooldown = Mathf.Max(0,Cooldown + cooldownAmount);
            }
            
            //This also fixes the duration!
            synthParameters = MutateSynthParameters(false, amount);
            
            base.Mutate(amount);
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
            Duration = GetSoundLength();
            
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
            
            //NOTE: Play on Setup means Play on first tick, because we're not actually executing during the duration.
            PlaySound();
        }

        protected override bool ExecuteTick()
        {
            
            //NOTE: AudioSynthPlayEffect is Durational, in order to be able to queue effects after the sound finishes.
            //We're just doing nothing here, because the sounds if not played by our system.
            //PlaySound(); //PlaySound is executed in the Setup function, ie. on first tick.
            
            //We return false, so it waits for the duration.
            return false;
        }
        
        public override bool CompareTo(GameFeelEffect other)
        {
            return other is AudioSynthPlayEffect audio && other.target == target && audio.synthParameters == synthParameters;
        }
    }
}