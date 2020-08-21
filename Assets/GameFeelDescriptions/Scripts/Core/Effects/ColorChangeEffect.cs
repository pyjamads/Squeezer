using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class ColorChangeEffect : TweenEffect<Color>
    {
        public override void Randomize()
        {
            //10% chance to setFromValue 
            setFromValue = RandomExtensions.Boolean(0.1f);

            
            if (setFromValue)
            {
                @from = Random.ColorHSV().withA(Random.value);;
            }
            else
            {
                @from = Color.clear;
            }

            //No chance to set the relative value to true, because it's kinda weird with colors.
            //relative = RandomExtensions.Boolean(0.05f);
            relative = false;
            
            to = Random.ColorHSV().withA(Random.value);
            
            easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});
            
            //NOTE: curve is excluded here.

            loopType = EnumExtensions.GetRandomValue<LoopType>();
            repeat = Random.Range(-1, 3);

            base.Randomize();
            
            //NOTE: Need that SetElapsed in DurationalGameFeelEffect to be run first.
            SetupLooping();
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean(amount))
            {
                setFromValue = !setFromValue;
            }

            //Make a random color, and add/subtract a proportional amount here.
            @from += RandomExtensions.Sign(0.5f) * amount * Random.ColorHSV().withA(Random.value);
        
            //Make a random color, and add/subtract a proportional amount here.
            to += RandomExtensions.Sign(0.5f) * amount * Random.ColorHSV().withA(Random.value);

            if (RandomExtensions.Boolean(amount))
            {
                easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});
            }

            if (RandomExtensions.Boolean(amount))
            {
                loopType = EnumExtensions.GetRandomValue<LoopType>();
            }

            if (RandomExtensions.Boolean(amount))
            {
                repeat = Random.Range(-1, 3);
            }

            base.Mutate(amount);
            
            //NOTE: Need that SetElapsed in DurationalGameFeelEffect to be run first.
            SetupLooping();
        }

        protected override Color GetRelativeValue(Color fromValue, Color addValue)
        {
            //NOTE: This is a bit odd, maybe just roll over the Hue,
            //and simply add or average the Saturation/Value 2020-08-19
            // float H1, S1, V1;
            // Color.RGBToHSV(fromValue, out H1, out S1, out  V1);
            //
            // float H2, S2, V2;
            // Color.RGBToHSV(addValue, out H2, out S2, out  V2);
            //
            // var col = Color.HSVToRGB((H1 + H2) % 1, (S1 + S2)% 1, (V1 + V2)% 1);
            // return col.withA(Mathf.Clamp01(fromValue.a + addValue.a));

            //NOTE: this simple addition, makes all relative adjustments move towards white.
            return fromValue + addValue;
            
            // var r = (fromValue.r + addValue.r) % 1;
            // var g = (fromValue.g + addValue.g) % 1;
            // var b = (fromValue.b + addValue.b) % 1;
            // var alpha = Mathf.Clamp01(fromValue.a + addValue.a);
            // //new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
            // return new Color(r,g,b, alpha);
        }

        protected override Color GetDifference(Color fromValue, Color toValue)
        {
            //NOTE: This is a bit odd, maybe just roll over the Hue,
            //and simply add or average the Saturation/Value 2020-08-19
            // float H1, S1, V1;
            // Color.RGBToHSV(fromValue, out H1, out S1, out  V1);
            //
            // float H2, S2, V2;
            // Color.RGBToHSV(toValue, out H2, out S2, out  V2);
            //
            // var col = Color.HSVToRGB((H1 - H2) % 1, (S1 - S2) % 1, (V1 - V2) % 1);
            // return col.withA(Mathf.Clamp01(fromValue.a - toValue.a)); 

            return toValue - fromValue;

            // var r = Mathf.Abs(toValue.r - fromValue.r);
            // var g = Mathf.Abs(toValue.g - fromValue.g);
            // var b = Mathf.Abs(toValue.b - fromValue.b);
            // var alpha = (toValue.a - fromValue.a);
            //
            // return new Color(r,g,b,alpha);
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is ColorChangeEffect && other.target == target;
        }
    }
}