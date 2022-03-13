using System;
using System.Linq;
using AlephVault.Unity.TextureUtils.Types;
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
            private Texture2DSource[] textures;

            [SerializeField]
            private bool accelerated;

            [SerializeField]
            private Vector2Int cellSize;
            
            [SerializeField]
            private Vector2Int cellCount;

            private void Awake()
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (cellSize.x < 1) cellSize.x = 1;
                if (cellSize.y < 1) cellSize.y = 1;
                if (cellCount.x < 1) cellCount.x = 1;
                if (cellCount.y < 1) cellCount.y = 1;
            }

            private void Start()
            {
                if (!accelerated)
                {
                    Texture2D tex2d = new Texture2D(
                        cellSize.x * cellCount.x, cellSize.y * cellCount.y,
                        TextureFormat.ARGB32, true
                    );
                    Utils.Textures.Paste2D(tex2d, true, textures);
                    spriteRenderer.sprite = Sprite.Create(tex2d, new Rect(
                        cellSize.x, cellSize.y * 3, 
                        cellSize.x, cellSize.y
                    ), Vector2.zero, 48);
                }
                else
                {
                    Texture2D tex2d = new Texture2D(
                        cellSize.x * cellCount.x, cellSize.y * cellCount.y,
                        TextureFormat.ARGB32, false
                    );
                    RenderTexture rtex = new RenderTexture(
                        cellSize.x * cellCount.x, cellSize.y * cellCount.y, 32,
                        RenderTextureFormat.ARGB32, 0
                    );
                    Utils.Textures.Paste2D(rtex, true, textures);
                    Graphics.CopyTexture(rtex, tex2d);
                    spriteRenderer.sprite = Sprite.Create(tex2d, new Rect(
                        cellSize.x, cellSize.y * 3, 
                        cellSize.x, cellSize.y
                    ), Vector2.zero, 48);
                }
            }
        }
    }
}