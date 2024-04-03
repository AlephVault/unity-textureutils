using AlephVault.Unity.TextureUtils.Types;
using UnityEngine;


namespace AlephVault.Unity.TextureUtils
{
    namespace Utils
    {
        public partial class Textures
        {
            // GPU implementation of the texture pasting. This is
            // done by blit & switch.
            private static void Paste2DGPU(RenderTexture target, bool clear = false, params Texture2DSource[] sources)
            {
                // The first thing to do is to ensure that we have
                // two render textures to perform the process of
                // ((input -> output), switch). One cannot use at
                // the same time the same texture for input and
                // for output (on blit operations).
                RenderTexture input = target;
                RenderTexture output = new RenderTexture(target);
                // And this is the material we will use for the
                // offset-aware pasting.
                foreach (Texture2DSource source in sources)
                {
                    Material material = new Material(Shader.Find("Hidden/AlephVault/TextureUtils/Paste"));
                    material.DisableKeyword(clear ? "PASTE_ABOVE" : "CLEAR_PREVIOUS");
                    material.EnableKeyword(clear ? "CLEAR_PREVIOUS" : "PASTE_ABOVE");
                    material.SetTexture("_OverlayTex", source.Texture);
                    material.SetVector("_OverlayOffset", (Vector2)source.Offset);
                    if (source.Mask) material.SetTexture("_Mask", source.Mask);
                    Graphics.Blit(input, output, material);
                    clear = false;
                    (input, output) = (output, input);
                }

                // Now, release one texture, and return the other
                // one (after clearing the graphics source).
                Graphics.SetRenderTarget(null);
                if (target == input)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        Object.DestroyImmediate(output);
                    }
                    else
                    {
                        Object.Destroy(output);
                    }
#else
                    Object.Destroy(output);
#endif
                }
                else
                {
                    Graphics.CopyTexture(input, target);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        Object.DestroyImmediate(input);
                    }
                    else
                    {
                        Object.Destroy(input);
                    }
#else
                    Object.Destroy(input);
#endif
                }
            }
        }
    }
}