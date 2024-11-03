using Godot;
using System;

public partial class CameraHandler : Camera2D
{
	private enum cameraState
	{
		normal,
		panning,
	}
	private cameraState _cameraState = cameraState.normal;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("CameraHandler ready");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
