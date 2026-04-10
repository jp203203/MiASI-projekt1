using Godot;
using System;
using Godot.Collections;

public partial class LevelData : Resource
{
    [Export] public int LevelID = 0;
    [Export] public string LevelName = "Template";

    // shooter config
    //format: position -> {interval, counter}
    [Export] public Array<Vector2I> ShooterPositions = new();
    [Export] public Array<int> ShooterIntervals = new();
    [Export] public Array<int> ShooterCounters = new();

    // goal position
    [Export] public Vector2I GoalPosition = new Vector2I(0, 0);

    //convert to shooter data dict format
    public Dictionary GetShooterData()
    {
        Dictionary data = new Dictionary();

        for (int i = 0;  i < ShooterPositions.Count; i++)
        {
            if (i <  ShooterIntervals.Count && i < ShooterCounters.Count)
            {
                data[ShooterPositions[i]] = new Dictionary
                {
                    { "interval", Variant.From(ShooterIntervals[i]) },
                    { "counter", Variant.From(ShooterCounters[i])  }
                };
            }
        }

        return data;
    }
}
