using UnityEngine;

public static class Helpers
{
    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        byte r = 255;
        byte g = 255;
        byte b = 255;
        byte a = 255;

        if (hex.Length == 6)
        {
            // RRGGBB
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            // RRGGBBAA
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 3)
        {
            // RGB
            r = byte.Parse(hex[0].ToString() + hex[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex[1].ToString() + hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex[2].ToString() + hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 4)
        {
            // RGBA
            r = byte.Parse(hex[0].ToString() + hex[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex[1].ToString() + hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex[2].ToString() + hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex[3].ToString() + hex[3].ToString(), System.Globalization.NumberStyles.HexNumber);
        }
        else
        {
            Debug.LogError("Invalid hex color format: " + hex);
            return Color.white;
        }

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}