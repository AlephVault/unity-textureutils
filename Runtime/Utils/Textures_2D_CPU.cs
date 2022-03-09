using UnityEngine;
using Debug = System.Diagnostics.Debug;


namespace AlephVault.Unity.TextureUtils
{
    namespace Utils
    {
        public partial class Textures
        {
            // CPU implementation of the texture pasting.
            private static void Paste2DCPU(Texture2D target, bool clear = false, params Texture2DSource[] sources)
            {
                Color[] pixels = target.GetPixels();
                if (clear)
                {
                    for (int index = 0; index < pixels.Length; index++)
                    {
                        pixels[index] = new Color(0, 0, 0, 0);
                    }
                }
                
                foreach (var source in sources)
                {
                    Debug.Assert(source.Bounds != null, "source.Bounds != null");
                    RectInt r = source.Bounds.Value;
                    if (r.height == 0 || r.width == 0) continue;

                    Color[] sourcePixels = source.Texture.GetPixels(r.x, r.y, r.width, r.height);
                    for (int index = 0; index < pixels.Length; index++)
                    {
                        if (sourcePixels[index].a > 0)
                        {
                            float a = sourcePixels[index].a + (1 - sourcePixels[index].a) * pixels[index].a;
                            Color c = (sourcePixels[index] * sourcePixels[index].a +
                                       pixels[index] * pixels[index].a * (1 - sourcePixels[index].a)) / a;
                            c.a = a;
                            pixels[index] = c;
                        }
                    }
                }
                target.SetPixels(0, 0, target.width, target.height, pixels);
                target.Apply();
            }
        }
    }
}