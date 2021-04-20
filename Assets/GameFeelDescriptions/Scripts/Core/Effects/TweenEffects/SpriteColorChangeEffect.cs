using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    //TODO: make an attribute that disables things in StepThroughMode instead.
    //NOTE: Sprite colors can be controlled with the MaterialColorChangeEffect,
    //that adjusts the "tint" of the sprite renderer, it's not a perfect solution,
    //but it's less error prone in step through mode.
    /*
    public class SpriteColorChangeEffect : ColorChangeEffect
    {
        public SpriteColorChangeEffect()
        {
            Description = "Sprite Color change Effect allows you to change the color of a sprite using easing.";
        }
        
        private SpriteRenderer sprite;
        
        protected override void SetValue(GameObject target, Color value)
        {
            if(sprite == null) return;
            
            sprite.color = value;
        }

        protected override Color GetValue(GameObject target)
        {
            if(sprite == null) return Color.magenta;
            
            return sprite.color;
        }

        protected override void ExecuteSetup()
        {
            if (sprite == null)
            {
                var renderer = target.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    Debug.LogError("No sprite renderer attached to target.");
                    return;
                }

                sprite = renderer;    
            }
            
            base.ExecuteSetup();
        }
        
        protected override bool TickTween()
        {
            if(sprite == null) return true;
            
            SetValue(target, GameFeelTween.Interpolate(start, elapsed / duration, end, GetEaseFunc()));

            return false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new SpriteColorChangeEffect();
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }
    }
    */
}