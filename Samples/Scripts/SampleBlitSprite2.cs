using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace AlephVault.Unity.TextureUtils
{
    namespace Samples
    {
        [RequireComponent(typeof(SpriteRenderer))]
        public class SampleBlitSprite2 : MonoBehaviour
        {
            private SpriteRenderer spriteRenderer;
            
            [SerializeField]
            private Texture2D[] textures;

            private void Awake()
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            private void Start()
            {
                // First, two render textures are created. One will serve as
                // current input / progress, and another one will serve as
                // blending output. Additionally, a flag to tell whether it
                // is the first merge or not.
                RenderTexture rtexIn = new RenderTexture(144, 192, 32, RenderTextureFormat.ARGB32);
                rtexIn.dimension = TextureDimension.Tex2D;
                RenderTexture rtexOut = new RenderTexture(rtexIn);
                bool first = true;
                foreach (var texture in textures)
                {
                    if (first)
                    {
                        Graphics.Blit(texture, rtexOut);
                    }
                    else
                    {
                        Material material = new Material(Shader.Find("Don't Know Which Shader Should Go Here"));
                        // The texture to apply.
                        material.SetTexture("_Overlay", texture);
                        // The rect, in overlay-space, to use to read from the overlay.
                        // The rect size (width=z, height=w) will be used both for the
                        // overlay input and the source/destination (considering that
                        // both source and destination will have the same size).
                        material.SetVector("_OverlayRect", new Vector4(0, 0, 144, 192));
                        // The point, in source/destination space, to which the data
                        // will be mapped. Taking the zw coordinates from the rect in
                        // overlay space, we make the destination rect.
                        material.SetVector("_DestPoint", new Vector4(0, 0, 0, 0));
                        // In a production implementation, both the point and the rects
                        // will be validated considering the target texture's size.
                        //
                        // Finally, the rtexIn is given as source, the rtexOut is given
                        // as destination (both of the same size), and since the overlay
                        // is set to the current texture, the blit will be made. This,
                        // always considering the appropriate rect.
                        Graphics.Blit(rtexIn, rtexOut, material);
                    }
                    // Unmark the "first blit" flag, and swap input and output.
                    first = false;
                    (rtexIn, rtexOut) = (rtexOut, rtexIn);
                }

                // Create the final texture, copy it from the render one, and assign it.
                Texture2D final = new Texture2D(144, 192, TextureFormat.ARGB32, false);
                Graphics.CopyTexture(rtexIn, final);
                spriteRenderer.sprite = Sprite.Create(final, new Rect(48, 144, 48, 48), Vector2.zero, 48);
            }
        }
    }
}