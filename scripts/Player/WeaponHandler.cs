using System;
using System.Collections.Generic;
using Godot;
using static Player;

public partial class WeaponHandler : Node2D
{
    [Export] private PackedScene _bullet;
    [Export] private int _bulletCount = 2;
    [Export] private int _bulletSpread = 50;

    HitBox _hitBox = new HitBox();

    [Export] private Node2D _barrelTip;
    // [Export] private Area2D _hitBoxes = new Area2D();
    public float l_momentum = 0.0f;
    private PlayerConfig playerConfig = new PlayerConfig();
    private Player player = null;

    private EventManager _eventManager;

    public override void _Ready()
    {
        // var bulltest = (Bullet) _bullet.Instantiate();
        // CallDeferred("add_child", bulltest);
        //     if (_barrelTip != null) bulltest.GlobalPosition = _barrelTip.GlobalPosition;
        // bulltest.Position = Vector2.Zero;
        // _barrelTip = GetTree().Root.GetNode<Node2D>("Player/w_tip");
        // GD.Print(_barrelTip.Position);
        _eventManager = GameManager.Instance.GetEventManager();
        // GD.Print(player);
        // playerConfig = player.GetPlayerConfig();
        // l_momentum = player.Velocity.X;


        _hitBox.Initialize(GetNode<Area2D>("w_area"));
        _hitBox.HandleHit((Node2D body) =>
        {
            GD.Print("hit ", body);
            var Bull = body.GetParentOrNull<IDamageable>();
            //  GD.Print(Bull);
            if (Bull != null)
            {
                Bull.TakeDamage(10);
            }
            var newBullets = body.GetParentOrNull<Bullet>();
            if (newBullets != null)
            {
                playerConfig = player.GetPlayerConfig();
                l_momentum = player.Velocity.X;
                // GD.Print("Bullet hit");
                // // newBullets.QueueFree();
                GD.Print("Bounce", l_momentum, playerConfig.Facing,l_momentum*playerConfig.Facing);

                newBullets.Bounce(l_momentum, playerConfig.Facing);
            }
        });
        // _eventManager.TriggerEventAsThread("_Req_Player", callback => player = (Player)callback);

        // _hitBox.HandleHit(_on_w_area_area_entered);
        // _hitBoxes = GetNode<Area2D>("w_area");
        // _hitBoxes.Connect("area_entered", new Callable(this, nameof(OnBodyEntered)));
        GD.Print("WeaponHandler ready");
    }

    public void Shoot()
    {
        // _eventManager.TriggerEventAsThread("_Req_Player", callback => player = (Player)callback);
        // GD.Print(player);
        playerConfig = player.GetPlayerConfig();
        l_momentum = player.Velocity.X;

        // GD.Print("Shooting", _bulletCount,_barrelTip.GlobalPosition);
        // GD.Print("Bullet Spread: " + _bulletSpread);

        GD.Print("Facing: " + GlobalRotation, playerConfig.Facing, _barrelTip.Position.X);
        if (playerConfig.Facing == -1)
        {
            _barrelTip.Position = new Vector2(-6, _barrelTip.Position.Y);
        }
        else
        {
            _barrelTip.Position = new Vector2(6, _barrelTip.Position.Y);
        }
        // _bulletCount = 10;
        for (int i = 0; i < _bulletCount; i++)
        {
            var newBullets = (Bullet)_bullet.Instantiate();
            if (_barrelTip != null) newBullets.Position = _barrelTip.GlobalPosition;
            // if (playerConfig.Facing == -1) newBullets.GlobalRotation += 180;
            newBullets.Init(playerConfig, l_momentum);
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

    public override void _PhysicsProcess(double delta)
    {
        if (player == null)
        {
            
        _eventManager.TriggerEventAsThread("_Req_Player", callback => player = (Player)callback);
        }
        // GD.Print(_barrelTip.Position, _barrelTip.GlobalPosition);
        // var mousePosition = GetGlobalMousePosition();
        // LookAt(mousePosition);
        // GD.Print(_hitBoxes.HasOverlappingAreas());
    }

    private void OnBodyEntered(Node2D area)
    {
        GD.Print("area entered");
    }
}
