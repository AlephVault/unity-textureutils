using System;
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
            ///   A texture source consisting of:
            ///   - A required 2D texture to read from. It may be
            ///     either <see cref="Texture2D"/> or a 2-dimension
            ///     <see cref="RenderTexture"/> (or alternatively
            ///     <see cref="CustomRenderTexture"/>).
            ///   - An optional IntRect to read from (otherwise, the
            ///     (0, 0, width, height) rect will be used instead).
            /// </summary>
            public class Texture2DSource
            {
                /// <summary>
                ///   The texture (2D) to read from.
                /// </summary>
                public Texture2D Texture;

                /// <summary>
                ///   The bounds of the texture. If absent, a full rect
                ///   (i.e. (0, 0, width, height)) will be used instead.
                /// </summary>
                public RectInt? Bounds;
            }

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
                    throw new NotImplementedException(
                        "Passing a RenderTexture is not yet supported. A future version will " +
                        "support this texture type (as it will involve GPU rendering)"
                    );
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
                RectInt defaultRect = new RectInt(0, 0, width, height);
                foreach (Texture2DSource element in sources)
                {
                    if (element.Texture == null || !element.Texture.isReadable)
                    {
                        throw new ArgumentException(
                            "At least one of the sources has a null or non-readable texture"
                        );
                    }

                    RectInt r = element.Bounds ??= defaultRect;
                    if (r.x < 0 || r.y < 0 || r.max.x > width || r.max.y > height)
                    {
                        throw new ArgumentException(
                            $"Invalid rect ({r.x}, {r.y}, {r.width}, {r.height}) when the " +
                            $"size of the target texture is ({width}, {height})"
                        );
                    }
                }
            }
        }
    }
}