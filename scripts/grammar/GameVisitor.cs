using Godot;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;

public class GameVisitor : GameBaseVisitor<object>
{
	private Dictionary<string, int> variables = new Dictionary<string, int>();
	private Dictionary<string, Game.ProcedureDeclContext> procedures
	= new Dictionary<string, Game.ProcedureDeclContext>();
	private Stack<Dictionary<string, int>> scopes;
	private PlayerCharacter player;
	
	public GameVisitor(PlayerCharacter player)
	{
		this.player = player;
		
		scopes = new Stack<Dictionary<string, int>>();
		scopes.Push(new Dictionary<string, int>()); // global scope
	}
	
	public override object VisitMoveCommand(Game.MoveCommandContext context)
	{
		int tiles = (int)Visit(context.expr());

		player.EnqueueCommand(
			new PlayerCommand(PlayerCommandType.Move, intParam: tiles)
		);

		return null;
	}
	
	public override object VisitRotateCommand(Game.RotateCommandContext context)
	{
		string direction = context.direction().GetText();

		player.EnqueueCommand(
			new PlayerCommand(PlayerCommandType.Rotate, stringParam: direction)
		);

		return null;
	}
	
	public override object VisitTakeCommand(Game.TakeCommandContext context)
	{
		player.EnqueueCommand(new PlayerCommand(PlayerCommandType.Take));
		
		return null;
	}
	
	public override object VisitDropCommand(Game.DropCommandContext context)
	{
		player.EnqueueCommand(new PlayerCommand(PlayerCommandType.Drop));
		
		return null;
	}
	
	public override object VisitShieldCommand(Game.ShieldCommandContext context)
	{
		player.EnqueueCommand(new PlayerCommand(PlayerCommandType.Shield));
		
		return null;
	}
	
	public override object VisitIfStatement(Game.IfStatementContext context)
	{
	    var savedQueue = player.CaptureQueue();

	    // THEN
	    player.ClearQueue();
	    Visit(context.block(0));
	    var thenBody = player.DrainQueueToList();

	    // ELSE (if exists)
	    List<PlayerCommand> elseBody = null;

	    if (context.ELSE() != null)
	    {
	        player.ClearQueue();
	        Visit(context.block(1));
	        elseBody = player.DrainQueueToList();
	    }

	    player.RestoreQueue(savedQueue);

	    player.EnqueueCommand(new PlayerCommand(PlayerCommandType.If)
	    {
	        Condition = () => (bool)Visit(context.condition()),
	        ThenBody = thenBody,
	        ElseBody = elseBody
	    });

	    return null;
	}
	
	public override object VisitWhileStatement(Game.WhileStatementContext context)
	{
	    var savedQueue = player.CaptureQueue();

	    player.ClearQueue();

	    Visit(context.block());

	    var bodyCommands = player.DrainQueueToList();

	    player.RestoreQueue(savedQueue);

	    player.EnqueueCommand(new PlayerCommand(PlayerCommandType.While)
	    {
	        Condition = () => (bool)Visit(context.condition()),
	        Body = bodyCommands
	    });

	    return null;
	}
	
	public override object VisitRepeatStatement(Game.RepeatStatementContext context)
	{
		int count = Math.Max(0, (int)Visit(context.expr()));

		for (int i = 0; i < count; i++)
		{
			Visit(context.block());
		}

		return null;
	}
	
	public override object VisitProcedureDecl(Game.ProcedureDeclContext context)
	{
		string name = context.ID().GetText();

		procedures[name] = context;

		GD.Print($"Defined procedure: {name}");

		return null;
	}
	
	// procedure call is defined as both statement and expression -
	// having it only as statement made the grammar see the
	// recursive call as variable instead of procedure.
	// this approach fixed recursive calls
	
	private List<int> EvaluateArgs(Game.ArgListContext argList)
	{
		var args = new List<int>();

		if (argList == null)
			return args;

		foreach (var expr in argList.expr())
			args.Add((int)Visit(expr));

		return args;
	}
	
	// procedure call as expr
	public override object VisitProcCallExpr(Game.ProcCallExprContext context)
	{
		return ExecuteCall(
			context.ID().GetText(),
			EvaluateArgs(context.argList())
		);
	}
	
	// procedure call as statement
	public override object VisitProcedureCall(Game.ProcedureCallContext context)
	{
		ExecuteCall(
			context.ID().GetText(),
			EvaluateArgs(context.argList())
		);

		return null;
	}
	
	// procedure as expr and as statement both lead here
	private object ExecuteCall(string name, List<int> argValues)
	{
		if (!procedures.ContainsKey(name))
			throw new Exception($"Undefined procedure: {name}");

		var proc = procedures[name];

		var paramNames = new List<string>();

		if (proc.paramList() != null)
		{
			foreach (var id in proc.paramList().ID())
				paramNames.Add(id.GetText());
		}

		if (argValues.Count != paramNames.Count)
			throw new Exception($"Procedure '{name}' expects {paramNames.Count} args, got {argValues.Count}");

		var localScope = new Dictionary<string, int>();

		for (int i = 0; i < paramNames.Count; i++)
			localScope[paramNames[i]] = argValues[i];

		scopes.Push(localScope);

		Visit(proc.block());

		scopes.Pop();

		return null;
	}
	
	public override object VisitBlock(Game.BlockContext context)
	{
		foreach (var stmt in context.statement())
		{
			Visit(stmt);
		}
		
		return null;
	}
	
	public override object VisitAssignment(Game.AssignmentContext context)
	{
		string name = context.ID().GetText();
		int value = (int)Visit(context.expr());

		SetVariable(name, value);

		GD.Print($"Set {name} = {value}");

		return null;
	}
	
	public override object VisitCondition(Game.ConditionContext context)
	{
		return VisitChildren(context);
	}
	
	public override object VisitComparison(Game.ComparisonContext context)
	{
		int left = (int)Visit(context.expr(0));
		int right = (int)Visit(context.expr(1));

		string op = context.compOp().GetText();

		var opType = context.compOp().Start.Type;

		switch (opType)
		{
			case Game.EQ: return left == right;
			case Game.NEQ: return left != right;
			case Game.GT: return left > right;
			case Game.LT: return left < right;
			case Game.GE: return left >= right;
			case Game.LE: return left <= right;
			default: throw new Exception("Unknown comparison operator");
		}
	}
	
	public override object VisitPredicate(Game.PredicateContext context)
	{
	    string direction = context.direction().GetText();

	    if (context.ITEM_TO() != null)
	    {
	        return player.HasItemInDirection(direction);
	    }

	    if (context.OBSTACLE_TO() != null)
	    {
	        return player.HasObstacleInDirection(direction);
	    }

	    throw new Exception("Unknown predicate");
	}
	
	public override object VisitMulDivExpr(Game.MulDivExprContext context)
	{
		int left = (int)Visit(context.expr(0));
		int right = (int)Visit(context.expr(1));

		return context.op.Text == "*" ? left * right : left / right;
	}
	
	public override object VisitAddSubExpr(Game.AddSubExprContext context)
	{
		int left = (int)Visit(context.expr(0));
		int right = (int)Visit(context.expr(1));

		return context.op.Text == "+" ? left + right : left - right;
	}
	
	public override object VisitIntLiteralExpr(Game.IntLiteralExprContext context)
	{
		return int.Parse(context.INT().GetText());
	}
	
	public override object VisitParenExpr(Game.ParenExprContext context)
	{
		return Visit(context.expr());
	}
	
	public override object VisitVarExpr(Game.VarExprContext context)
	{
		string name = context.ID().GetText();

		int value = GetVariable(name);

		GD.Print($"Using variable {name} = {value}");

		return value;
	}
	
	private void SetVariable(string name, int value)
	{
		scopes.Peek()[name] = value;
	}

	private int GetVariable(string name)
	{
		foreach (var scope in scopes)
		{
			if (scope.ContainsKey(name))
				return scope[name];
		}

		throw new Exception($"Undefined variable: {name}");
	}
}
