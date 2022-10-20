using UnityEngine;

namespace KEEK.TAS; 

internal static class VectorExtensions {
    public static string ToSimpleString(this Vector3 vector3, int decimals = 2) {
        return $"{vector3.x.ToFormattedString(decimals)}, {vector3.y.ToFormattedString(decimals)}";
    }
}

internal static class NumberExtensions {
    public static string ToFormattedString(this float value, int decimals = 2) {
        return ((double) value).ToFormattedString(decimals);
    }

    public static string ToFormattedString(this double value, int decimals = 2) {
        return value.ToString($"F{decimals}");
    }
}