using System.Collections.Generic;
using System.Linq;

namespace GameFeelDescriptions
{
    public static class GameFeelExtensions
    {
        /// <summary>
        /// Allows chaining effects together.
        /// <code>
        /// ShakeEffect.OnComplete(BlinkEffect).OnComplete(ExplodeEffect);
        /// </code>
        /// </summary>
        /// <param name="waitFor">The effect to wait for.</param>
        /// <param name="effect">The effect to execute afterwards.</param>
        /// <returns>Returns the added effect, for further chaining.</returns>
        public static GameFeelEffect OnComplete(this GameFeelEffect waitFor, GameFeelEffect effect)
        {
            if (waitFor.ExecuteAfterCompletion == null)
            {
                waitFor.ExecuteAfterCompletion = new List<GameFeelEffect>();        
            }

            waitFor.ExecuteAfterCompletion.Add(effect);
            
            return effect;
        }
        
        /// <summary>
        /// Allows adding a list of effects to be executed after another effect is done.
        /// </summary>
        /// <param name="waitFor"></param>
        /// <param name="effects"></param>
        public static void OnComplete(this GameFeelEffect waitFor, List<GameFeelEffect> effects)
        {
            if (waitFor.ExecuteAfterCompletion == null)
            {
                waitFor.ExecuteAfterCompletion = new List<GameFeelEffect>();        
            }

            waitFor.ExecuteAfterCompletion.AddRange(effects);
        }
        
        public static void QueueExecution(this GameFeelEffect effect)
        {
            GameFeelEffectExecutor.Instance.QueueEffect(effect);
        }

        public static GameFeelEffect CurrentActiveEffect(this GameFeelEffect effect)
        {
            return GameFeelEffectExecutor.Instance.activeEffects.FirstOrDefault(effect.CompareTo);
        }
        
        public static IEnumerable<GameFeelEffect> CurrentActiveEffects(this GameFeelEffect effect)
        {
            return GameFeelEffectExecutor.Instance.activeEffects.Where(effect.CompareTo);
        }
        
        public static void StopExecution(this GameFeelEffect effect)
        {
            GameFeelEffectExecutor.Instance.RemoveEffect(effect);
        }
    }
}