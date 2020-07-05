using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class SpawningGameFeelEffect : GameFeelEffect
    {
        [Header("A list of effects to be executed on the objects spawned by this effect!")]
        public List<GameFeelEffect> ExecuteOnOffspring = new List<GameFeelEffect>();

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
    }
}