using Godot;
using System;
using Antlr4.Runtime.Misc;

public class GameVisitor : GameBaseVisitor<int>
{
	public override int VisitMoveCommand(Game.MoveCommandContext context)
	{
		GD.Print("TEMP: move");
		return 0;
	}
	
	public override int VisitRotateCommand(Game.RotateCommandContext context)
	{
		GD.Print("TEMP: rotate");
		return 0;
	}
	
	public override int VisitTakeCommand(Game.TakeCommandContext context)
	{
		GD.Print("TEMP: take");
		return 0;
	}
	
	public override int VisitDropCommand(Game.DropCommandContext context)
	{
		GD.Print("TEMP: drop");
		return 0;
	}
	
	public override int VisitShieldCommand(Game.ShieldCommandContext context)
	{
		GD.Print("TEMP: shield");
		return 0;
	}
}
