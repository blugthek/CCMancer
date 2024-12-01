using System;
using Godot;

public partial class HitBox : Node2D
    {
        [Signal]
        public delegate void HitEventHandler(Area2D area); // Signal declaration

        private Action<Area2D> _hitCallback; // Store the callback

        private Area2D _hitBox;

        public override void _Ready()
        {
            // Ensure the "HitBox" node exists in the scene tree
            _hitBox = GetNode<Area2D>("HitBox");
            _hitBox.Connect("area_entered", new Callable(this, nameof(_OnHitboxEntered)));
        }

        public void Initialize(Area2D hitBox)
        {
            // Ensure the "HitBox" node exists in the scene tree
            _hitBox = hitBox;
            _hitBox.Connect("area_entered", new Callable(this, nameof(_OnHitboxEntered)));
        }

        private void _OnHitboxEntered(Area2D area)
        {
            // EmitSignal(nameof(HitEventHandler), area); // Emit the signal
            _hitCallback?.Invoke(area); // Call the stored callback
        }

        public void HandleHit(GodotObject obj, string method)
        {
            Connect(nameof(HitEventHandler), new Callable(obj, method)); // Connect signal to the object's method
        }

        public void HandleHit(Action<Area2D> hitCallback)
        {
            _hitCallback = hitCallback; // Store the lambda or method reference
        }
    }