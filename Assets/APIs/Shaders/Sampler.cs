using UnityEngine;

namespace Common.ShaderUtils
{
    public static class Sampler
    {
        /// <summary>
        /// Converts a Gradient to a Texture2D.
        /// </summary>
        public static Texture2D ToTexture(this Gradient gradient, int width)
        {
            if (width <= 1)
            {
                Debug.LogError("Width must be greater than zero.");
                return null;
            }

            Texture2D texture = new Texture2D(width, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;


            for (int i = 0; i < width; i++)
            {
                float t = i / (float)(width - 1);
                texture.SetPixel(i, 0, gradient.Evaluate(t));
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Converts an AnimationCurve to a Texture2D.
        /// </summary>
        public static Texture2D ToTexture(this AnimationCurve curve, int width)
        {
            if (width <= 1)
            {
                Debug.LogError("Width must be greater than zero.");
                return null;
            }

            Texture2D texture = new Texture2D(width, 1, TextureFormat.RFloat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            float[] data = new float[width];

            for (int i = 0; i < width; i++)
            {
                float t = i / (float)(width - 1);
                float value = curve.Evaluate(t);
                data[i] = value;
            }

            texture.SetPixelData(data, 0);
            texture.Apply();
            return texture;
        }
    }
}