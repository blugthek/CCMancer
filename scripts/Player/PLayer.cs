using Godot;
using System;
using System.Threading;

public partial class PLayer : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private GameManager _gameManager;
	private EventManager _eventManager;

	private Vector2 _rawInput = new Vector2(0, 0);

	private enum State
	{
		Idle,
		Moving,
		Jumping,
	}

	private enum ExtraState
	{
		Default,
		Dash,
	}

	private struct PlayerConfig
	{

		public PlayerConfig()
		{
			CanDoubleJump = 0;
			JumpCount = 0;
			MaxJumpCount = 2;
			Dash = 1;
			Dashlength = 500;
			DashSpeed = 300;
			IFrame = 0;
			State = State.Idle;

			ExtraState = ExtraState.Default;
		}
		public int CanDoubleJump { get; set; }
		public int JumpCount { get; set; }
		public int MaxJumpCount { get; set; }
		public int Dash { get; set; }
		public int Dashlength { get; set; }
		public int DashSpeed { get; set; }
		public int IFrame { get; set; }
		public State State { get; set; }
		public ExtraState ExtraState { get; set; }
	}

	private PlayerConfig _playerConfig = new PlayerConfig();

	public override void _Ready()
	{
		_gameManager = GameManager.Instance;

		_eventManager = _gameManager.GetEventManager();

		_eventManager.RegisterEvent("_Input_Accept", parameters =>
		{
			if (_playerConfig.State != State.Jumping && (IsOnFloor() || _playerConfig.CanDoubleJump > 0) && _playerConfig.JumpCount < _playerConfig.MaxJumpCount)
			{
				_playerConfig.State = State.Jumping;
				_playerConfig.JumpCount = _playerConfig.JumpCount + 1;
			}
		});

		_eventManager.RegisterEvent("_Input_K", parameters =>
		{
			_playerConfig.CanDoubleJump = 1;
		});

		_eventManager.RegisterEvent("_Input_J", parameters =>
		{
			_playerConfig.DashSpeed += 500;
			_playerConfig.Dashlength += 500;
		});

		_eventManager.RegisterEvent("_Input_Shift", parameters =>
		{
			_playerConfig.ExtraState = ExtraState.Dash;
			_playerConfig.IFrame = 1;
			Thread.Sleep(_playerConfig.Dashlength);
			_playerConfig.IFrame = 0;
			GD.Print("Dash");
		});
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		var tSpeed = Speed;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		else
		{
			_playerConfig.JumpCount = 0;
		}

		if (_playerConfig.IFrame > 0)	
		{
			velocity.Y = 0;
			tSpeed += _playerConfig.DashSpeed;
		}

		if (_playerConfig.State == State.Jumping)
		{
			velocity.Y = JumpVelocity;
			_playerConfig.State = State.Idle;
		}

		if (velocity != Vector2.Zero)
		{
			_playerConfig.State = State.Moving;
		}

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		float finalX = velocity.X;
		if (direction != Vector2.Zero)
		{
			finalX = direction.X * tSpeed;
		}
		else if (_playerConfig.IFrame <= 0)
		{
			finalX = Mathf.MoveToward(Velocity.X, 0,Speed);
		}

		if (_playerConfig.ExtraState == ExtraState.Dash)
		{
			_playerConfig.ExtraState = ExtraState.Default;
			finalX += (float) direction.X * _playerConfig.DashSpeed;
		}

		// if (_playerConfig.IFrame > 0)
		// {
		// 	finalX = Mathf.MoveToward(Velocity.X, finalX, 50.1f);
		// }

		velocity.X = finalX;

		Velocity = velocity;
		MoveAndSlide();
	}
}
