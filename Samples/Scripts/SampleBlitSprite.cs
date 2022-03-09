using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


namespace AlephVault.Unity.TextureUtils
{
    namespace Samples
    {
        [RequireComponent(typeof(SpriteRenderer))]
        public class SampleBlitSprite : MonoBehaviour
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
                // I'm not sure about the performance of doing this.
                // The algorithm works, but I need a more performant
                // way to do this, if any. I did not stress-test this
                // solution, even when the pooling system may mitigate
                // a good part of the impact. Perhaps there is a more
                // performant way of doing this.
                //
                // For this script to work, all the textures in the
                // array must be either created dynamically or marked
                // as "Read/Write enabled" while importing the texture
                // in the TextureImporter Editor settings.
                Texture2D tex2d = new Texture2D(144, 192, TextureFormat.ARGB32, false);
                Utils.Textures.Paste2D(tex2d, true, (from texture in textures select new Utils.Textures.Texture2DSource {
                    Texture = texture
                }).ToArray());
                spriteRenderer.sprite = Sprite.Create(tex2d, new Rect(48, 144, 48, 48), Vector2.zero, 48);
            }
        }
    }
}