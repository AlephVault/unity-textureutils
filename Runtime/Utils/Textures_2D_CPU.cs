using System;
using System.Linq;
using AlephVault.Unity.TextureUtils.Types;
using UnityEngine;


namespace AlephVault.Unity.TextureUtils
{
    namespace Utils
    {
        public partial class Textures
        {
            // CPU implementation of the texture pasting.
            private static void Paste2DCPU(Texture2D target, bool clear = false, params Texture2DSource[] sources)
            {
                // First, get the texture pixels and, if told to clear,
                // then clear the texture.
                Color[] pixels = target.GetPixels();
                if (clear)
                {
                    Color clearWhite = new Color(1, 1, 1, 0);
                    for (int index = 0; index < pixels.Length; index++)
                    {
                        pixels[index] = clearWhite;
                    }
                }
                
                foreach (var source in sources)
                {
                    // First, get the source texture's pixels.
                    Color[] sourcePixels = source.Texture.GetPixels(
                        source.Offset.x, source.Offset.y,
                        target.width, target.height
                    );

                    // Then get the mask. Ont out of two different procedures
                    // will run, depending on whether the mask is present or
                    // not. One is the
                    if (source.Mask == null)
                    {
                        PasteWithoutMask(pixels, sourcePixels);
                    }
                    else
                    {
                        if (source.Mask.filterMode == FilterMode.Point)
                        {
                            PasteWithPointMask(
                                pixels, sourcePixels, source.Mask.GetPixels(),
                                target.width, target.height, 
                                source.Mask.width, source.Mask.height
                            );
                        }
                        else
                        {
                            PasteWithLinearMask(
                                pixels, sourcePixels, source.Mask.GetPixels(),
                                target.width, target.height, 
                                source.Mask.width, source.Mask.height
                            );
                        }
                    }
                }
                target.SetPixels(0, 0, target.width, target.height, pixels);
                target.Apply();
            }

            // Pastes the pixels on top, without applying any mask.
            private static void PasteWithoutMask(Color[] pixels, Color[] sourcePixels)
            {
                for (int index = 0; index < pixels.Length; index++)
                {
                    UpdatePixel(pixels, sourcePixels, index, sourcePixels[index].a);
                }
            }

            // Pastes the pixels on top, applying a mask. If the mask has
            // a different size, no interpolation will be done. Instead,
            // a scaling and rounding-down algorithm will be used to
            // decide the pixel value.
            private static void PasteWithPointMask(
                Color[] pixels, Color[] sourcePixels, Color[] mask,
                int pasteWidth, int pasteHeight, int maskWidth, int maskHeight
            )
            {
                int pasteX = 0;
                int pasteY = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    // Compute the mask coordinates.
                    int maskX = pasteX * maskWidth / pasteWidth;
                    int maskY = pasteY * maskHeight / pasteHeight;

                    // If the source point has alpha, then apply the same
                    // algorithm of the pasting but apply a different alpha,
                    // modified by the mask.
                    float ma = mask[maskY * maskWidth + maskX].r;
                    float sa = ma * sourcePixels[index].a;
                    UpdatePixel(pixels, sourcePixels, index, sa);
                    
                    // Adjust the source coordinates.
                    pasteX += 1;
                    if (pasteX == pasteWidth)
                    {
                        pasteX = 0;
                        pasteY += 1;
                    }
                }
            }

            // Compute the bilinear interpolation.
            private static float DoubleLerp(
                Color[] pixels, int width, int height, float x, float y
            )
            {
                int xDown = (int)Math.Floor(x);
                int yDown = (int)Math.Floor(y);
                int xUp = xDown == x || xDown == width - 1 ? xDown : xDown + 1;
                int yUp = yDown == y || yDown == height - 1 ? yDown : yDown + 1;

                float bl = pixels[yDown * width + xDown].r;
                float br = pixels[yDown * width + xUp].r;
                float tl = pixels[yUp * width + xDown].r;
                float tr = pixels[yUp * width + xUp].r;

                if (xDown == xUp && yDown == yUp)
                {
                    return bl;
                }
                if (xDown == xUp)
                {
                    return Mathf.Lerp(bl, tl, y - yDown);
                }
                if (yDown == yUp)
                {
                    return Mathf.Lerp(bl, br, x - xDown);
                }

                return Mathf.Lerp(
                    Mathf.Lerp(bl, br, x - xDown),
                    Mathf.Lerp(tl, tr, x - xDown),
                    y - yDown
                );
            }
            
            // Pastes the pixels on top, applying a mask. If the mask has
            // a different size, an interpolation will be done. It will
            // use a linear interpolation algorithm.
            private static void PasteWithLinearMask(
                Color[] pixels, Color[] sourcePixels, Color[] mask,
                int pasteWidth, int pasteHeight, int maskWidth, int maskHeight
            )
            {
                int pasteX = 0;
                int pasteY = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    // Compute the mask coordinates.
                    float maskX = (float)pasteX * maskWidth / pasteWidth;
                    float maskY = (float)pasteY * maskHeight / pasteHeight;

                    // If the source point has alpha, then apply the same
                    // algorithm of the pasting but apply a different alpha,
                    // modified by the mask.
                    float ma = DoubleLerp(mask, maskWidth, maskHeight, maskX, maskY);
                    float sa = ma * sourcePixels[index].a;
                    UpdatePixel(pixels, sourcePixels, index, sa);

                    // Adjust the source coordinates.
                    pasteX += 1;
                    if (pasteX == pasteWidth)
                    {
                        pasteX = 0;
                        pasteY += 1;
                    }
                }
            }

            private static void UpdatePixel(Color[] pixels, Color[] sourcePixels, int index, float sourceAlpha)
            {
                if (sourceAlpha > 0)
                {
                    // Something is wrong here - have to fix.
                    float a = Mathf.Lerp(pixels[index].a, 1, sourceAlpha);
                    Color c = Color.Lerp(pixels[index] * pixels[index].a, sourcePixels[index], sourceAlpha) / a;
                    c.a = a;
                    pixels[index] = c;
                }
            }
        }
    }
}