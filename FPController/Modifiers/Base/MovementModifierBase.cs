using KadaXuanwu.Utils.Runtime.FPController.Core;
using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using KadaXuanwu.Utils.Runtime.FPController.InputAbstraction;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Base {
    public abstract class MovementModifierBase<TConfig, TEvents> : IMovementModifier
        where TConfig : ScriptableObject, IModifierConfig
        where TEvents : class, IModifierEvents, new() {

        public virtual bool IsActive => _isActive;
        public TEvents Events { get; } = new TEvents();

        protected FirstPersonController Controller { get; private set; }
        protected FirstPersonControllerConfig BaseConfig => Controller.Config;
        protected ICharacterInput Input => Controller.Input;
        protected CharacterEvents CharacterEvents => Controller.Events;
        protected TConfig Config { get; private set; }

        private bool _isActive = true;

        protected MovementModifierBase(TConfig config) {
            Config = config;
        }

        public virtual void OnInitialize(FirstPersonController controller) {
            Controller = controller;
        }

        public virtual void OnRemove() {
            Events.ClearSubscribers();
            Controller = null;
        }

        public abstract void ProcessMovement(ref MovementContext context);

        public void SetActive(bool active) => _isActive = active;
        public void SetConfig(TConfig config) => Config = config;
    }

    /// <summary>
    /// Base class for modifiers with config but no custom events.
    /// </summary>
    public abstract class MovementModifierBase<TConfig> : IMovementModifier
        where TConfig : ScriptableObject, IModifierConfig {

        public virtual bool IsActive => _isActive;

        protected FirstPersonController Controller { get; private set; }
        protected FirstPersonControllerConfig BaseConfig => Controller.Config;
        protected ICharacterInput Input => Controller.Input;
        protected CharacterEvents CharacterEvents => Controller.Events;
        protected TConfig Config { get; private set; }

        private bool _isActive = true;

        protected MovementModifierBase(TConfig config) {
            Config = config;
        }

        public virtual void OnInitialize(FirstPersonController controller) {
            Controller = controller;
        }

        public virtual void OnRemove() {
            Controller = null;
        }

        public abstract void ProcessMovement(ref MovementContext context);

        public void SetActive(bool active) {
            _isActive = active;
        }

        public void SetConfig(TConfig config) {
            Config = config;
        }
    }

    /// <summary>
    /// Base class for modifiers without config or custom events.
    /// </summary>
    public abstract class MovementModifierBase : IMovementModifier {
        public virtual bool IsActive => _isActive;

        protected FirstPersonController Controller { get; private set; }
        protected FirstPersonControllerConfig BaseConfig => Controller.Config;
        protected ICharacterInput Input => Controller.Input;
        protected CharacterEvents CharacterEvents => Controller.Events;

        private bool _isActive = true;

        public virtual void OnInitialize(FirstPersonController controller) {
            Controller = controller;
        }

        public virtual void OnRemove() {
            Controller = null;
        }

        public abstract void ProcessMovement(ref MovementContext context);

        public void SetActive(bool active) {
            _isActive = active;
        }
    }
}
