using System;
using System.Collections.Generic;

public enum PlayerCommandType
{
	Move,
	Rotate,
	Take,
	Drop,
	Shield,
	If,
	While,
}

public class PlayerCommand
{
	public PlayerCommandType Type;

	public int IntParam;
	public string StringParam;
	
	public Func<bool> Condition;
	
	// IF
    public List<PlayerCommand> ThenBody;
    public List<PlayerCommand> ElseBody;

    // WHILE
    public List<PlayerCommand> Body;

	public PlayerCommand(PlayerCommandType type, int intParam = 0, string stringParam = null)
	{
		Type = type;
		IntParam = intParam;
		StringParam = stringParam;
	}
}
