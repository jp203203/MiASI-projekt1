using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class LevelUI : Control
{
    private TextEdit _codeEditor;
    private Button _runButton;
    private RichTextLabel _errorDisplay;
    private Button _zoomInButton;
    private Button _zoomOutButton;

    // world refs
    private SubViewportContainer _worldViewportContainer;
    private SubViewport _worldViewport;

    private World _world;
    private PlayerCharacter _playerCharacter;
    private Camera2D _camera;

    // camera constants
    private static readonly float[] ZoomLevels = { 2.0f, 2.2f, 2.4f, 2.6f, 2.8f, 3.0f, 3.2f };
    private const int StartZoomIdx = 4;

    // camera control
    private Rect2 _worldBounds;
    private bool _isDragging = false;
    private Vector2 _dragStartPosition = Vector2.Zero;
    private Vector2 _cameraStartPosition = Vector2.Zero;
    private int _currentZoomLevel = StartZoomIdx;

    public override void _Ready()
    {
        _codeEditor = GetNode<TextEdit>("HSplitContainer/RightPanel/VBoxContainer/VSplitContainer/CodeEditorContainer/CodeEditor");
        _runButton = GetNode<Button>("HSplitContainer/RightPanel/VBoxContainer/RunButton");
        _errorDisplay = GetNode<RichTextLabel>("HSplitContainer/RightPanel/VBoxContainer/VSplitContainer/VBoxContainer/ErrorContainer/ErrorDisplay");
        _zoomInButton = GetNode<Button>("MarginContainer/ZoomButtonsContainer/ZoomInButton");
        _zoomOutButton = GetNode<Button>("MarginContainer/ZoomButtonsContainer/ZoomOutButton");

        _worldViewportContainer = GetNode<SubViewportContainer>("HSplitContainer/WorldViewportContainer");
        _worldViewport = GetNode<SubViewport>("HSplitContainer/WorldViewportContainer/WorldViewport");

        LoadLevelWorld();

        // CONNECT TO INTERPRETER HERE

        // connect signals
        _runButton.Pressed += OnRunPressed;
        _codeEditor.TextChanged += OnCodeChanged;
        _zoomInButton.Pressed += OnZoomInPressed;
        _zoomOutButton.Pressed += OnZoomOutPressed;

        _codeEditor.SyntaxHighlighter = CreateSyntaxHighlighter();

        // add and configure gutter for line numbers
        _codeEditor.AddGutter(0);
        _codeEditor.SetGutterName(0, "lineNumbers");
        _codeEditor.SetGutterType(0, TextEdit.GutterType.String);
        _codeEditor.SetGutterDraw(0, true);
        _codeEditor.SetGutterWidth(0, 40);

        UpdateGutterLineNumbers();

        _ = InitCameraAsync();
    }

    private void LoadLevelWorld()
    {
        // load the world scene
        var worldScene = GD.Load<PackedScene>("res://scenes/levels/Template.tscn");

        if (worldScene == null)
        {
            GD.PushError("Failed to load world scene");
            return;
        }

        // instantiate and add to viewport
        _world = (World) worldScene.Instantiate();
        _worldViewport.AddChild(_world);

        // get refs to world components
        _playerCharacter = _world.GetNode<PlayerCharacter>("PlayerCharacter");
        _camera = _world.GetNode<Camera2D>("Camera2D");

        if (_playerCharacter == null ||  _camera == null)
        {
            GD.PushError("World scene must have both PlayerCharacter and Camera2D nodes");
            return;
        }

        // get world bounds
        _worldBounds = _world.GetWorldBounds();
    }

    private void OnRunPressed()
    {
        _errorDisplay.Text = "";  // clear the error display

        _runButton.Disabled = true;
        _runButton.Text = "RUNNING...";

        // SYNTAX VALIDATION AND CODE EXECUTION HERE

        // check if level was completed
        if (_world.IsLevelComplete())
            OnLevelCompleted();

        // reset world after code execution
        _world.ResetWorld();

        _runButton.Disabled = false;
        _runButton.Text = "▶   RUN";
    }

    private void OnLevelCompleted()
    {
        Console.WriteLine("Level completed!");  // nothing more here since we're not adding campaign or progression - nothing to unlock
    }

    private void OnCodeChanged()
    {
        UpdateGutterLineNumbers();
        _errorDisplay.Text = "";
    }

    private void OnZoomInPressed()
    {
        if (_camera is null) return;

        if (_currentZoomLevel == ZoomLevels.Length - 1) return;

        _currentZoomLevel++;
        _camera.Zoom = Vector2.One * ZoomLevels[_currentZoomLevel];
    }

    private void OnZoomOutPressed()
    {
        if (_camera is null) return;

        if (_currentZoomLevel == 0) return;

        _currentZoomLevel--;
        _camera.Zoom = Vector2.One * ZoomLevels[_currentZoomLevel];
    }

    private SyntaxHighlighter CreateSyntaxHighlighter()
    {
        CodeHighlighter highlighter = new CodeHighlighter();

        highlighter.SymbolColor = new Color("ffffff");  // white
        highlighter.FunctionColor = new Color("9751bd");  // purple

        highlighter.NumberColor = new Color("7eff6e");  // bright green

        Color blue = new Color("6ecfff");
        Color orange = new Color("ffa06e");
        Color yellow = new Color("ffeb6e");

        // commands
        highlighter.AddKeywordColor("MOVE", blue);
        highlighter.AddKeywordColor("ROTATE", blue);
        highlighter.AddKeywordColor("TAKE", blue);
        highlighter.AddKeywordColor("DROP", blue);
        highlighter.AddKeywordColor("SHIELD", blue);

        // conditional and loops
        highlighter.AddKeywordColor("IF", orange);
        highlighter.AddKeywordColor("ELSE", orange);
        highlighter.AddKeywordColor("WHILE", orange);
        highlighter.AddKeywordColor("REPEAT", orange);

        // other keywords
        highlighter.AddKeywordColor("LEFT", yellow);
        highlighter.AddKeywordColor("RIGHT", yellow);
        highlighter.AddKeywordColor("UP", yellow);
        highlighter.AddKeywordColor("DOWN", yellow);

        // comments
        highlighter.AddColorRegion("#", "", new Color("396637"));  // dark green

        return highlighter;
    }

    private void UpdateGutterLineNumbers()
    {
        int lineCount = _codeEditor.GetLineCount();
        for (int i = 0; i < lineCount; i++)
        {
            _codeEditor.SetLineGutterText(i, 0, (i + 1).ToString());
        }
    }

    private async Task InitCameraAsync()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        SetupInitialCamera();
    }

    private void SetupInitialCamera()
    {
        if (_camera == null) return;

        _camera.Zoom = Vector2.One * ZoomLevels[_currentZoomLevel];

        // center camera on player character
        if (_playerCharacter != null)
            _camera.GlobalPosition = _playerCharacter.GlobalPosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _dragStartPosition = mouseButton.Position;
                    _cameraStartPosition = _camera.GlobalPosition;
                }
                else
                {
                    _isDragging = false;
                }
            }
        } else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            Vector2 dragOffset = (mouseMotion.Position - _dragStartPosition) * -1.0f;
            _camera.GlobalPosition = _cameraStartPosition + dragOffset;
        }
    }
}
