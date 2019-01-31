// ----------------------------------------------------------------------------
// The MIT License
// Reactive behaviour for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2019 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

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
    public abstract class EcsReactiveSystemBase : IEcsFilterListener, IEcsPreInitSystem, IEcsRunSystem {
        [Obsolete ("Use foreach (var entity in this) {} loop instead. Take care - returned value is EntityId, not index")]
        public int[] ReactedEntities { get { return _reactedEntities; } }

        [Obsolete ("Use foreach (var entity in this) {} loop instead. Take care - returned value is EntityId, not index")]
        public int ReactedEntitiesCount { get { return _reactedEntitiesCount; } }

        int[] _reactedEntities = new int[32];
        int _reactedEntitiesCount;

        EcsReactiveType _reactType;

        void IEcsPreInitSystem.PreInitialize () {
            _reactType = GetReactiveType ();
            GetFilter ().AddListener (this);
        }

        void IEcsPreInitSystem.PreDestroy () {
            _reactedEntitiesCount = 0;
            GetFilter ().RemoveListener (this);
        }

        void IEcsRunSystem.Run () {
            if (_reactedEntitiesCount > 0) {
                RunReactive ();
                _reactedEntitiesCount = 0;
            }
        }

        void IEcsFilterListener.OnEntityAdded (int entity) {
            if (_reactType == EcsReactiveType.OnAdded) {
                if (_reactedEntities.Length == _reactedEntitiesCount) {
                    Array.Resize (ref _reactedEntities, _reactedEntitiesCount << 1);
                }
                _reactedEntities[_reactedEntitiesCount++] = entity;
            }
        }

        void IEcsFilterListener.OnEntityRemoved (int entity) {
            if (_reactType == EcsReactiveType.OnRemoved) {
                if (_reactedEntities.Length == _reactedEntitiesCount) {
                    Array.Resize (ref _reactedEntities, _reactedEntitiesCount << 1);
                }
                _reactedEntities[_reactedEntitiesCount++] = entity;
            }
        }

        /// <summary>
        /// Returns EcsFilterReactive instance for watching on it.
        /// </summary>
        protected abstract EcsFilter GetFilter ();

        /// <summary>
        /// Returns reactive type behaviour.
        /// </summary>
        protected abstract EcsReactiveType GetReactiveType ();

        /// <summary>
        /// Processes reacted entities.
        /// Will be called only if any entities presents for processing.
        /// </summary>
        protected abstract void RunReactive ();

#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Enumerator GetEnumerator () {
            return new Enumerator (_reactedEntities, _reactedEntitiesCount);
        }

        public struct Enumerator : IEnumerator<int> {
            readonly int[] _entities;
            readonly int _count;
            int _idx;

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            internal Enumerator (int[] entities, int entitiesCount) {
                _entities = entities;
                _count = entitiesCount;
                _idx = -1;
            }

            public int Current {
#if NET_4_6 || NET_STANDARD_2_0
                [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
                get { return _entities[_idx]; }
            }

            object System.Collections.IEnumerator.Current { get { return null; } }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Dispose () { }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public bool MoveNext () {
                return ++_idx < _count;
            }

#if NET_4_6 || NET_STANDARD_2_0
            [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Reset () {
                _idx = -1;
            }
        }
    }

    /// <summary>
    /// Reactive system with support for one component type.
    /// </summary>
    /// <typeparam name="Inc1">Component type.</typeparam>
    public abstract class EcsReactiveSystem<Inc1> : EcsReactiveSystemBase where Inc1 : class, new () {
        protected EcsFilter<Inc1> _reactiveFilter = null;

        public EcsReactiveSystem () { }

        public EcsReactiveSystem (EcsWorld world) {
            _reactiveFilter = world.GetFilter<EcsFilter<Inc1>> ();
        }

        sealed protected override EcsFilter GetFilter () {
            return _reactiveFilter;
        }
    }

    /// <summary>
    /// Reactive system with support for two component types.
    /// </summary>
    /// <typeparam name="Inc1">First component type.</typeparam>
    /// <typeparam name="Inc2">Second component type.</typeparam>
    public abstract class EcsReactiveSystem<Inc1, Inc2> : EcsReactiveSystemBase where Inc1 : class, new () where Inc2 : class, new () {
        protected EcsFilter<Inc1, Inc2> _reactiveFilter = null;

        public EcsReactiveSystem () { }

        public EcsReactiveSystem (EcsWorld world) {
            _reactiveFilter = world.GetFilter<EcsFilter<Inc1, Inc2>> ();
        }

        sealed protected override EcsFilter GetFilter () {
            return _reactiveFilter;
        }
    }

    /// <summary>
    /// For internal use only! Special component for mark user components as updated.
    /// </summary>
    /// <typeparam name="T">User component type.</typeparam>
    public class EcsUpdateReactiveFlag<T> where T : class, new () { }

    /// <summary>
    /// Reactive system for processing updated components (EcsWorld.MarkComponentAsUpdated).
    /// </summary>
    /// <typeparam name="Inc1">Component type.</typeparam>
    public abstract class EcsUpdateReactiveSystem<Inc1> : EcsReactiveSystemBase where Inc1 : class, new () {
        /// <summary>
        /// EcsWorld instance.
        /// </summary>
        protected EcsWorld _world = null;

        /// <summary>
        /// Internal filter for custom reaction on entities.
        /// </summary>
        protected EcsFilter<EcsUpdateReactiveFlag<Inc1>> _reactiveFilter = null;

        public EcsUpdateReactiveSystem () { }

        public EcsUpdateReactiveSystem (EcsWorld world) {
            _world = world;
            _reactiveFilter = _world.GetFilter<EcsFilter<EcsUpdateReactiveFlag<Inc1>>> ();
        }

        sealed protected override EcsFilter GetFilter () {
            return _reactiveFilter;
        }

        sealed protected override EcsReactiveType GetReactiveType () {
            return EcsReactiveType.OnAdded;
        }

        sealed protected override void RunReactive () {
            foreach (var i in _reactiveFilter) {
                _world.RemoveComponent<EcsUpdateReactiveFlag<Inc1>> (_reactiveFilter.Entities[i]);
            }
            RunUpdateReactive ();
        }

        /// <summary>
        /// Processes MarkComponentAsUpdated reacted entities.
        /// Will be called only if any entities presents for processing.
        /// </summary>
        protected abstract void RunUpdateReactive ();
    }
}