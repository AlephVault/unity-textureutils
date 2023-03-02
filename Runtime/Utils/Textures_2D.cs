using System;
using AlephVault.Unity.TextureUtils.Types;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;


namespace AlephVault.Unity.TextureUtils
{
    namespace Utils
    {
        /// <summary>
        ///   Several utils to deal with textures (e.g. pasting many).
        /// </summary>
        public static partial class Textures
        {
            /// <summary>
            ///   Pastes several textures into one target, 2D, texture.
            /// </summary>
            /// <param name="target">The target texture to paste stuff from</param>
            /// <param name="clear">Whether to previously clear the target, or paste on top</param>
            /// <param name="sources">The textures to paste, and their rects</param>
            public static void Paste2D(Texture target, bool clear = false, params Texture2DSource[] sources)
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                if (sources == null)
                {
                    throw new ArgumentNullException(nameof(sources));
                }
                
                if (target is Texture2D target2d)
                {
                    ValidateSources(target2d.width, target2d.height, sources);
                    Paste2DCPU(target2d, clear, sources);
                }
                else if (target is RenderTexture rtex && rtex.dimension == TextureDimension.Tex2D)
                {
                    ValidateSources(rtex.width, rtex.height, sources);
                    Paste2DGPU(rtex, clear, sources);
                }
                else
                {
                    throw new NotSupportedException(
                        "Target texture types other than Texture2D or RenderTexture(dimensions=2D) " +
                        "are not supported by this method"
                    );
                }
            }

            // Validates the input rects of each source.
            private static void ValidateSources(int width, int height, Texture2DSource[] sources)
            {
                foreach (Texture2DSource element in sources)
                {
                    if (element.Texture == null || !element.Texture.isReadable)
                    {
                        throw new ArgumentException(
                            $"At least one of the sources has a null or non-readable texture: {element}"
                        );
                    }
                    
                    if (element.Offset.x < 0 || element.Offset.y < 0 || element.Offset.x + width > element.Texture.width
                        || element.Offset.y + height > element.Texture.height)
                    {
                        throw new ArgumentException(
                            $"Invalid source texture rect ({element.Offset.x}, {element.Offset.y}, {width}, " +
                            $"{height}) when the size of the source texture is ({element.Texture.width}, " +
                            $"{element.Texture.height})"
                        );
                    }

                    if (element.Mask != null)
                    {
                        switch (element.Mask.format)
                        {
                            case TextureFormat.R8:
                            case TextureFormat.R16:
                            case TextureFormat.BC4:
                            case TextureFormat.RHalf:
                            case TextureFormat.RFloat:
                            case TextureFormat.EAC_R:
                            case TextureFormat.EAC_R_SIGNED:
                                break;
                            default:
                                Debug.LogWarning(
                                    "The mask texture is not single-channel. Please note that only the red " +
                                    "channel will be used for masking purposes."
                                );
                                break;
                        }

                        if (element.Mask.width != width || element.Mask.height != height)
                        {
                            Debug.LogWarning(
                                "The dimensions for mask do not match the dimensions for " +
                                "the render target. Stretching will occur"
                            );
                        }
                    }
                }
            }
        }
    }
}