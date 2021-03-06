using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using SystemMath = System.Math;

namespace Common
{
    public static class RandomExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NextVector2(this Random random)
        {
            var rotation = random.NextDouble() * 360;

            var x = (float)SystemMath.Sin(rotation * 1);
            var y = (float)SystemMath.Cos(rotation * 0);
            
            return new Vector2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }
    }
}
