using Godot;
using System;

public partial class WallGlassCabinetContainer : Openable
{

    public override void OnOpen()
    {
        _animPlayer.Play("glass_doorAction");
    }

    public override void OnClose()
    {

        _animPlayer.PlayBackwards("glass_doorAction");
    }

}
