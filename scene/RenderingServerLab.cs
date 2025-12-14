using Godot;
using System;

public partial class RenderingServerLab : Node3D
{
    private Node3D _someNode3D;

    private Mesh _mesh;

    public override void _Ready()
    {
        Rid scenario = GetWorld3D().Scenario;
        _mesh = ResourceLoader.Load<Mesh>("sadasd");
        
        _someNode3D = ResourceLoader.Load<Node3D>("res://assets/collectables/test_cube/collectable_cube.tscn");
        Rid instance = RenderingServer.InstanceCreate2(scenario, _mesh.GetRid());
    }
}
