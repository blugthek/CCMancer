using System;
using Godot;

public partial class HitBox : Node2D
{
    [Signal]
    public delegate void HitEventHandler(Node2D area); // Signal declaration
    public delegate void ExitEventHandler(Node2D area);
    private Action<Node2D> _hitCallback; // Store the callback
    private Action<Node2D> _exitCallback; // Store the callback
    private Area2D _hitBox;

    private struct HitBoxConfig
    {
        public byte enterEventConnect;
        public byte exitEventConnect;
        public byte enterCallbackConnect;
        public byte exitCallbackConnect;

        public HitBoxConfig()
        {
            enterEventConnect = 0;
            exitEventConnect = 0;
            enterCallbackConnect = 0;
            exitCallbackConnect = 0;
        }
    }
    private HitBoxConfig _hitBoxConfig;

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
        _hitBox.Connect("area_exited", new Callable(this, nameof(_OnHitboxExited)));

        _hitBox.Connect("body_entered", new Callable(this, nameof(_OnBodyEntered)));
        _hitBox.Connect("body_exited", new Callable(this, nameof(_OnBodyExited)));
    }

    private void _OnHitboxEntered(Area2D area)
    {
        if (_hitBoxConfig.enterEventConnect == 1)
        {
            EmitSignal(nameof(HitEventHandler), area); // Emit the signal
        }
        _hitCallback?.Invoke(area); // Call the stored callback
    }

    private void _OnHitboxExited(Area2D area)
    {
        if (_hitBoxConfig.exitEventConnect == 1)
        {
            EmitSignal(nameof(ExitEventHandler), area); // Emit the signal
        }
        _exitCallback?.Invoke(area); // Call the stored callback
    }

    private void _OnBodyEntered(Node2D body)
    {
        if (_hitBoxConfig.enterEventConnect == 1)
        {
            EmitSignal(nameof(HitEventHandler), body); // Emit the signal
        }
        _hitCallback?.Invoke(body); // Call the stored callback
    }

    private void _OnBodyExited(Node2D body)
    {
        if (_hitBoxConfig.exitEventConnect == 1)
        {
            EmitSignal(nameof(ExitEventHandler), body); // Emit the signal
        }
        _exitCallback?.Invoke(body); // Call the stored callback
    }

    public void HandleHit(GodotObject obj, string method)
    {
        _hitBoxConfig.enterEventConnect = 1;
        Connect(nameof(HitEventHandler), new Callable(obj, method)); // Connect signal to the object's method
    }

    public void HandleHit(Action<Node2D> hitCallback)
    {
        _hitCallback = hitCallback; // Store the lambda or method reference
    }

    public void HandleExit(GodotObject obj, string method)
    {
        _hitBoxConfig.exitEventConnect = 1;
        Connect(nameof(ExitEventHandler), new Callable(obj, method)); // Connect signal to the object's method
    }

    public void HandleExit(Action<Node2D> exitCallback)
    {
        _exitCallback = exitCallback; // Store the lambda or method reference
    }
}