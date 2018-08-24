// ----------------------------------------------------------------------------
// The MIT License
// Reactive behaviour for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Leopotam.Ecs.Reactive {
    /// <summary>
    /// Type of reaction.
    /// </summary>
    public enum EcsReactiveType {
        OnAdded,
        OnRemoved
    }

    /// <summary>
    /// Base class for all reactive systems.
    /// </summary>
    public abstract class EcsReactiveSystemBase : IEcsFilterReactiveListener, IEcsPreInitSystem, IEcsRunSystem {
        public int[] ReactedEntities = new int[32];
        public int ReactedEntitiesCount;
        EcsReactiveType _reactType;

        void IEcsPreInitSystem.PreInitialize () {
            _reactType = GetReactiveType ();
            GetFilter ().AddListener (this);
        }

        void IEcsPreInitSystem.PreDestroy () {
            ReactedEntitiesCount = 0;
            GetFilter ().RemoveListener (this);
        }

        void IEcsRunSystem.Run () {
            if (ReactedEntitiesCount > 0) {
                RunReactive ();
            }
            ReactedEntitiesCount = 0;
        }

        void IEcsFilterReactiveListener.OnEntityAdded (int entity) {
            if (_reactType == EcsReactiveType.OnAdded) {
                if (ReactedEntities.Length == ReactedEntitiesCount) {
                    Array.Resize (ref ReactedEntities, ReactedEntitiesCount << 1);
                }
                ReactedEntities[ReactedEntitiesCount++] = entity;
            }
        }

        void IEcsFilterReactiveListener.OnEntityRemoved (int entity) {
            if (_reactType == EcsReactiveType.OnRemoved) {
                if (ReactedEntities.Length == ReactedEntitiesCount) {
                    Array.Resize (ref ReactedEntities, ReactedEntitiesCount << 1);
                }
                ReactedEntities[ReactedEntitiesCount++] = entity;
            }
        }

        /// <summary>
        /// Returns EcsFilterReactive instance for watching on it.
        /// </summary>
        protected abstract IEcsFilterReactive GetFilter ();

        /// <summary>
        /// Returns reactive type behaviour.
        /// </summary>
        protected abstract EcsReactiveType GetReactiveType ();

        /// <summary>
        /// Processes reacted entities.
        /// Will be called only if any entities presents for processing.
        /// </summary>
        protected abstract void RunReactive ();
    }

    /// <summary>
    /// Reactive system with support of generic component type.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    public abstract class EcsReactiveSystem<T> : EcsReactiveSystemBase where T : class, new () {
        protected EcsFilterReactive<T> _reactiveFilter = null;

        public EcsReactiveSystem () { }

        public EcsReactiveSystem (EcsWorld world) {
            _reactiveFilter = world.GetFilter<EcsFilterReactive<T>> ();
        }

        protected override IEcsFilterReactive GetFilter () {
            return _reactiveFilter;
        }
    }
}