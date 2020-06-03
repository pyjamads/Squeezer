using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{

    [Serializable]
    public class GameFeelRecipe
    {
        /// <summary>
        /// Name of the Game Feel Recipe.
        /// <para>Eg. BounceEffect or LandEffect</para>
        /// </summary>
        public string Name;

        /// <summary>
        /// A description of what the recipe is supposed to do and example application.
        /// </summary>
        public string Description;


        //TODO: Maybe this should have a specific interaction type defined as well. 04/02/2020

        /// <summary>
        /// List of effects to execute.
        /// <para>Note that some of these effects may contain chained effects.</para>
        /// </summary>
        [SerializeReference]
        public List<GameFeelEffect> effects;

        #region Data manipulation

        /// <summary>
        /// Shorthand for accessing the effects in this recipe.
        /// </summary>
        /// <param name="index"></param>
        public GameFeelEffect this[int index]
        {
            get => effects[index];
            set => effects[index] = value;
        }

        /// <summary>
        /// Shorthand for adding effects to the list of effects.
        /// </summary>
        /// <param name="effect"></param>
        public void Add(GameFeelEffect effect)
        {
            effects.Add(effect);
        }

        /// <summary>
        /// Shorthand for removing effects in the list of effects.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            effects.RemoveAt(index);
        }

        #endregion
    }

}