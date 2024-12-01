using Godot;
using System;
using System.Threading;

public partial class CameraPanningTest : CharacterBody2D,IDamageable
{
    private EventManager _eventManager;

    public override void _Ready()
    {
        _eventManager = GameManager.Instance.GetEventManager();
    }
    [Export] private Node2D _panningTarget;

    private float jumpingSpeed = -400.0f;
    private bool isMonsterInit = false;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey) //Simulate encounter
        {
            if (eventKey.Keycode == Key.T && eventKey.Pressed)
            {
                GD.Print("T pressed");
                _eventManager.TriggerEventAsThread("_Input_T", callback => isMonsterInit = true, _panningTarget);
                // GameManager.Instance.SetPannigTarget(_panningTarget,1000);
            }
        }
    }

    private bool mInit = false;
    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;
        if (isMonsterInit)
        {
            mInit = true;
            velocity.Y = jumpingSpeed;
            isMonsterInit = false;
            GD.Print("Monster Init");
        }

        if (mInit)
        {
            if (!IsOnFloor())
            {
                velocity += GetGravity() * (float)delta;
            }
            Velocity = velocity;
            MoveAndSlide();
        }
        // base._PhysicsProcess(delta);
    }

    public void TakeDamage(int damage)
    {
        GD.Print("Hit AHHHHHHHH");
        QueueFree();
    }
}