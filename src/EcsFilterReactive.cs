// ----------------------------------------------------------------------------
// The MIT License
// Reactive behaviour for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Leopotam.Ecs.Reactive {
    /// <summary>
    /// Common interface for all reactive filter listeners.
    /// </summary>
    public interface IEcsFilterReactiveListener {
        void OnEntityAdded (int entity);
        void OnEntityRemoved (int entity);
    }

    /// <summary>
    /// Common interface for all reactive filters.
    /// </summary>
    public interface IEcsFilterReactive {
        void AddListener (IEcsFilterReactiveListener listener);
        void RemoveListener (IEcsFilterReactiveListener listener);
    }

    /// <summary>
    /// Default implementation of reactive filter for one component.
    /// </summary>
    /// <typeparam name="Inc1">Component type</typeparam>
    public class EcsFilterReactive<Inc1> : EcsFilter, IEcsFilterReactive where Inc1 : class, new () {
        IEcsFilterReactiveListener[] _listeners = new IEcsFilterReactiveListener[4];
        int _listenersCount;

        protected EcsFilterReactive () {
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnAddEvent (int entity) {
            if (Entities.Length == EntitiesCount) {
                Array.Resize (ref Entities, EntitiesCount << 1);
            }
            Entities[EntitiesCount++] = entity;
            for (var i = 0; i < _listenersCount; i++) {
                _listeners[i].OnEntityAdded (entity);
            }
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnRemoveEvent (int entity) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (Entities[i] == entity) {
                    EntitiesCount--;
                    Array.Copy (Entities, i + 1, Entities, i, EntitiesCount - i);
                    for (var j = 0; j < _listenersCount; j++) {
                        _listeners[j].OnEntityRemoved (entity);
                    }
                    break;
                }
            }
        }

        public void AddListener (IEcsFilterReactiveListener listener) {
#if DEBUG
            for (var i = 0; i < _listenersCount; i++) {
                if (_listeners[i] == listener) {
                    throw new Exception ("Listener already subscribed.");
                }
            }
#endif
            if (_listeners.Length == _listenersCount) {
                Array.Resize (ref _listeners, _listenersCount << 1);
            }
            _listeners[_listenersCount++] = listener;
        }

        public void RemoveListener (IEcsFilterReactiveListener listener) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (_listeners[i] == listener) {
                    _listenersCount--;
                    Array.Copy (_listeners, i + 1, _listeners, i, _listenersCount - i);
                    break;
                }
            }
        }
    }
}