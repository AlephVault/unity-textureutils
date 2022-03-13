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
        /// </summary>
        [Serializable]
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
            public RectInt Bounds;
        }
    }
}