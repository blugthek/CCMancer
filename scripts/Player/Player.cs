using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

public partial class Player : CharacterBody2D
{
	float Speed,JumpVelocity;
	int health;

	private ExtendedComponent Component = new ExtendedComponent();
	private void ComponentSetup()
	{
		Component.Set("Health",new HealthData(3,100));
		Component.Set("Stat",new UnitStat(300,500,1,null));
	}

	private void UnitSetup()
	{
		Component.Get<UnitStat>("Stat",(UnitStat) => {
			Speed = UnitStat.HorizontalSpeed;
			JumpVelocity = UnitStat.JumpSpeed * -1;
		});

		Component.Get<HealthData>("Health",(HealthData) => {
			health = HealthData.CurrentHealth;
		});
	}

	private HealthData healthData = new HealthData(100,100);

	private GameManager _gameManager;
	private EventManager _eventManager;
	private Vector2 _rawInput = new Vector2(0, 0);

	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public AnimatedSprite2D weapon_animatedSprite2D;
	[Export] public WeaponHandler weapon_handler;
	[Export] public AnimationPlayer animationPlayer;

	public enum State
	{
		Idle,
		Moving,
		Jumping,
		Dead,
	}

	public enum ExtraState
	{
		Default,
		Dash,
		Attack,
		AirAttack,
		Range,
		AirRange,
	}

	public struct PlayerConfig
	{

		public PlayerConfig()
		{
			Facing = 1;
			CanDoubleJump = 0;
			JumpCount = 0;
			MaxJumpCount = 2;
			Dash = 1;
			Dashlength = 500;
			DashSpeed = 300;
			IFrame = 0;
			CanShoot = 0;
			ShootCooldown = 1000;
			ProjectileBounce = 1;
			State = State.Idle;
			ExtraState = ExtraState.Default;
		}
		public int Facing { get; set; }
		public int CanDoubleJump { get; set; }
		public int JumpCount { get; set; }
		public int MaxJumpCount { get; set; }
		public int Dash { get; set; }
		public int Dashlength { get; set; }
		public int DashSpeed { get; set; }
		public int IFrame { get; set; }
		public int CanShoot { get; set; }
		public int ShootCooldown { get; set; }
		public int ProjectileBounce { get; set; }
		public State State { get; set; }
		public ExtraState ExtraState { get; set; }
	}

	private PlayerConfig _playerConfig = new PlayerConfig();

	public PlayerConfig GetPlayerConfig(){
		GD.Print("GetPlayerConfig");
		return _playerConfig;
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
			if (_playerConfig.ExtraState == ExtraState.Default)
			{
				_playerConfig.ExtraState = ExtraState.Dash;
				_playerConfig.IFrame = 1;
				Thread.Sleep(_playerConfig.Dashlength);
				_playerConfig.IFrame = 0;
				if (_playerConfig.ExtraState == ExtraState.Dash)
				{
					_playerConfig.ExtraState = ExtraState.Default;
				}
				GD.Print("Dash");
			}
		});

		_eventManager.RegisterEvent("_Input_Click_Left", parameters =>
		{
			GD.Print("Attack ", _playerConfig.State, "", _playerConfig.ExtraState, IsOnFloor());
			if (_playerConfig.State == State.Idle && IsOnFloor())
			{
				_playerConfig.ExtraState = ExtraState.Attack;
				Thread.Sleep(300);
				_playerConfig.ExtraState = ExtraState.Default;
				GD.Print("Attack ", _playerConfig.State, "", _playerConfig.ExtraState);
			}

			if (!IsOnFloor())
			{
				GD.Print("AirAttack ", _playerConfig.State, "", _playerConfig.ExtraState);
				_playerConfig.ExtraState = ExtraState.AirAttack;
				Thread.Sleep(300);
				_playerConfig.ExtraState = ExtraState.Default;
			}
		});

		var canShot = 0;
		_eventManager.RegisterEvent("_Input_Click_Right", parameters =>
		{
			GD.Print("RangeAttack ", _playerConfig.State, "", _playerConfig.ExtraState, _playerConfig.CanShoot);
			if (_playerConfig.CanShoot == 0 && canShot == 0)
			{
				_playerConfig.CanShoot = 1;
				canShot = 1;
				_playerConfig.ExtraState = ExtraState.Range;
				// CallDeferred(nameof(Shoot));
				// CallDeferredThreadGroup(nameof(Shoot));
				// Shoot();
				Thread.Sleep(500);
				_playerConfig.ExtraState = ExtraState.Default;
				canShot = 0;
				_playerConfig.CanShoot = 0;
			}
		});

		
		_eventManager.RegisterEvent("_Force_Death", parameters =>
		{
			_playerConfig.State = State.Dead;
			_eventManager.TriggerEvent("_Player_Dead",_ => {});
			Thread.Sleep(1000);
			QueueFree();
		});

		// animatedSprite2D.Connect("animation_finished",new Callable(this,  nameof(OnAnimationFinished)));
		// animatedSprite2D.AnimationFinished += OnAnimationFinished;
		// weapon_animatedSprite2D.AnimationFinished += OnAnimationFinished;
		// animatedSprite2D.FrameChanged += () => { GD.Print("Frame Changed", animatedSprite2D.Frame); }; //OnFrameChanged;
																									   // weapon_animatedSprite2D.FrameChanged += () => { GD.Print("Frame Changed",weapon_animatedSprite2D.Frame); }; //OnFrameChanged;
																									   // weapon_animatedSprite2D.Connect("animation_finished",  nameof(OnAnimationFinished));
	}

	private void OnAnimationFinished()
	{
		GD.Print("Animation Finished");
		// _playerConfig.ExtraState = ExtraState.Default;
	}

	private void OnFrameChanged(int frame)
	{
		GD.Print("Animation Finished", frame);
		// _playerConfig.ExtraState = ExtraState.Default;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_playerConfig.State == State.Dead)
		{
			animatedSprite2D.Play("dying");
			return;
		}
		Velocity = movementCal(Velocity, delta);
		// Position += Velocity * (float)delta;
		MoveAndSlide();
		if (_playerConfig.ExtraState == ExtraState.Range && _playerConfig.CanShoot == 1)
		{
			_playerConfig.CanShoot = 0;
			Shoot();
		}
	}

	public void TakeDamage(int damage)
	{
		GD.Print("Hit Player AHHHhH");
		health -= 1;
		if (health <= 0)
		{
			_eventManager.TriggerEventAsThread("_Force_Death", callback => { });
		}
	}

	private Vector2 momentum = Vector2.Zero;
	private float accerelation = 0.1f;
	private Vector2 movementCal(Vector2 velocity, double delta)
	{
		// var additionalVelocity = new Vector2(_rawInput.X, _rawInput.Y);
		var additional_x = Speed;
		var tSpeed = Speed;
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
			if (_playerConfig.ExtraState == ExtraState.AirAttack)
			{
				animatedSprite2D.Play("melee_attack_air");
				if (_playerConfig.Facing == 1)
				{
					animationPlayer.Play("weapons_swing_r");
				}
				else
				{
					animationPlayer.Play("weapons_swing_l");
				}

			}
			else
			{

				animatedSprite2D.Play("jump");
				if (velocity.Y > 0)
				{
					animatedSprite2D.Frame = 4;
				}
			}
			accerelation = 0.02f;
		}
		else
		{
			_playerConfig.JumpCount = 0;
			accerelation = 0.1f;
			if (_playerConfig.ExtraState == ExtraState.Attack)
			{
				animatedSprite2D.Play("melee_attack");
				weapon_animatedSprite2D.Play("weapon_melee");
				if (_playerConfig.Facing == 1)
				{
					animationPlayer.Play("weapons_swing_r");
				}
				else
				{
					animationPlayer.Play("weapons_swing_l");
				}
			}
			else if (_playerConfig.ExtraState == ExtraState.Range)
			{
				animatedSprite2D.Play("range_attack");
			}
			else
			{
				if (velocity.X == 0)
				{
					animatedSprite2D.Play("idle");
					weapon_animatedSprite2D.Play("default");
				}
				else
				{
					if (_playerConfig.ExtraState == ExtraState.Dash)
					{
						animatedSprite2D.Play("dash");
					}
					else
					{
						animatedSprite2D.Play("run");
					}
				}
			}

		}

		if (_playerConfig.IFrame > 0)
		{
			// velocity.Y = MathF.Min(velocity.Y, 0);
			additional_x += _playerConfig.DashSpeed;
		}
		var des_speed = Speed * accerelation;
		tSpeed = Mathf.MoveToward(additional_x, tSpeed, 0.001f);

		if (_playerConfig.State == State.Jumping)
		{
			velocity.Y = JumpVelocity;
			_playerConfig.State = State.Idle;
		}

		if (velocity.X != 0)
		{
			// GD.Print(velocity);
			_playerConfig.State = State.Moving;
		}
		else
		{
			_playerConfig.State = State.Idle;
		}

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		float finalX = velocity.X;
		if (direction != Vector2.Zero)
		{
			finalX = direction.X * tSpeed;
		}
		else if (_playerConfig.IFrame <= 0)
		{
			finalX = Mathf.MoveToward(Velocity.X, 0, des_speed);
		}

		if (direction.X > 0)
		{
			_playerConfig.Facing = 1;
			animatedSprite2D.FlipH = false;
			weapon_animatedSprite2D.Position = new Vector2(5, weapon_animatedSprite2D.Position.Y);
			weapon_animatedSprite2D.FlipH = false;
		}
		else if (direction.X < 0)
		{
			_playerConfig.Facing = -1;
			animatedSprite2D.FlipH = true;
			weapon_animatedSprite2D.Position = new Vector2(-5, weapon_animatedSprite2D.Position.Y);
			weapon_animatedSprite2D.FlipH = true;
		}

		// if (_playerConfig.ExtraState == ExtraState.Dash)
		// {
		// 	_playerConfig.ExtraState = ExtraState.Default;
		// 	finalX += (float)direction.X * _playerConfig.DashSpeed;
		// }

		velocity.X = finalX;
		momentum = velocity;
		return velocity;
	}

	private void Shoot()
	{
		weapon_handler.Shoot();
	}

	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Keycode == Key.W && eventKey.Pressed)
            {
                _rawInput.Y = JumpVelocity;
            }

            if (eventKey.Keycode == Key.S && eventKey.Pressed)
            {
                _rawInput.Y = -1;
            }

            if (eventKey.Keycode == Key.A && eventKey.Pressed)
            {
                _rawInput.X = -1 * Speed;
            }

            if (eventKey.Keycode == Key.D && eventKey.Pressed)
            {
                _rawInput.X = Speed;
            }

            if (eventKey.Keycode == Key.Shift && !eventKey.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_Shift", parameters => { });
            }

            if (eventKey.Keycode == Key.Space && eventKey.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_Accept", Args =>
                {
                    var callback = (int)Args;
                    switch (callback)
                    {
                        case 0:
                            GD.Print("Input accepted");
                            break;
                        case 1:
                            GD.Print("Input rejected");
                            break;
                    }
                    // GD.Print(Args as string);
                    // GD.Print("Input accepted");
                }, _rawInput);
            }

            if (eventKey.Keycode == Key.K && eventKey.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_K", parameters => { });
            }

            if (eventKey.Keycode == Key.J && eventKey.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_J", parameters => { });
            }
        }

        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_Click_Left", parameters => { });
            }

            if (eventMouseButton.ButtonIndex == MouseButton.Right && eventMouseButton.Pressed)
            {
                _eventManager.TriggerEventAsThread("_Input_Click_Right", parameters => { });
            }
        }
    }
}
