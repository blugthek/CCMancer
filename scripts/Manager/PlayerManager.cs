using Godot;

public partial class PlayerManager : CharacterBody2D
{
    private EventManager _eventManager;
    private GameManager _gameManager;

    private const float Speed = 300.0f;
    private const float JumpVelocity = -400.0f;

    [Export] private Vector2 _rawInput = new Vector2(0, 0);

    public void Initialize(EventManager eventManager)
    {
        _eventManager = eventManager;
        GD.Print("PlayerManager initializing");
    }

    public override void _Ready()
    {
        _gameManager = GameManager.Instance;
        GD.Print("PlayerManager ready");
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
    }

    private float timer = 0f;
    public override void _PhysicsProcess(double delta)
    {
        //Simulate gravity
        _gameManager.PlayerMovementHandler = _rawInput;

        //TODO:: Make player do something (adaptor)

        _rawInput = _gameManager.PlayerMovementHandler;
        // _rawInput += GetGravity() * (float)delta;
        timer += (float)delta;
        if (timer < 0.333f) return;
        timer = 0f;
        SteppingFunction();
    }

    private void SteppingFunction()
    {
        GD.Print(_rawInput);
    }
}