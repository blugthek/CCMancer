using Godot;
using System;
using System.Threading;

public partial class Enemy : CharacterBody2D, IDamageable
{
	public const float Speed = 200.0f;
	public const float JumpVelocity = -400.0f;

	[Export] public int health = 1;
	[Export] public int damage = 1;

	[Export] public Area2D detect_area = null;

	[Export] public AnimatedSprite2D animatedSprite2D = null;
	[Export] public int attack_threshold = 50;
	[Export] public PackedScene _bullet = null;
	[Export] public Node2D _barrelTip = null;

	private enum State
	{
		Idle,
		Chase,
		Attack,
		Dead
	}
	private State state = State.Idle;
	private HitBox hitBox = new HitBox();

	private EventManager eventManager = new EventManager();

	private CharacterBody2D player = null;
	private int triggerBreak = 0;

	public override void _Ready()
	{
		// base._Ready();
		animatedSprite2D = GetNode<AnimatedSprite2D>("enemy_anim");
		
		detect_area = GetNode<Area2D>("detect_area");
		hitBox.Initialize(detect_area);
		hitBox.HandleHit(OnBodyEntered);
		// hitBox.HandleHit(this, nameof(OnBodyEntered));

		eventManager = GameManager.Instance.GetEventManager();
		eventManager.TriggerEventAsThread("_Req_Player", callback => player = (CharacterBody2D)callback);
		eventManager.RegisterEvent("_Player_Dead", callback =>
		{
			player = null;
			state = State.Idle;
			triggerBreak = 1;
		});
		// detect_area.Connect("area_entered", new Callable(this, nameof(OnBodyEntered)));
		animatedSprite2D.FrameChanged += () =>
		{
			var _bulletCount = 1;
			var _bulletSpread = 10;
			if (animatedSprite2D.Animation == "attack" && animatedSprite2D.Frame == 1)
			{
				for (int i = 0; i < _bulletCount; i++)
				{
					var newBullets = (Bullet)_bullet.Instantiate();
					if (_barrelTip != null) newBullets.Position = _barrelTip.GlobalPosition;
					// if (playerConfig.Facing == -1) newBullets.GlobalRotation += 180;
					newBullets.Init(facing, Velocity.X * 5.0f);
					if (_bulletCount > 1)
					{
						var arcRad = Mathf.DegToRad(_bulletSpread);
						var increment = arcRad / (_bulletCount - 1);
						newBullets.GlobalRotation = (
							GlobalRotation +
							increment * i -
							arcRad / 2
						);
					}
					GetTree().Root.CallDeferred("add_child", newBullets);
					//     var arcRad = Mathf.DegToRad(_bulletSpread);
					//     var increment = arcRad / (_bulletCount - 1);
					//     newBullets.GlobalRotation = (
					//         GlobalRotation +
					//         increment * i -
					//         arcRad / 2
					//     );
					// GD.Print("Shooting", _bulletCount);
					//     GD.Print("" + newBullets.Position);
					//     CallDeferred("add_child", newBullets);
					// GetTree().Root.CallDeferred("add_child", newBullets);
				}
			}
		};
	}

	private void OnBodyEntered(Node2D area)
	{
		GD.Print("area entered", area, area.GetParent<Node2D>().GetParentOrNull<Player>());
		// area.GetNode<Area2D>("HitBox").Connect("area_entered", new Callable(this, nameof(OnBodyEntered)));
		if (area.GetParentOrNull<IDamageable>() != null)
		{
			area.GetParent<IDamageable>().TakeDamage(damage);
		}

		if (area.GetParent<Node2D>().GetParentOrNull<Player>() != null)
		{
			GD.Print("Player detected");
		// eventManager.TriggerEventAsThread("_Req_Player", callback => player = (CharacterBody2D)callback);
			// player = area.GetParent<Node2D>().GetParentOrNull<CharacterBody2D>();
			state = State.Chase;
			GD.Print("Player detected",state,this.Name);
		}
	}

	private float dist = 0.02f;
	private int facing = 1;
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		if (state == State.Idle)
		{
			animatedSprite2D.Play("idle");
		}

		if (health <= 0)
		{
			state = State.Dead;
		}

		if (player == null && triggerBreak == 0)
		{
			
		eventManager.TriggerEventAsThread("_Req_Player", callback => player = (CharacterBody2D)callback);
		}

		// // Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (state != State.Idle && player != null && state != State.Dead)
		{
			dist = Util.DistanceTo(GlobalPosition, player.GlobalPosition);

			if (player.GlobalPosition.X < GlobalPosition.X)
			{
				velocity.X = -Speed;
				_barrelTip.Position = new Vector2(-9, _barrelTip.Position.Y);
				facing = -1;
				animatedSprite2D.FlipH
				= true;
			}
			else
			{
				velocity.X = Speed;
				_barrelTip.Position = new Vector2(9, _barrelTip.Position.Y);
				facing = 1;
				animatedSprite2D.FlipH
				= false;
			}
			if (dist < attack_threshold)
			{
				velocity.X = 0;
				state = State.Attack;
				animatedSprite2D.Play("attack");
			}
			else
			{
				state = State.Chase;
				animatedSprite2D.Play("walking");
			}
		}

		// // Handle Jump.
		// if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		// {
		// 	velocity.Y = JumpVelocity;
		// }

		// // Get the input direction and handle the movement/deceleration.
		// // As good practice, you should replace UI actions with custom gameplay actions.
		// Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		// if (direction != Vector2.Zero)
		// {
		// 	velocity.X = direction.X * Speed;
		// }
		// else
		// {
		// 	velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		// }

		Velocity = velocity;
		MoveAndSlide();
	}

	private int threadCount = 0;
	public void TakeDamage(int damage)
	{
		// GD.Print("Hit AHHHHHHHH");
		if (state == State.Dead)
		{
			return;
		}

		health -= damage;
		if (health <= 0 && threadCount == 0)
		{
			threadCount = 1;
			state = State.Dead;
			animatedSprite2D.Play("dying");
			Thread thread = new Thread(() =>
			{
				Thread.Sleep(1000);
				threadCount = 0;
				QueueFree();
			});
			thread.Start();
		}
		else
		{
			state = State.Dead;
			animatedSprite2D.Play("hurt");
			Thread thread = new Thread(() =>
			{
				Thread.Sleep(1000);
				state = State.Chase;
			});
			thread.Start();
			animatedSprite2D.Play("hurt");
		}
	}

}
