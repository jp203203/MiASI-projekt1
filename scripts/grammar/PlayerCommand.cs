using System;

public enum PlayerCommandType
{
	Move,
	Rotate,
	Take,
	Drop,
	Shield
}

public class PlayerCommand
{
	public PlayerCommandType Type;

	public int IntParam;
	public string StringParam;

	public PlayerCommand(PlayerCommandType type, int intParam = 0, string stringParam = null)
	{
		Type = type;
		IntParam = intParam;
		StringParam = stringParam;
	}
}
