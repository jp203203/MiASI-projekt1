using Godot;
using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

public partial class GrammarWindow : Panel
{
	private TextEdit _input;
	
	public override void _Ready()
	{
		_input = GetNode<TextEdit>("TextEdit");
	}
	
	private void OnParseButtonPressed()
	{
		string text = _input.Text;
		
		try
		{
			var inputStream = new AntlrInputStream(text);
			var lexer = new GameLexer(inputStream);
			var tokens = new CommonTokenStream(lexer);
			var parser = new Game(tokens);
			
			var tree = parser.program();

			var visitor = new GameVisitor();
			int result = visitor.Visit(tree);
		}
		catch (Exception ex)
		{
			GD.Print($"Error: {ex.Message}");
		}
	}
}
