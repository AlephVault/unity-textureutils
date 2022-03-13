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
                Material material = new Material(Shader.Find("Hidden/AlephVault/TextureUtils/Paste"));

                foreach (Texture2DSource source in sources)
                {
                    material.DisableKeyword(clear ? "PASTE_ABOVE" : "CLEAR_PREVIOUS");
                    material.EnableKeyword(clear ? "CLEAR_PREVIOUS" : "PASTE_ABOVE");
                    material.SetTexture("_OverlayTex", source.Texture);
                    material.SetVector("_OverlayOffset", (Vector2)source.Bounds.min);
                    Debug.Log($"Using overlay offset: {(Vector2)source.Bounds.min}");
                    Graphics.Blit(input, output, material);
                    clear = false;
                    (input, output) = (output, input);
                }

                // Now, release one texture, and return the other
                // one (after clearing the graphics source).
                Graphics.SetRenderTarget(null);
                if (target == input)
                {
                    Object.Destroy(output);
                }
                else
                {
                    Graphics.CopyTexture(input, target);
                    Object.Destroy(input);
                }
            }
        }
    }
}