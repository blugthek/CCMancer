using Godot;
using System;
using System.Threading;

public partial class CameraHandler : Camera2D
{
	private enum cameraState
	{
		normal,
		panning,
		panBack,
	}
	private cameraState _cameraState = cameraState.normal;

	[Export] private Node2D target = null;

	private float _initY = 0.0f;

	private EventManager _eventManager;

	private Node2D _panningTarget = null;
	private int panningWaiting = 0;
	private int focusTarget = 0;
	private int cameraWaitingParam = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_eventManager = GameManager.Instance.GetEventManager();

		_eventManager.RegisterEvent("_Player_Dead", callback => target = null);

		_eventManager.RegisterEvent("SetPannigTarget", parameters =>
		{
			panningWaiting = 1;
			_panningTarget = (Node2D)parameters[1];
			if (_panningTarget != null)
			{
				GD.Print("Panning");
				_cameraState = cameraState.panning;
				var callback = (Action<object>)parameters[0];
				if (callback != null)
				{
					callback?.Invoke(null);
				}
			}
		});

		_eventManager.RegisterEvent("SetPannigTarget", parameters =>
		{
			GD.Print("Panning Duplicate test");
			var delay = parameters[2] != null ? (float) parameters[2] : 0.995f;
			if (delay != 0.995f)
			{
				GD.Print("Panning Duplicate");
				while (focusTarget == 0)
				{
					// GD.Print("Panning Duplicate waiting");
					Thread.Sleep(10);
				}
				GD.Print("Panning Duplicate done");
				cameraWaitingParam = 1;
				while (cameraWaitingParam > 0)
				{
					if (delay == -1)
					{
					}
					else if (delay > 0)
					{
						delay--;
					}
					else
					{
						cameraWaitingParam = 0;
					}
					Thread.Sleep(1);
				}
				GD.Print("Panning Duplicate delay done", delay);
				panningWaiting = 0;
			}
		});

		_eventManager.RegisterEvent("_Input_Shift", parameters =>
		{
			cameraWaitingParam = 0;
			// _cameraState = cameraState.panBack;
		});

		_initY = this.GlobalPosition.Y;
		GD.Print("CameraHandler ready");
	}


	private Vector2 targetPos = new Vector2(0, 0);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// GD.Print("CameraHandler process",this.GlobalPosition);
		if (target == null) return;

		if (_cameraState == cameraState.normal && target != null)
		{
			targetPos.X = target.GlobalPosition.X;
			targetPos.Y = target.GlobalPosition.Y;
			targetPos.Y = Mathf.Min(targetPos.Y, _initY);
			this.GlobalPosition = targetPos;
		}
		else if (_cameraState == cameraState.panning)
		{
			// targetPos = this.GlobalPosition;
			// var HandDelta = targetPos + _panningTarget.GlobalPosition;
			// targetPos += HandDelta * (float)delta;
			// // targetPos = targetPos + _panningTarget.GlobalPosition;
			// // targetPos.X = Mathf.MoveToward(targetPos.X, _panningTarget.GlobalPosition.X, 3.33f);
			// // targetPos.Y = Mathf.MoveToward(targetPos.Y, _panningTarget.GlobalPosition.Y, 3.33f);
			// // Zoom = new Vector2(0.5f, 0.5f);
			// this.GlobalPosition = targetPos;
			// GD.Print("Panning", this.GlobalPosition, _panningTarget.GlobalPosition);
			// GD.Print("Zoom", Zoom);
			if (Util.DistanceTo(this.GlobalPosition, _panningTarget.GlobalPosition) < 3.33f)
			{
				focusTarget = 1;
			}

			if (panningWaiting == 0)
			{
				_cameraState = cameraState.panBack;
				focusTarget = 0;
			}

			targetPos = Util.Lerp(targetPos, _panningTarget.GlobalPosition, (float)delta);
			this.GlobalPosition = targetPos;
		}

		if (_cameraState == cameraState.panBack)
		{
			var posOffset = target.GlobalPosition;
			posOffset.Y = Mathf.Min(posOffset.Y, _initY);

			if (Util.DistanceTo(this.GlobalPosition, posOffset) < 0.33f )
			{
				_cameraState = cameraState.normal;
			}

			targetPos = Util.Lerp(targetPos, posOffset, (float) delta * 6.33f);
			this.GlobalPosition = targetPos;
		}
	}
}
