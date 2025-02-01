using Godot;
using System;

public abstract class State
{
    protected Player player;

    public State(Player player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
    public virtual void PhysicsUpdate(float delta) { }
    public virtual void HandleInput(InputEvent @event) { }
}

public class IdleState : State
{
    private float deceleration = 1.5f; // Adjust this to control how quickly the player slows down

    public IdleState(Player player) : base(player) { }

    public override void Enter()
    {
        player.animatedSprite2D.Play("idle");
    }

    public override void HandleInput(InputEvent @event)
    {
        GD.Print("handle input");
        if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Keycode == Key.Space && player.IsOnFloor())
            {
                player.ChangeState(new JumpState(player));
            }
        }

        // Check for any horizontal input
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (direction.X != 0)
        {
            player.ChangeState(new MovingState(player));
        }
    }

    public override void PhysicsUpdate(float delta)
    {
        // Gradually slow down the player if they're moving
        if (Mathf.Abs(player.Velocity.X) > 0)
        {
            player.Velocity = new Vector2(
                Mathf.MoveToward(player.Velocity.X, 0, deceleration * player.Speed * delta),
                player.Velocity.Y
            );
        }
        
        player.MoveAndSlide();
    }
}

public class MovingState : State
{
    private float stopThreshold = 10.0f; // Adjust this value based on how fast you want the player to stop
    private float stopTimer = 0f; // Timer for how long the player has been nearly stopped
    private float stopTimeThreshold = 0.1f; // Time threshold before considering the player as stopped

    public MovingState(Player player) : base(player) { }

    public override void Enter()
    {
        player.animatedSprite2D.Play("run");
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        float targetSpeed = direction.X * player.Speed;

        // Smooth movement between current velocity and target speed
        player.Velocity = new Vector2(
            Mathf.Lerp(player.Velocity.X, targetSpeed, 0.1f), // Adjust the 0.1f for speed of acceleration/deceleration
            player.Velocity.Y
        );

        player.MoveAndSlide();

        // Check if player is nearly stopped
        if (Mathf.Abs(player.Velocity.X) < stopThreshold)
        {
            stopTimer += delta;
            if (stopTimer >= stopTimeThreshold)
            {
                player.ChangeState(new IdleState(player));
            }
        }
        else
        {
            stopTimer = 0; // Reset timer if player starts moving again
        }

        // Jump check
        if (Input.IsActionJustPressed("ui_accept") && player.IsOnFloor())
        {
            player.ChangeState(new JumpState(player));
        }
    }

    public override void Exit()
    {
        stopTimer = 0; // Reset timer when leaving this state
    }
}

public class JumpState : State
{
    private float jumpForce = 0; // Keeps track of jump force for variable jump height
    private float maxJumpForce = 200; // Maximum additional force for jump
    private float jumpTime = 0; // Time since jump started
    private float maxJumpTime = 0.2f; // Maximum time jump can be held for higher jump
    private bool jumpButtonHeld = false; // Tracks if jump button is still being held

    public JumpState(Player player) : base(player) { }

    public override void Enter()
    {
        player.animatedSprite2D.Play("jump");
        player.Velocity = new Vector2(player.Velocity.X, player.JumpVelocity);
        jumpForce = 0;
        jumpTime = 0;
        jumpButtonHeld = true;
    }

    public override void PhysicsUpdate(float delta)
    {
        // Apply gravity
        player.Velocity += Vector2.Down * 980 * delta;

        // Variable jump height
        if (jumpButtonHeld && jumpForce < maxJumpForce && jumpTime < maxJumpTime)
        {
            jumpForce += player.JumpVelocity * delta * 5; // Adjust this multiplier for jump feel
            player.Velocity = new Vector2(player.Velocity.X, Mathf.Min(player.Velocity.Y - jumpForce * delta, player.JumpVelocity));
            jumpTime += delta;
        }
        else
        {
            jumpButtonHeld = false; // Reset if conditions aren't met
        }

        player.MoveAndSlide();

        // Change to idle only when on floor and no jump button held
        if (player.IsOnFloor() && !Input.IsActionPressed("ui_accept"))
        {
            player.ChangeState(new IdleState(player));
        }
    }

    public override void HandleInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Keycode == Key.Space && !eventKey.Pressed)
            {
                jumpButtonHeld = false; // Release jump button
            }
        }
    }
}