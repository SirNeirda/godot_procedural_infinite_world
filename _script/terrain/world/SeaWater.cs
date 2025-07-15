using Godot;
using System;

[GlobalClass]                     // lets you add it from the editor
public partial class SeaWater : Node3D
{
    // ── OPTIONS ────────────────────────────────────────────────
    [Export] public bool InfiniteTiling = true;

    [Export(PropertyHint.Range, "1,1000,1")]
    public float RepositionThreshold = 50.0f;   // metres camera may move before we teleport

    [Export] public float SeaLevelY = 0.0f;      // vertical reference for your shader

    private const float PlaneSize = 10000.0f;    // scale applied once at start

    private Camera3D       _camera;
    private Node3D _meshInstance;

    // ── SET‑UP ─────────────────────────────────────────────────
    public override void _Ready()
    {
        _camera       = GameManager.Instance?.MainCamera;
        _meshInstance = (Node3D)this;        // script is attached to the plane

        if (_camera == null)
        {
            GD.PushError($"{Name}: GameManager.Instance.MainCamera is null!");
        }

        if (_meshInstance == null)
        {
            GD.PushError($"{Name}: Script must be on a MeshInstance3D.");
        }

        // Make the plane huge once; no per‑frame scaling required
        if (_meshInstance != null)
        {
            _meshInstance.Scale = new Vector3(PlaneSize, 1.0f, PlaneSize);
        }

        if (_camera != null)
        {
            SnapToCamera();
        }
    }

    // ── PER‑FRAME ──────────────────────────────────────────────
    public override void _Process(double delta)
    {
        if (!InfiniteTiling)
        {
            return;
        }
        if (_camera == null)
        {
            return;
        }

        Vector3 deltaXZ = _camera.GlobalPosition - GlobalPosition;
        deltaXZ.Y = 0.0f;   // ignore height difference

        if (Mathf.Abs(deltaXZ.X) > RepositionThreshold ||
            Mathf.Abs(deltaXZ.Z) > RepositionThreshold)
        {
            SnapToCamera();
        }
    }

    // ── HELPERS ────────────────────────────────────────────────
    private void SnapToCamera()
    {
        Vector3 camPos = _camera.GlobalPosition;
        GlobalPosition = new Vector3(camPos.X, SeaLevelY, camPos.Z);
    }
}
