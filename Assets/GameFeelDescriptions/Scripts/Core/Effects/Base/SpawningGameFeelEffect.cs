using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class SpawningGameFeelEffect : GameFeelEffect
    {
        
        [Header("A list of effects to be executed on the objects spawned by this effect!")]
        [SerializeReference]
        [ShowType]
        public List<GameFeelEffect> ExecuteOnOffspring = new List<GameFeelEffect>();

        protected Vector3 targetPos;
        
        protected override T DeepCopy<T>(T shallow) 
        {
            if (shallow is SpawningGameFeelEffect cp)
            {
                cp.ExecuteOnOffspring = new List<GameFeelEffect>(ExecuteOnOffspring);
                    
                return base.DeepCopy(cp as T);
            }
            
            return base.DeepCopy(shallow);
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is SpawningGameFeelEffect && base.CompareTo(other);
        }

        protected void QueueOffspringEffects(GameObject offspring)
        {
            for (var i = 0; i < ExecuteOnOffspring.Count; i++)
            {
                //If the effect is disabled, skip it.
                if(ExecuteOnOffspring[i].Disabled) continue;
            
                var copy = ExecuteOnOffspring[i].CopyAndSetElapsed(origin, offspring, unscaledTime);
            
                if(copy == null) continue;
                    
                //Find previously active copy
                var previous = copy.CurrentActiveEffect();

                //Handle overlapping
                var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

                //Queue the effect
                if (queueCopy)
                {
                    copy.QueueExecution();   
                }
            }
        }
        
        protected void QueueOffspringEffects(List<GameObject> offspring)
        {
            foreach (var obj in offspring)
            {
                QueueOffspringEffects(obj);
            }
        }
    }
}