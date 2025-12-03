using Godot;
using System;

public partial class GlassCabinetContainer : Openable
{

    public override void OnOpen()
    {
        _animPlayer.Play("door_open");
    }

    public override void OnClose()
    {
        _animPlayer.PlayBackwards("door_open");
    }
}
