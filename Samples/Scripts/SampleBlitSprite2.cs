using UnityEngine;
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
                // First, the render texture will be created.
                RenderTexture rtexIn = new RenderTexture(144, 192, 32, RenderTextureFormat.ARGB32);
                rtexIn.dimension = TextureDimension.Tex2D;
                
                // Then we do the whole blit loop.
                RenderTexture result = BlitLoop(rtexIn);
                
                // Create the final texture, copy it from the render one, release the
                // render texture, and assign the content of the dumped texture as the
                // new sprite source.
                Debug.Log("Creating final texture");
                Texture2D final = new Texture2D(144, 192, TextureFormat.ARGB32, false);
                Debug.Log("Copying the result texture into the final one");
                Graphics.CopyTexture(result, final);
                Destroy(result);
                Debug.Log("Picking texture -> sprite");
                spriteRenderer.sprite = Sprite.Create(final, new Rect(48, 144, 48, 48), Vector2.zero, 48);
            }

            private RenderTexture BlitLoop(RenderTexture rtexIn)
            { 
                bool first = true;
                // Create an "out" texture, used to flip results, and also
                // create the material with the Paste shader. This material
                // will pass a (0, 0, 0, 0) position (just for this sample)
                // in every case
                Debug.Log("Creating material");
                RenderTexture rtexOut = new RenderTexture(rtexIn);
                Material material = new Material(Shader.Find("Hidden/AlephVault/TextureUtils/Paste"));
                
                foreach (var texture in textures)
                {
                    if (first)
                    {
                        Debug.Log("Blitting first texture");
                        Graphics.Blit(texture, rtexOut);
                        Debug.Log("First texture blitted");
                    }
                    else
                    {
                        // The texture to apply.
                        Debug.Log("Applying _Overlay");
                        material.SetTexture("_OverlayTex", texture);
                        // In a production implementation, both the point and the rects
                        // will be validated considering the target texture's size.
                        //
                        // Finally, the rtexIn is given as source, the rtexOut is given
                        // as destination (both of the same size), and since the overlay
                        // is set to the current texture, the blit will be made. This,
                        // always considering the appropriate rect.
                        Debug.Log("Blitting the previous texture into the new one (with the material)");
                        Graphics.Blit(rtexIn, rtexOut, material);
                        Debug.Log("Next texture blitted");
                    }
                    // Unmark the "first blit" flag, and swap input and output.
                    first = false;
                    (rtexIn, rtexOut) = (rtexOut, rtexIn);
                }

                Graphics.SetRenderTarget(null);
                Destroy(rtexOut);
                return rtexIn;
            }

            private RenderTexture BlitLoop2(RenderTexture rtexIn)
            {
                RenderTexture rtexOut = new RenderTexture(rtexIn);
                Graphics.Blit(textures[0], rtexIn);
                Material material = new Material(Shader.Find("Hidden/AlephVault/TextureUtils/Paste"));
                material.SetVector("_OverlayOffset", new Vector2(0, 0));
                material.SetTexture("_OverlayTex", textures[1]);
                Graphics.Blit(rtexIn, rtexOut, material);
                material.SetTexture("_OverlayTex", textures[2]);
                (rtexOut, rtexIn) = (rtexIn, rtexOut);
                Graphics.Blit(rtexIn, rtexOut, material);
                Graphics.SetRenderTarget(null);
                Destroy(rtexIn);
                return rtexOut;
            }
        }
    }
}