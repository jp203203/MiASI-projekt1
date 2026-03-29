using Godot;
using System;

[GlobalClass]
public partial class Clock : Node
{
    [Signal]
    public delegate void TickEventHandler(int currentTurn);

    [Export]
    private float _stepDuration = 32f / 150f + 0.05f;

    private int _turnCount = 0;
    private Timer _timer;
    private bool _paused = false;

    public override void _Ready()
    {
        _timer = new Timer();
        _timer.WaitTime = _stepDuration;
        _timer.Autostart = true;
        _timer.OneShot = false;

        AddChild(_timer);
        _timer.Timeout += OnTimerTimeout;

        Pause();
    }

    private void OnTimerTimeout()
    {
        _turnCount++;
        EmitSignal(SignalName.Tick, _turnCount);
    }

    public void Pause()
    {
        _paused = true;
        _timer.Stop();
    }

    public void Resume()
    {
        _paused = false;
        _timer.Start();
    }

    public void Reset()
    {
        _turnCount = 0;
    }
}
