using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class MaterialColorChangeEffect : ColorChangeEffect
    {
        public MaterialColorChangeEffect()
        {
            Description = "Material Color change Effect allows you to change the color of a material using easing.";
            relative = false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new MaterialColorChangeEffect
            {
                materialToModify = materialToModify,
                applyToAllInstances = applyToAllInstances,
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }


        [Header("Finds the first material on the target, and changes the color.", order = 0)]
        [Header("Or use a material reference", order = 1)]
        public Material materialToModify;

        [HideFieldIf("materialToModify", null, true)]
        [Header("Also apply the color to all other instances of the material on the target.")]
        public bool applyToAllInstances;

        private static readonly int ColorPropId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock materialPropertyBlock;

        private MaterialPropertyBlock MaterialPropertyBlock
        {
            get
            {
                if (materialPropertyBlock == null)
                {
                    materialPropertyBlock = new MaterialPropertyBlock();
                }

                return materialPropertyBlock;
            }
        }

        private Renderer renderer;

        protected override void SetValue(GameObject target, Color value)
        {
            if (materialToModify)
            {
                materialToModify.SetColor(ColorPropId, value);
                return;
            }

            if (renderer == null)
            {
                if (target == null) return;

                renderer = target.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    Debug.LogError(
                        "No renderer attached to target. A renderer is required unless using a material reference.");
                    return;
                }
            }

            if (applyToAllInstances)
            {
                renderer.sharedMaterial.SetColor(ColorPropId, value);
            }
            else
            {
                renderer.GetPropertyBlock(MaterialPropertyBlock);
                MaterialPropertyBlock.SetColor(ColorPropId, value);
                renderer.SetPropertyBlock(MaterialPropertyBlock);
            }
        }

        protected override Color GetValue(GameObject target)
        {
            if (materialToModify) return materialToModify.GetColor(ColorPropId);

            if (renderer == null)
            {
                if (target == null) return Color.magenta;

                renderer = target.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    Debug.LogError(
                        "No renderer attached to target. A renderer is required unless using a material reference.");
                    return Color.magenta;
                }
            }

            if (applyToAllInstances)
            {
                return renderer.sharedMaterial.GetColor(ColorPropId);
            }

            if (MaterialPropertyBlock.isEmpty)
            {
                renderer.GetPropertyBlock(MaterialPropertyBlock);
                MaterialPropertyBlock.SetColor(ColorPropId, renderer.sharedMaterial.GetColor(ColorPropId));
            }

            return MaterialPropertyBlock.isEmpty
                ? renderer.sharedMaterial.GetColor(ColorPropId)
                : MaterialPropertyBlock.GetColor(ColorPropId);
        }

        protected override void ExecuteSetup()
        {
            base.ExecuteSetup();

            if (materialToModify) return;

            if (target == null) return;
            
            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning(
                    "No renderer attached to target. A renderer is required unless using a material reference.");
            }
        }

        protected override bool TickTween()
        {
            if (materialToModify == null && (target == null || target.GetComponentInChildren<Renderer>() == null))
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            //TODO: Add relative value change, which would remove the singleton need!!!
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

        public override bool CompareTo(GameFeelEffect other)
        {
            if (other is MaterialColorChangeEffect mat)
            {
                if (!materialToModify) return base.CompareTo(other);

                return mat.materialToModify == materialToModify;
            }

            return false;
        }
    }
}