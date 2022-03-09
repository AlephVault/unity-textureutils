using System;
using AlephVault.Unity.TextureUtils.Types;
using UnityEngine;


namespace AlephVault.Unity.TextureUtils
{
    namespace Samples
    {
        public class SampleTextureAssigner : MonoBehaviour
        {
            [SerializeField]
            private Texture2D[] textures;

            [SerializeField]
            private SpriteRenderer[] appliers;

            private int preparedIndex = -1;

            private TexturePool<int, Texture2D> pool = new TexturePool<int, Texture2D>(3);

            private static Rect[] rects = {
                new Rect(0, 32, 32, 32),
                new Rect(32, 32, 32, 32),
                new Rect(0, 0, 32, 32),
                new Rect(32, 0, 32, 32),
            };

            private void Awake()
            {
                if (appliers.Length < 4)
                {
                    Destroy(gameObject);
                    throw new ArgumentException("Please set at least 4 appliers");
                }
            }
            
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    preparedIndex = 0;
                }                
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    preparedIndex = 1;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    preparedIndex = 2;
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    preparedIndex = 3;
                }
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    preparedIndex = -1;
                }
                else
                {
                    bool found = false;
                    for(KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
                    {
                        if (Input.GetKeyDown(key) && preparedIndex != -1)
                        {
                            SpriteRenderer obj = appliers[preparedIndex];
                            int index = (int) key - 48;
                            if (obj.sprite) pool.Release(obj.sprite.texture);
                            Texture2D tex = pool.Use(index, () =>
                            {
                                Debug.Log($"Initializing texture: {index}");
                                return textures[index];
                            }, (texture) =>
                            {
                                Debug.Log($"Shifting texture out: {index}");
                            });
                            obj.sprite = Sprite.Create(tex, rects[preparedIndex], Vector2.zero, 32);                            
                            found = true;
                            preparedIndex = -1;
                            break;
                        }
                    }

                    if (!found)
                    {
                        if (Input.GetKeyDown(KeyCode.Minus) && preparedIndex != -1)
                        {
                            SpriteRenderer obj = appliers[preparedIndex];
                            preparedIndex = -1;
                            Sprite sprite = obj.sprite;
                            obj.sprite = null;
                            if (sprite)
                            {
                                pool.Release(sprite.texture);
                            }
                        }
                    }
                }
            }
        }
    }
}
