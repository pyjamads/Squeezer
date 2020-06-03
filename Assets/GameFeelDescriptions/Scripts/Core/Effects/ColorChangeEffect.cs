using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class ColorChangeEffect : TweenEffect<Color>
    {
        protected override Color GetRelativeValue(Color fromValue, Color addValue)
        {
            return fromValue + addValue;
        }

        protected override Color GetDifference(Color fromValue, Color toValue)
        {
            return toValue - fromValue;
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is ColorChangeEffect && other.target == target;
        }
    }
}