using Godot;
using System;
using System.Reflection.Metadata;

public partial class GameManager : Node2D
{
	public static GameManager Instance { get; private set; }
	public static EventManager EventManager { get; private set; }

	// [Export] private PlayerManager _playerManager;
	[Export] private MonsterSpawningManager _monsterSpawningManager;
	public Vector2 PlayerMovementHandler { get; set; }

	public EventManager GetEventManager()
	{
		return EventManager;
	}

	public void SpawnUnit(int index)
	{
		_monsterSpawningManager.SpawnMonster(index);
	}

	private Node2D PanningTarget = null;
	public void SetPannigTarget(Node2D panningTarget,float delay = 0)
	{
		if (panningTarget != null)
		{
		PanningTarget = panningTarget;
			EventManager.TriggerEventAsThread("SetPannigTarget", callback => {
				GD.Print("SetPannigTarget");
			}, panningTarget,delay);
		}
	}

	private void Initialize()
	{
		if (Instance != null && Instance != this)
		{
			GD.PushError("GameManager singleton already exists.");
			return;
		}
		Instance = this;
		EventManager = new EventManager();
		// _playerManager = new PlayerManager(this);
		// _playerManager.Initialize(EventManager);
		GD.Print("GameManager initialized");

		EventManager.RegisterEvent("_Input_Accept", parameters =>
		{
			GD.Print("Input accepting");
			var callback = (Action<object>)parameters[0];
			if (callback != null)
            {
                // Invoke the callback with the result
                var velocity = (Vector2)parameters[1];
                if (velocity == null)
                {
                    return;
                }
                if (velocity.X > 0)
                {
                    callback?.Invoke(1);
                }
                else
                {
                    callback?.Invoke(0);
                }
            }
        });
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Initialize();
		GD.Print("GameManager ready");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
			{
				// TODO:: Make player do something (adaptor)
			}
		}
	}


}
