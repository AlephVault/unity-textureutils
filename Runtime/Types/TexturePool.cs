using System;
using System.Collections;
using System.Collections.Generic;
using AlephVault.Unity.Support.Generic.Authoring.Types;
using UnityEngine;


namespace AlephVault.Unity.TextureUtils
{
    namespace Types
    {
        public class TexturePool<KeyType, TexType> where TexType : Texture
        {
            // A tracking struct for the registered instances.
            // Tracks instance, key, reference count and what
            // to do when the entry is shifted out of the pool.
            private class TexTracking
            {
                public KeyType key;
                public TexType texture;
                public uint refCount;
                public Action<TexType> onShifted;
            }
            
            // A mapping class which tells texture status by key.
            private class InstanceMappingByKey : System.Collections.Generic.Dictionary<KeyType, TexTracking> {}

            // A mapping instance which keeps alive instances by their key.
            private InstanceMappingByKey instanceByKey = new InstanceMappingByKey();

            // A mapping class which tells texture status by texture.
            private class InstanceMappingByRef : System.Collections.Generic.Dictionary<Texture, TexTracking> {}

            // A mapping instance which keeps alive instances by their reference.
            private InstanceMappingByRef instanceByRef = new InstanceMappingByRef();

            // A last-second rescue class, implemented as an ordered set.
            // (this, to get the last element(s) and also in an indexed manner).
            private class LastSecondRescue : OrderedSet<TexTracking>
            {
                // Removes the first elements, keeping a given size.
                public IEnumerable<TexTracking> ShiftUntil(int size)
                {
                    while (Count > size)
                    {
                        TexTracking entry = Shift();
                        entry?.onShifted(entry.texture);
                        yield return entry;
                    }
                }
            }

            // A last-second instance which keeps the references that were totally
            // released in calls to Release. They may be used again from this list
            // without being garbage-collected.
            private LastSecondRescue lastSecondRescue = new LastSecondRescue();

            /// <summary>
            ///   The length of the last-second rescue list. This value can be changed
            ///   later and only takes effect when an instance is totally released.
            /// </summary>
            public int LastSecondRescueSize;

            /// <summary>
            ///   Creates a texture pool with a given size.
            /// </summary>
            /// <param name="lastSecondRescueSize">The size for the pool</param>
            public TexturePool(int lastSecondRescueSize)
            {
                LastSecondRescueSize = lastSecondRescueSize;
            }

            /// <summary>
            ///   Creates a texture pool with a pool size of 20.
            /// </summary>
            public TexturePool() : this(20) {}

            // Clears all the references from the pool, both dead and alive.
            // This, because textures cannot be defined themselves to self
            // manage their death with the pool-relevant logic, so the logic
            // must be invoked here, on pool destruction.
            ~TexturePool()
            {
                // Dead pool (!!!! XD) references.
                foreach (TexTracking tracking in lastSecondRescue.ShiftUntil(0))
                {
                    instanceByKey.Remove(tracking.key);
                    instanceByRef.Remove(tracking.texture);
                }

                // Alive references.
                foreach (TexTracking tracking in instanceByKey.Values)
                {
                    instanceByRef.Remove(tracking.texture);
                    tracking.onShifted?.Invoke(tracking.texture);
                }
                instanceByKey.Clear();
            }

            /// <summary>
            ///   <para>
            ///     Attempts to retrieve an existing texture (by its key) or create/load
            ///     a new texture (giving its key). An <see cref="onAbsent"/> function
            ///     is required to create the texture if no texture exists by the supplied
            ///     key. A key is always mandatory, and must not be null.
            ///   </para>
            ///   <para>
            ///     Typically, <see cref="onAbsent"/> will create a texture when none
            ///     exists for the provided key, and <see cref="onShifted"/> will drop
            ///     the texture (e.g. destroying it) when not needed anymore at all.
            ///     The <see cref="onShifted"/> callback will be kept, as tied to the
            ///     texture, for when the texture is removed from the pool and not
            ///     referenced anymore.
            ///   </para>
            /// </summary>
            /// <param name="key">The key to use to create a new, or recover an existing, texture</param>
            /// <param name="onAbsent">
            ///   The function used to add a new texture into the pool (new or loaded from
            ///   somewhere else) when a texture is not present for the given key.
            /// </param>
            /// <param name="onShifted">
            ///   The function used to acknowledge when the texture is being completely
            ///   shifted out of the pool. Typically, a dynamic texture is destroyed by
            ///   this point. This is the opposite of <see cref="onAbsent"/>. This one
            ///   will be kept until the texture is removed from the pool, and will be
            ///   ignored if a texture already exists with the given key.
            /// </param>
            /// <returns>The corresponding texture</returns>
            public TexType Use(KeyType key, Func<TexType> onAbsent, Action<TexType> onShifted)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                
                // First, check whether the instance exists. If that's the case,
                // retrieve it. Otherwise, the instance may exist but means to
                // reconnect it to the pool will not, so it will not exist by
                // the specified key, and has to be created.
                if (instanceByKey.TryGetValue(key, out TexTracking existingTexture))
                {
                    if (existingTexture.refCount <= 0)
                    {
                        existingTexture.refCount = 1;
                        lastSecondRescue.Remove(existingTexture);
                    }
                    else
                    {
                        existingTexture.refCount++;
                    }
                    return existingTexture.texture;
                }
                
                // By this point, it is an error for the onAbsent function to
                // be null, since it will be invoked, the texture retrieved,
                // and linked to the key. If TexType is RenderTexture, the user
                // must ensure that the texture's dimensionality is appropriate.
                // Otherwise, the instance is created and stored. Also, a warning
                // is issued when the onShifted function is null.
                if (onAbsent == null) throw new ArgumentNullException(nameof(onAbsent));
                TexType newTexture = onAbsent();
                if (onShifted == null) Debug.LogWarning(
                    "A value for onShifted was not provided while adding a new " +
                    "texture into the pool. This might be an error. If you don't want " +
                    "to pass a function, be sore you know what you are doing to release " +
                    "this texture when not referenced anymore and pass an empty function " +
                    "as an argument to this call"
                );
                TexTracking tracking = new TexTracking
                {
                    texture = newTexture, onShifted = onShifted, refCount = 1, key = key
                };
                instanceByKey[key] = tracking;
                instanceByRef[newTexture] = tracking;
                return newTexture;
            }

            // Decrements the reference count of a tracking and, perhaps, keeps
            // it in the last-second rescue pool.
            private void DecrementRefCount(TexTracking tracking)
            {
                tracking.refCount--;
                if (tracking.refCount == 0)
                {
                    lastSecondRescue.Add(tracking);
                    foreach (TexTracking removedTracking in lastSecondRescue.ShiftUntil(LastSecondRescueSize))
                    {
                        instanceByKey.Remove(removedTracking.key);
                        instanceByRef.Remove(removedTracking.texture);
                    }
                }
            }

            /// <summary>
            ///   Releases a texture. If the texture has no more references, then
            ///   it is added into the pool. Finally, the pool is perhaps shifted
            ///   to remove trailing elements, out of the max capacity.
            /// </summary>
            /// <param name="key">The key to release, if present, a texture by</param>
            /// <returns>false if the key is not present</returns>
            public bool Release(KeyType key)
            {
                if (instanceByKey.TryGetValue(key, out TexTracking tracking) && tracking.refCount > 0)
                {
                    DecrementRefCount(tracking);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///   Releases a texture. If the texture has no more references, then
            ///   it is added into the pool. Finally, the pool is perhaps shifted
            ///   to remove trailing elements, out of the max capacity.
            /// </summary>
            /// <param name="texture">The texture reference to release, if present</param>
            /// <returns>false if the texture is not present</returns>
            public bool Release(TexType texture)
            {
                if (instanceByRef.TryGetValue(texture, out TexTracking tracking) && tracking.refCount > 0)
                {
                    DecrementRefCount(tracking);
                    return true;
                }

                return false;
            }
        }
    }
}