[![gitter](https://img.shields.io/gitter/room/leopotam/ecs.svg)](https://gitter.im/leopotam/ecs)
[![license](https://img.shields.io/github/license/Leopotam/ecs.reactive.svg)](https://github.com/Leopotam/ecs.reactive/blob/develop/LICENSE)
# Reactive behaviour for LeoECS
Reactive filters / systems for using with [Entity Component System Framework](https://github.com/Leopotam/ecs).

> Tested on unity 2018.2 (not dependent on it) and contains assembly definition for compiling to separate assembly file for performance reason.

> **Its early work-in-progress stage, not recommended to use in real projects, any api / behaviour can change later.**

## Example:
```csharp
class ReactiveComponent1 {
    public int Id;
}

sealed class TestReactiveStartup : MonoBehaviour {
    EcsWorld _world;
    EcsSystems _systems;

    void OnEnable () {
        _world = new EcsWorld ();
        _systems = new EcsSystems (_world);
        _systems
            .Add (new TestReactiveSystemOnAdd ())
            .Add (new TestReactiveSystemOnRemove ())
            .Initialize ();
    }

    void Update () {
        _systems.Run ();
    }

    void OnDisable () {
        _systems.Dispose ();
        _systems = null;
        _world.Dispose ();
        _world = null;
    }
}

[EcsInject]
sealed class TestReactiveSystemOnAdd : EcsReactiveSystem<ReactiveComponent1>, IEcsInitSystem {
    EcsWorld _world = null;

    void IEcsInitSystem.Initialize () {
        // create test data for catching OnAdded event at react system later.
        _world.CreateEntityWith<ReactiveComponent1> ().Id = 10;
        _world.CreateEntityWith<ReactiveComponent1> ().Id = 20;
        _world.CreateEntityWith<ReactiveComponent1> ().Id = 30;
    }

    void IEcsInitSystem.Destroy () { }

    protected override EcsReactiveType GetReactiveType () {
        // this system should react on Add event.
        return EcsReactiveType.OnAdded;
    }

    protected override void RunReactive () {
        // ReactedEntitiesCount contains amount of reacted entities at ReactedEntities collection.
        for (var i = 0; i < ReactedEntitiesCount; i++) {
            var entity = ReactedEntities[i];
            var c = _world.GetComponent<ReactiveComponent1> (entity);
            Debug.LogFormat ("[ON-ADDED] Reacted entity \"{0}\" and component {1}", entity, c.Id);

            // remove reacted entities for test OnRemoved reactive system.
            _world.RemoveEntity (entity);
        }
    }
}

[EcsInject]
sealed class TestReactiveSystemOnRemove : EcsReactiveSystem<ReactiveComponent1> {
    protected override EcsReactiveType GetReactiveType () {
        // this system should react on Remove event.
        return EcsReactiveType.OnRemoved;
    }

    protected override void RunReactive () {
        for (var i = 0; i < ReactedEntitiesCount; i++) {
            Debug.LogFormat ("[ON-REMOVE] Reacted entity: {0}", ReactedEntities[i]);
        }
    }
}
```

Result log:
```
[ON-ADDED] Reacted entity "0" and component 10
[ON-ADDED] Reacted entity "1" and component 20
[ON-ADDED] Reacted entity "2" and component 30
[ON-REMOVE] Reacted entity: 0
[ON-REMOVE] Reacted entity: 1
[ON-REMOVE] Reacted entity: 2
```