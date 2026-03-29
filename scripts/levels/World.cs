using Godot;
using System;
using System.Runtime.InteropServices.JavaScript;

public partial class World : Node2D
{
    public PlayerCharacter PlayerCharacter;
    public TileMapLayer BoundaryLayer;

    [Export]
    public Clock Clock;

    // level data ref here?

    public override void _Ready()
    {
        // load level data here probably

        Clock.Pause();
    }

    public void StartCodeExecution()
    {
        Clock.Resume();
    }

    public void ResetWorld()
    {
        PlayerCharacter.ResetToInitial();

        Clock.Reset();
        Clock.Pause();
    }

    public bool IsLevelComplete()
    {
        // logic here when level data (with goal position) is added

        return false;
    }
}
