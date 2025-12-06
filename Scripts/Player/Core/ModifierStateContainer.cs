using System;
using System.Collections.Generic;

/// <summary>
/// Container for modifier-specific state. Allows modifiers to store and share
/// custom data without modifying core MovementContext.
/// </summary>
public class ModifierStateContainer {
    private readonly Dictionary<Type, object> _states = new();

    /// <summary>
    /// Gets existing state or creates new instance if not found.
    /// </summary>
    public T GetOrCreate<T>() where T : class, new() {
        Type type = typeof(T);
        if (!_states.TryGetValue(type, out object state)) {
            state = new T();
            _states[type] = state;
        }
        return (T)state;
    }

    /// <summary>
    /// Tries to get existing state without creating.
    /// </summary>
    public bool TryGet<T>(out T state) where T : class {
        if (_states.TryGetValue(typeof(T), out object obj)) {
            state = (T)obj;
            return true;
        }
        state = null;
        return false;
    }

    /// <summary>
    /// Sets state directly.
    /// </summary>
    public void Set<T>(T state) where T : class {
        _states[typeof(T)] = state;
    }

    /// <summary>
    /// Checks if state type exists.
    /// </summary>
    public bool Has<T>() where T : class {
        return _states.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Resets all states that implement IResettable.
    /// Called each frame before processing modifiers.
    /// </summary>
    public void ResetAll() {
        foreach (object state in _states.Values) {
            if (state is IResettableState resettable) {
                resettable.Reset();
            }
        }
    }
}

/// <summary>
/// Implement on state classes that need per-frame reset.
/// </summary>
public interface IResettableState {
    void Reset();
}
