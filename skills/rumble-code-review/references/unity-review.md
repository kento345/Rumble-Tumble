# Unity Review Criteria

Use this reference whenever the review touches Unity runtime behavior, assets, scenes, prefabs, MonoBehaviours, or frame performance.

## Lifecycle And Initialization

- Check whether `Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, and `OnDestroy` are used with clear ownership.
- Watch for initialization order assumptions between objects in different scenes or prefabs.
- Confirm event subscription and unsubscription happen in matching lifecycle methods.
- Look for state that is reset in only some entry paths, such as scene reloads, respawns, or replay flows.

## Serialized References And Assets

- `SerializeField` references can fail at runtime if scene or prefab wiring is missing.
- Inspector-only dependencies should have validation, safe fallbacks, or clear error output when failure would be hard to diagnose.
- Review code changes together with likely prefab, scene, animation, audio, VFX, and input configuration dependencies.
- Be careful with renamed fields, serialized data migration, and enum/index changes that can silently change existing asset behavior.

## Scene And Prefab Integration

- Check whether scripts assume a specific scene hierarchy, object name, tag, layer, or component placement.
- `Find`, `FindObjectOfType`, tag searches, and name-based lookups should be treated as fragile unless the dependency is intentionally global and stable.
- Prefab variants can inherit stale values. Mention when an issue should be verified in prefab/scene assets.

## Frame Performance

- Inspect `Update`, `FixedUpdate`, animation callbacks, collision callbacks, and input loops for avoidable repeated work.
- Repeated `GetComponent`, `Find`, LINQ, string operations, allocations, and collection rebuilding inside hot paths can cause frame instability.
- Prefer cached references or event-driven updates when the behavior does not need per-frame polling.
- Avoid recommending optimization work without connecting it to a hot path or likely scale problem.

## Physics, Input, And Time

- Input usually belongs in `Update`; physics mutation usually belongs in `FixedUpdate`.
- Watch for mixed use of `Time.deltaTime`, `Time.fixedDeltaTime`, and unscaled time.
- Check collision/trigger logic for missing layer filtering, duplicate events, and cleanup when objects are disabled or destroyed.

## Coroutines, Async, And State Transitions

- Coroutines can stack accidentally if started repeatedly without guards.
- Stopping a coroutine by string or losing its handle can make cancellation fragile.
- Scene transitions, respawns, object disable/enable cycles, and game over flows are common places for stale coroutine work.
- Async callbacks should not assume target objects still exist.

## Static State, Singletons, And ScriptableObjects

- Static fields and singleton instances can survive longer than expected and create cross-scene state leakage.
- ScriptableObjects can be shared mutable state. Review whether runtime mutations are intended and reset correctly.
- Domain reload and enter-play-mode settings can change how reliably static state resets.

## Unity Null And Destroy Semantics

- Destroyed Unity objects can compare as null while still having managed references.
- Code that stores references across destruction, pooling, or scene unload should be reviewed carefully.
- Null guards should protect both missing serialized references and destroyed runtime objects.

## Animation, Audio, VFX, And Events

- Animator parameter names, animation events, audio mixer names, VFX object paths, and timeline signals are string/configuration dependencies.
- Suggest constants, wrappers, validation, or editor checks when name mismatches would be painful to debug.
- Review callback order when animation events mutate gameplay state.

## Build And Platform Concerns

- Editor-only APIs should be guarded with `#if UNITY_EDITOR` or kept out of runtime assemblies.
- Platform-specific input, resolution, file paths, and performance assumptions should be flagged when relevant.
- Mention when a finding requires a build test rather than only editor play mode.
