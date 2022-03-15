using System;
using UnityEngine;


namespace AlephVault.Unity.TextureUtils
{
    namespace Types
    {
        /// <summary>
        ///   A texture source consisting of:
        ///   - A required 2D texture to read from. It may be
        ///     either <see cref="Texture2D"/> or a 2-dimension
        ///     <see cref="RenderTexture"/> (or alternatively
        ///     <see cref="CustomRenderTexture"/>).
        ///   - An optional IntRect to read from (otherwise, the
        ///     (0, 0, width, height) rect will be used instead).
        ///   - An optional mask, ideally single-channel but
        ///     anyway it will only take the red channel.
        /// </summary>
        [Serializable]
        public class Texture2DSource
        {
            /// <summary>
            ///   The texture (2D) to read from.
            /// </summary>
            public Texture2D Texture;

            /// <summary>
            ///   The offset to read the source texture from.
            ///   If absent, (0, 0) will be used.
            /// </summary>
            public Vector2Int Offset;

            /// <summary>
            ///   The texture (2D) to use as a mask. Ensure that,
            ///   when set, the mask is of the same pixel size as
            ///   the target texture to render into, otherwise it
            ///   will be stretched.
            /// </summary>
            public Texture2D Mask;
        }
    }
}