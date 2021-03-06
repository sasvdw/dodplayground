using System.Runtime.CompilerServices;

namespace Common
{
    public static class MathF
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Select(float x, float y, bool predicate)
        {
            return predicate ? y : x;
        }
    }
}
