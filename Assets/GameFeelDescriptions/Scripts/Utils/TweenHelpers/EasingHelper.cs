using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class EasingHelper
    {
        public static AnimationCurve Ease2Curve(EaseType ease, int keyFrameCount = 20)
        {
            //TODO: make easing evaluation better 10/02/2020 (made the curve better at least 18/05/2020)
            var func = Ease(ease);
            var curve = new AnimationCurve();
            var keyframes = new Keyframe[keyFrameCount+1];

            for (int i = 0; i < keyFrameCount; i++)
            {
                var t = i / (float) keyFrameCount;
                keyframes[i] = new Keyframe(t, func(t));
                curve.AddKey(keyframes[i]);
            }
            
            //Also generate the last point on the curve.
            keyframes[keyFrameCount] = new Keyframe(1, func(1));
            curve.AddKey(keyframes[keyFrameCount]);

            for (int i = 0; i < keyFrameCount+1; i++)
            {
	            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
	            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            return curve; //new AnimationCurve(keyframes);
        }
        
        //Default Implementation is linear.
        public enum EaseType
        {
            Linear,

            SineIn,
            SineOut,
            SineInOut,

            QuadIn,
            QuadOut,
            QuadInOut,

//            CubicIn,
//            CubicOut,
//            CubicInOut,
//
//            QuartIn,
//            QuartOut,
//            QuartInOut,
//
//            QuintIn,
//            QuintOut,
//            QuintInOut,

            ExpoIn,
            ExpoOut,
            ExpoInOut,

//            CircIn,
//            CircOut,
//            CircInOut,
//
//            ElasticIn,
//            ElasticOut,
//            ElasticInOut,

            BackIn,
            BackOut,
            BackInOut,

//            BounceIn,
//            BounceOut,
//            BounceInOut
	        Curve,
	        
        }

        public static Func<float, float> Ease(EaseType ease)
        {
            switch (ease)
            {
                case EaseType.Linear:
                    return Linear;
                case EaseType.SineIn:
                    return SineIn;
                case EaseType.SineOut:
                    return SineOut;
                case EaseType.SineInOut:
                    return SineInOut;
                case EaseType.QuadIn:
                    return QuadIn;
                case EaseType.QuadOut:
                    return QuadOut;
                case EaseType.QuadInOut:
                    return QuadInOut;
//                case EaseType.CubicIn:
//                    break;
//                case EaseType.CubicOut:
//                    break;
//                case EaseType.CubicInOut:
//                    break;
//                case EaseType.QuartIn:
//                    break;
//                case EaseType.QuartOut:
//                    break;
//                case EaseType.QuartInOut:
//                    break;
//                case EaseType.QuintIn:
//                    break;
//                case EaseType.QuintOut:
//                    break;
//                case EaseType.QuintInOut:
//                    break;
                case EaseType.ExpoIn:
                    return ExpoIn;
                case EaseType.ExpoOut:
                    return ExpoOut;
                case EaseType.ExpoInOut:
                    return ExpoInOut;
//                case EaseType.CircIn:
//                    break;
//                case EaseType.CircOut:
//                    break;
//                case EaseType.CircInOut:
//                    break;
//                case EaseType.ElasticIn:
//                    break;
//                case EaseType.ElasticOut:
//                    break;
//                case EaseType.ElasticInOut:
//                    break;
                case EaseType.BackIn:
                    return BackIn;
                case EaseType.BackOut:
                    return BackOut;
                case EaseType.BackInOut:
                    return BackInOut;
//                case EaseType.BounceIn:
//                    break;
//                case EaseType.BounceOut:
//                    break;
//                case EaseType.BounceInOut:
//                    break;
                default:
                    Debug.Log(ease.GetName()+": Not yet implemented, using Linear instead.");
                    return Linear;
            }
        }
        
        public static float Linear(float t)
        {
            t = Mathf.Clamp01(t);
	        
            return t;
        }

        public static float QuadIn(float t)
        {
            t = Mathf.Clamp01(t);
	        
            return t * t;   
        }
        
        public static float QuadOut(float t)
        {
            t = Mathf.Clamp01(t);
	        
            return 1f - QuadIn(1f - t);
        }
       
        public static float QuadInOut(float t)
        {
            t = Mathf.Clamp01(t);
	        
            if (t < 0.5f)
            {
                return QuadIn(t * 2f) / 2f;
            }

            return 0.5f + QuadOut((t - 0.5f) * 2f) / 2f;
        }

        public static float SineIn(float t)
        {
            t = Mathf.Clamp01(t);

            return 1f - SineOut(1f - t);
        }
        
        public static float SineOut(float t)
        {
            t = Mathf.Clamp01(t);

            return Mathf.Sin((t * Mathf.PI) / 2);
        }
        
        

        public static float SineInOut(float t)
        {
            t = Mathf.Clamp01(t);

            if (t < 0.5f)
            {
                return SineIn(t * 2f) / 2f;
            }

            return 0.5f + SineOut((t - 0.5f) * 2f) / 2f;
        }

        public static float ExpoIn(float t)
        {
            t = Mathf.Clamp01(t);

            return t == 0f ? 0f : Mathf.Pow(2, 10 * t - 10);
        }
        
        public static float ExpoOut(float t)
        {
            t = Mathf.Clamp01(t);

            return t == 1f ? 1f : 1f - Mathf.Pow(2, -10 * t);
        }

        public static float ExpoInOut(float t)
        {
            t = Mathf.Clamp01(t);

            if (t < 0.5f)
            {
                return ExpoIn(t * 2f) / 2f;
            }

            return 0.5f + ExpoOut((t - 0.5f) * 2f) / 2f;
        }
        
        public static float BackIn(float t)
        {
            return BezierEvaluator(new Vector2[3] {Vector2.zero, new Vector2(0.25f,-0.5f), Vector2.one}, t).y;
        }
	    
        public static float BackOut(float t)
        {
            return BezierEvaluator(new Vector2[3] {Vector2.zero, new Vector2(0.75f,1.5f), Vector2.one}, t).y;
        }

        public static float BackInOut(float t)
        {
            return SimpleBezierEvaluator(new Vector2(0.25f,-0.5f), t,new Vector2(0.75f,1.5f));
        }
        

        /// <summary>
        /// Simple Bezier curve evaluator with (0,0) and (1,1) as endpoints, and two control points.
        /// <para>Finding an easing value between (0,0) and (1,1).</para>
        /// </summary>
        /// <param name="control1">Control point 1, controls the direction out of (0,0)</param>
        /// <param name="t">The evaluation point on the time axis</param>
        /// <param name="control2">Control point 2, controls the direction into (1,1)</param>
        /// <returns></returns>
        private static float SimpleBezierEvaluator(Vector2 control1, float t, Vector2 control2)
        {
            //Clamp value between 0 and 1.
            t = Mathf.Clamp01(t);

            return BezierEvaluator(new Vector2[4] {Vector2.zero, control1, control2, Vector2.one}, t).y;
        }

        /// <summary>
        /// Bezier Evaluator, that finds a point on a Bezier Easing curve.
        /// </summary>
        /// <param name="points">List of points to evaluate, minimum 2 points.</param>
        /// <param name="t">The interpolation value.</param>
        /// <returns>Return a point interpolated between the input points.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Vector2 BezierEvaluator(IReadOnlyList<Vector2> points, float t)
        {
            if (points.Count < 2)
            {
                throw new ArgumentOutOfRangeException("BezierEvaluator needs at least two points to work.");
            }

            if (points.Count > 2)
            {
                var intermediatePoints = new Vector2[points.Count - 1];

                for (var index = 0; index < points.Count - 1; index++)
                {
                    var point1 = points[index];
                    var point2 = points[index+1];

                    intermediatePoints[index] = point1 + (point2 - point1) * t;
                }

                return BezierEvaluator(intermediatePoints, t);
            }
            
            //Evaluate the last couple of points, and return the y value of the last point.
            return points[0] + (points[1] - points[0]) * t;
        }
        
        
        /*
         NOTE: easings.net code for easing functions, these are probably available in many other places, but easings.net is under GPL 3.0, so probably a fine licence?.
         
         const pow = Math.pow;
const sqrt = Math.sqrt;
const sin = Math.sin;
const cos = Math.cos;
const PI = Math.PI;
const c1 = 1.70158;
const c2 = c1 * 1.525;
const c3 = c1 + 1;
const c4 = (2 * PI) / 3;
const c5 = (2 * PI) / 4.5;

function bounceOut(x: number) {
	const n1 = 7.5625;
	const d1 = 2.75;

	if (x < 1 / d1) {
		return n1 * x * x;
	} else if (x < 2 / d1) {
		return n1 * (x -= 1.5 / d1) * x + 0.75;
	} else if (x < 2.5 / d1) {
		return n1 * (x -= 2.25 / d1) * x + 0.9375;
	} else {
		return n1 * (x -= 2.625 / d1) * x + 0.984375;
	}
}

const easingsFunctions: any = {
//	easeInQuad(x: number) {
//		return x * x;
//	},
//	easeOutQuad(x: number) {
//		return 1 - (1 - x) * (1 - x);
//	},
	easeInOutQuad(x: number) {
		return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
	},
	easeInCubic(x: number) {
		return x * x * x;
	},
	easeOutCubic(x: number) {
		return 1 - pow(1 - x, 3);
	},
	easeInOutCubic(x: number) {
		return x < 0.5 ? 4 * x * x * x : 1 - pow(-2 * x + 2, 3) / 2;
	},
	easeInQuart(x: number) {
		return x * x * x * x;
	},
	easeOutQuart(x: number) {
		return 1 - pow(1 - x, 4);
	},
	easeInOutQuart(x: number) {
		return x < 0.5 ? 8 * x * x * x * x : 1 - pow(-2 * x + 2, 4) / 2;
	},
	easeInQuint(x: number) {
		return x * x * x * x * x;
	},
	easeOutQuint(x: number) {
		return 1 - pow(1 - x, 5);
	},
	easeInOutQuint(x: number) {
		return x < 0.5 ? 16 * x * x * x * x * x : 1 - pow(-2 * x + 2, 5) / 2;
	},
//	easeInSine(x: number) {
//		return 1 - cos((x * PI) / 2);
//	},
//	easeOutSine(x: number) {
//		return sin((x * PI) / 2);
//	},
	easeInOutSine(x: number) {
		return -(cos(PI * x) - 1) / 2;
	},
//	easeInExpo(x: number) {
//		return x === 0 ? 0 : pow(2, 10 * x - 10);
//	},
//	easeOutExpo(x: number) {
//		return x === 1 ? 1 : 1 - pow(2, -10 * x);
//	},
	easeInOutExpo(x: number) {
		return x === 0
			? 0
			: x === 1
			? 1
			: x < 0.5
			? pow(2, 20 * x - 10) / 2
			: (2 - pow(2, -20 * x + 10)) / 2;
	},
	easeInCirc(x: number) {
		return 1 - sqrt(1 - pow(x, 2));
	},
	easeOutCirc(x: number) {
		return sqrt(1 - pow(x - 1, 2));
	},
	easeInOutCirc(x: number) {
		return x < 0.5
			? (1 - sqrt(1 - pow(2 * x, 2))) / 2
			: (sqrt(1 - pow(-2 * x + 2, 2)) + 1) / 2;
	},
	easeInBack(x: number) {
		return c3 * x * x * x - c1 * x * x;
	},
	easeOutBack(x: number) {
		return 1 + c3 * pow(x - 1, 3) + c1 * pow(x - 1, 2);
	},
	easeInOutBack(x: number) {
		return x < 0.5
			? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
			: (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
	},
	easeInElastic(x: number) {
		return x === 0
			? 0
			: x === 1
			? 1
			: -pow(2, 10 * x - 10) * sin((x * 10 - 10.75) * c4);
	},
	easeOutElastic(x: number) {
		return x === 0
			? 0
			: x === 1
			? 1
			: pow(2, -10 * x) * sin((x * 10 - 0.75) * c4) + 1;
	},
	easeInOutElastic(x: number) {
		return x === 0
			? 0
			: x === 1
			? 1
			: x < 0.5
			? -(pow(2, 20 * x - 10) * sin((20 * x - 11.125) * c5)) / 2
			: (pow(2, -20 * x + 10) * sin((20 * x - 11.125) * c5)) / 2 + 1;
	},
	easeInBounce(x: number) {
		return 1 - bounceOut(1 - x);
	},
	easeOutBounce: bounceOut,
	easeInOutBounce(x: number) {
		return x < 0.5
			? (1 - bounceOut(1 - 2 * x)) / 2
			: (1 + bounceOut(2 * x - 1)) / 2;
	},
}; 
         */
        
        
        
    }
}