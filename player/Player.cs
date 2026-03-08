using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// Movement constants
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float RunSpeed = 8.0f;

	// Stamina variables
	public float stamina = 100.0f;
	private float regenDelayTimer = 0f;
	private float regenDelay = 3f;

	// State
	public bool isRunning = false;
	public bool isCrouching = false;

	// Child nodes
	private CollisionShape3D playerCollision;
	private MeshInstance3D playerMesh;
	private RayCast3D playerRay;

	public override void _Ready()
	{
		playerCollision = GetNode<CollisionShape3D>("playerCollision");
		playerMesh = GetNode<MeshInstance3D>("playerBody");
		playerRay = GetNode<RayCast3D>("head/Camera3D/playerRay");

		// Safety checks
		if (playerCollision == null) GD.PrintErr("playerCollision not found!");
		if (playerMesh == null) GD.PrintErr("playerMesh not found!");
		if (playerRay == null) GD.PrintErr("playerRay not found!");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// INTERACT
		if (playerRay != null && playerRay.IsColliding())
		{
			var target = playerRay.GetCollider();
			GD.Print("Colliding with: ", target);
		}

		// CROUCH
		if (Input.IsActionJustPressed("crouch") && playerCollision != null && playerMesh != null)
		{
			isCrouching = !isCrouching;

			if (isCrouching)
			{
				playerCollision.Scale = new Vector3(1f, 0.5f, 1f);
				playerMesh.Scale = new Vector3(playerMesh.Scale.X, 0.5f, playerMesh.Scale.Z);
			}
			else
			{
				playerCollision.Scale = new Vector3(1f, 1f, 1f);
				playerMesh.Scale = new Vector3(playerMesh.Scale.X, 1f, playerMesh.Scale.Z);
			}
		}

		// GRAVITY
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		// JUMP
		if (Input.IsActionJustPressed("jump") && IsOnFloor() && playerCollision != null && playerMesh != null)
		{
			velocity.Y = JumpVelocity;

			isCrouching = false;
			playerCollision.Scale = new Vector3(1f, 1f, 1f);
			playerMesh.Scale = new Vector3(playerMesh.Scale.X, 1f, playerMesh.Scale.Z);
		}

		// MOVEMENT INPUT
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float currentSpeed = Speed;

		// RUNNING + STAMINA
		if (Input.IsActionPressed("run") && stamina > 0f && direction != Vector3.Zero && !isCrouching)
		{
			isRunning = true;
			currentSpeed = RunSpeed;

			stamina -= 10.5f * (float)delta;
			regenDelayTimer = regenDelay;
		}
		else
		{
			isRunning = false;

			if (regenDelayTimer > 0f)
				regenDelayTimer -= (float)delta;
			else
				stamina += 10.5f * (float)delta;
		}

		stamina = Mathf.Clamp(stamina, 0f, 100f);

		// APPLY MOVEMENT
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
