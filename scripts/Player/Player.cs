using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

public partial class Player : CharacterBody2D
{
	public State currentState;
	private readonly Dictionary<string, State> states = new Dictionary<string, State>();

	public void ChangeState(State newState)
	{
		currentState?.Exit();
		currentState = newState;
		currentState.Enter();
	}

	public override void _Ready()
	{
		ComponentSetup();
		UnitSetup();
		GD.Print(health);

		_gameManager = GameManager.Instance;

		_eventManager = _gameManager.GetEventManager();

		animatedSprite2D = GetNode<AnimatedSprite2D>("p_anim");
		weapon_animatedSprite2D = GetNode<AnimatedSprite2D>("w_anim");
		weapon_handler = GetNode<WeaponHandler>("w_handler");
		animationPlayer = GetNode<AnimationPlayer>("p_animP");

		_eventManager.RegisterEvent("_Req_Player", parameters =>
		{
			GD.Print("_Req_Player");
			var callback = (Action<object>)parameters[0];
			if (callback != null)
			{
				callback?.Invoke(this);
			}
		});

		// Initialize states
		states["idle"] = new IdleState(this);
		states["moving"] = new MovingState(this);
		states["jumping"] = new JumpState(this);

		// Start with idle state
		ChangeState(states["idle"]);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsOnFloor()) Velocity += GetGravity() * (float)delta;
		currentState.PhysicsUpdate((float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		currentState.HandleInput(@event);
		_eventManager.HandleInput(@event);
	}

	public float Speed, JumpVelocity;
	int health;

	private ExtendedComponent Component = new ExtendedComponent();
	private void ComponentSetup()
	{
		Component.Set("Health", new HealthData(3, 100));
		Component.Set("Stat", new UnitStat(300, 500, 1, null));
	}

	private void UnitSetup()
	{
		Component.Get<UnitStat>("Stat", (UnitStat) =>
		{
			Speed = UnitStat.HorizontalSpeed;
			JumpVelocity = UnitStat.JumpSpeed * -1;
		});

		Component.Get<HealthData>("Health", (HealthData) =>
		{
			health = HealthData.CurrentHealth;
		});
	}

	private HealthData healthData = new HealthData(100, 100);
	private GameManager _gameManager;
	private EventManager _eventManager;
	private Vector2 _rawInput = new Vector2(0, 0);

	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public AnimatedSprite2D weapon_animatedSprite2D;
	[Export] public WeaponHandler weapon_handler;
	[Export] public AnimationPlayer animationPlayer;

	private void OnAnimationFinished()
	{
		GD.Print("Animation Finished");
	}

	private void OnFrameChanged(int frame)
	{
		GD.Print("Frame change", frame);
	}

	public void TakeDamage(int damage)
	{
		GD.Print("Hit Player AHHHhH");
		health -= 1;
		if (health <= 0)
		{
			_eventManager.TriggerEventAsThread("_Force_Death", callback => { });
		}

		switch (damage)
		{
			case > 1 and <= 2:
				animatedSprite2D.Play("hit");
				break;
			case <= 1:
				animatedSprite2D.Play("hit2");
				break;
		}
	}

	private void Shoot()
	{
		weapon_handler.Shoot();
	}
}
