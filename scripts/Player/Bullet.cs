using Godot;
using System;
using System.Threading;

public partial class Bullet : CharacterBody2D
{
	[Export] private PackedScene _bullet;
	[Export] private float _sp = 45;
	[Export] private int _rn;
	[Export] private Vector2 _dir = Vector2.Right;

	[Export] private CanvasTexture canvasTexture = new CanvasTexture();

	private Area2D _hitBoxes = new Area2D();

	private HitBox hitBox = new HitBox();
	private EventManager _eventManager = new EventManager();
	private int BulletType = 0;

	private AnimatedSprite2D _sprite = new AnimatedSprite2D();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("sprite");
		_eventManager = GameManager.Instance.GetEventManager();
		_hitBoxes = GetNode<Area2D>("hitBox");
		if (_hitBoxes == null)
		{
			GD.PrintErr("HitBox node not found!");
			return;
		}
		hitBox.Initialize(_hitBoxes);
		hitBox.HandleHit((Node2D body) =>
		{
			if (body == null) return;
			var dmg = body.GetParentOrNull<IDamageable>();
			if (dmg != null && BulletType == 0)
			{
				dmg.TakeDamage(10);
				_sprite.Play("break");
				// _sprite.Material.Set
				_sp = 0;
				GD.Print("Hit " + body.Name);
				Velocity = Vector2.Zero;

			}
			var player = body.GetParentOrNull<Player>();
			if (player != null && BulletType == 1)
			{
				// GD.Print("Player detected");
				// player = area.GetParent<Node2D>().GetParentOrNull<CharacterBody2D>();
				// state = State.Chase;
				_sprite.Play("break");
				_sp = 0;
				player.TakeDamage(10);
				Velocity = Vector2.Zero;
			}
			GD.Print("Hit " + body.Name);
		});
		// hitBox.HandleHit(OnBodyEntered);

		// _hitBoxes.Connect("area_entered", new Callable(this, nameof(OnBodyEntered)));
		// _hitBoxes.Connect("area_entered", new Callable(this, nameof(()=> {})));
		_dir = Vector2.Right.Rotated(GlobalRotation);
		// _eventManager.RegisterEvent("FreeQueue", parameters =>
		// {
		// 	Thread.Sleep(_rn);
		// 	QueueFree();
		// });
		// _eventManager.TriggerEventAsThread("FreeQueue", _ => {
		// 	_eventManager.UnregisterEvent("FreeQueue", _ => {});
		//  });
		_sprite.FrameChanged += () =>
		{
			if (_sprite.Animation == "break" && _sprite.Frame == 7)
			{
				QueueFree();
			}
		};
	}

	private double lifeTime = 3.0f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (BulletType == 0)
		{
			_sprite.Modulate = Color.Color8(255, 255, 255, 255);
		}
		else
		{
			_sprite.Modulate = Color.Color8(255, 0, 0, 255);
		}
		lifeTime -= delta;
		if (lifeTime <= 0)
		{
			_sprite.Play("break");
			_sp = 0;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = _dir * _sp * (float)delta;
		// Velocity = velocity;
		var collision = MoveAndCollide(velocity);
		// GD.Print("Bullet physics process, pos: " + Position);
	}

	public void Init(Player.PlayerConfig playerConfig, float momentum)
	{
		_sp = _sp * playerConfig.Facing;
		_sp += momentum;
		// Thread thread = new Thread(() =>
		// {
		// 	Thread.Sleep(5000);
		// 	if (this != null)
		// 	{
		// 		CallDeferred(nameof(PlayBreak));
		// 	}
		// });
		// thread.Start();
	}

	private void PlayBreak()
	{
		_sprite.Play("break");
	}

	public void Init(int facing, float momentum)
	{
		_sp = _sp * facing;
		_sp += momentum;
		BulletType = 1;
		// Thread thread = new Thread(() =>
		// {
		// 	Thread.Sleep(5000);
		// 	if (this != null)
		// 	{
		// 		CallDeferred(nameof(PlayBreak));
		// 	}
		// });
		// thread.Start();
	}

	// private void AnimatedBoom()
	// {
	// 	_sprite.Play("break");
	// }

	private void OnBodyEntered(Node2D body)
	{
		GD.Print("Hit!");
		// QueueFree();
		// hitBox.HandleHit(OnBodyEntered);
	}

	private int b_trigger = 0;
	public void Bounce(float momentum, int facing)
	{
		GD.Print("Bounce");
		BulletType = 0;
		if (b_trigger == 0)
		{
			b_trigger = 1;
			_sp = MathF.Abs(_sp * 2.55f);
			_sp *= facing;
			_sp += momentum;
			_sprite.Play("speed");
		}
	}
}
