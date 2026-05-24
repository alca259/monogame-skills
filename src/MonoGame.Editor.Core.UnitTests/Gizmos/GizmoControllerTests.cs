namespace MonoGame.Editor.Core.UnitTests.Gizmos;

public sealed class GizmoControllerTests
{
    // ── Default state ────────────────────────────────────────────────────────

    [Fact]
    public void Mode_Default_IsSelect()
    {
        GizmoController ctrl = new();
        Assert.Equal(GizmoMode.Select, ctrl.Mode);
    }

    [Fact]
    public void ShowGrid_Default_IsTrue()
    {
        GizmoController ctrl = new();
        Assert.True(ctrl.ShowGrid);
    }

    [Fact]
    public void GridCellSize_Default_Is32()
    {
        GizmoController ctrl = new();
        Assert.Equal(32f, ctrl.GridCellSize, precision: 3);
    }

    [Fact]
    public void GridCellSize_SetBelowOne_ClampedToOne()
    {
        GizmoController ctrl = new() { GridCellSize = -5f };
        Assert.Equal(1f, ctrl.GridCellSize, precision: 3);
    }

    [Fact]
    public void GridCellSize_SetZero_ClampedToOne()
    {
        GizmoController ctrl = new() { GridCellSize = 0f };
        Assert.Equal(1f, ctrl.GridCellSize, precision: 3);
    }

    // ── BeginDrag ────────────────────────────────────────────────────────────

    [Fact]
    public void BeginDrag_SelectMode_ReturnsFalse()
    {
        GizmoController ctrl = new() { Mode = GizmoMode.Select };
        EditorGameObject obj = new() { Name = "E" };

        bool hit = ctrl.BeginDrag(0, 0, 0, 0, 0, 0, obj);

        Assert.False(hit);
    }

    [Fact]
    public void BeginDrag_MoveMode_NoHandleHit_ReturnsFalse()
    {
        GizmoController ctrl = new() { Mode = GizmoMode.Move };
        EditorGameObject obj = new() { Name = "E" };

        // Click far from origin (500, 500), object at screen (0, 0) — no handle nearby
        bool hit = ctrl.BeginDrag(500, 500, 0, 0, 0, 0, obj);

        Assert.False(hit);
    }

    [Fact]
    public void BeginDrag_MoveMode_XAxisHit_ReturnsTrue()
    {
        GizmoController ctrl = new() { Mode = GizmoMode.Move };
        EditorGameObject obj = new() { Name = "E", Position = EditorVector2.Zero };

        // X handle is at (origin.X + ArrowLength/2, origin.Y) ≈ (40, 0) in screen coords
        bool hit = ctrl.BeginDrag(40, 0, 0, 0, 0, 0, obj);

        Assert.True(hit);
    }

    [Fact]
    public void BeginDrag_RotateMode_CircleHit_ReturnsTrue()
    {
        GizmoController ctrl = new() { Mode = GizmoMode.Rotate };
        EditorGameObject obj = new() { Name = "E" };

        // Rotate circle is at radius GizmoController.RotateRadius from origin
        float r = GizmoController.RotateRadius;
        bool hit = ctrl.BeginDrag(r, 0, 0, 0, r, 0, obj);

        Assert.True(hit);
    }

    // ── EndDrag ──────────────────────────────────────────────────────────────

    [Fact]
    public void EndDrag_WithoutBeginDrag_ReturnsNull()
    {
        GizmoController ctrl   = new() { Mode = GizmoMode.Move };
        EditorGameObject obj   = new() { Name = "E" };

        IEditorCommand? cmd = ctrl.EndDrag(obj, ctrlHeld: false);

        Assert.Null(cmd);
    }

    [Fact]
    public void EndDrag_WithNullSelected_ReturnsNull()
    {
        GizmoController ctrl = new() { Mode = GizmoMode.Move };
        EditorGameObject obj = new() { Name = "E", Position = EditorVector2.Zero };

        ctrl.BeginDrag(40, 0, 0, 0, 0, 0, obj);
        IEditorCommand? cmd = ctrl.EndDrag(null, ctrlHeld: false);

        Assert.Null(cmd);
    }

    [Fact]
    public void EndDrag_AfterMoveDrag_ReturnsMoveEntityCommand()
    {
        GizmoController  ctrl = new() { Mode = GizmoMode.Move };
        EditorGameObject obj  = new() { Name = "E", Position = new EditorVector2(0, 0) };

        ctrl.BeginDrag(40, 0, 0, 0, 0f, 0f, obj);
        ctrl.UpdateDrag(10f, 0f, 40, 0, 0, 0, obj); // move 10 world units on X
        IEditorCommand? cmd = ctrl.EndDrag(obj, ctrlHeld: false);

        Assert.IsType<MoveEntityCommand>(cmd);
    }

    [Fact]
    public void EndDrag_AfterRotateDrag_ReturnsRotateEntityCommand()
    {
        GizmoController  ctrl = new() { Mode = GizmoMode.Rotate };
        EditorGameObject obj  = new() { Name = "E" };
        float r = GizmoController.RotateRadius;

        ctrl.BeginDrag(r, 0, 0, 0, r, 0, obj);
        ctrl.UpdateDrag(0, 0, 0, r, 0, 0, obj);
        IEditorCommand? cmd = ctrl.EndDrag(obj, ctrlHeld: false);

        Assert.IsType<RotateEntityCommand>(cmd);
    }

    // ── Snap ────────────────────────────────────────────────────────────────

    [Fact]
    public void SnapToGrid_AlignedPosition_ReturnsUnchanged()
    {
        GizmoController ctrl = new() { GridCellSize = 32f };

        EditorVector2 result = ctrl.SnapToGrid(new EditorVector2(64f, 96f));

        Assert.Equal(new EditorVector2(64f, 96f), result);
    }

    [Fact]
    public void SnapToGrid_UnalignedPosition_SnapsToNearestCell()
    {
        GizmoController ctrl = new() { GridCellSize = 32f };

        EditorVector2 result = ctrl.SnapToGrid(new EditorVector2(20f, 50f));

        Assert.Equal(new EditorVector2(32f, 64f), result);
    }

    [Fact]
    public void SnapToGrid_NegativePosition_SnapsCorrectly()
    {
        GizmoController ctrl = new() { GridCellSize = 32f };

        EditorVector2 result = ctrl.SnapToGrid(new EditorVector2(-20f, -50f));

        Assert.Equal(new EditorVector2(-32f, -64f), result);
    }

    [Fact]
    public void EndDrag_WithCtrlHeld_SnapsPosition()
    {
        GizmoController  ctrl = new() { Mode = GizmoMode.Move, GridCellSize = 32f };
        EditorGameObject obj  = new() { Name = "E", Position = new EditorVector2(0, 0) };

        ctrl.BeginDrag(40, 0, 0, 0, 0f, 0f, obj);
        // Drag to world (20, 0) — not aligned to 32px grid
        ctrl.UpdateDrag(20f, 0f, 40, 0, 0, 0, obj);
        IEditorCommand? cmd = ctrl.EndDrag(obj, ctrlHeld: true);

        Assert.NotNull(cmd);
        // After snap, position should be at nearest grid cell (32, 0)
        Assert.Equal(new EditorVector2(32f, 0f), obj.Position);
    }
}
