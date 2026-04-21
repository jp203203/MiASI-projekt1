using Godot;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;

public class GameVisitor : GameBaseVisitor<object>
{
	private Dictionary<string, int> variables = new Dictionary<string, int>();
	
	public override object VisitMoveCommand(Game.MoveCommandContext context)
	{
		GD.Print("TEMP: move");
		return 0;
	}
	
	public override object VisitRotateCommand(Game.RotateCommandContext context)
	{
		GD.Print("TEMP: rotate");
		return 0;
	}
	
	public override object VisitTakeCommand(Game.TakeCommandContext context)
	{
		GD.Print("TEMP: take");
		return 0;
	}
	
	public override object VisitDropCommand(Game.DropCommandContext context)
	{
		GD.Print("TEMP: drop");
		return 0;
	}
	
	public override object VisitShieldCommand(Game.ShieldCommandContext context)
	{
		GD.Print("TEMP: shield");
		return 0;
	}
	
	public override object VisitIfStatement(Game.IfStatementContext context)
	{
		bool conditionResult = (bool)Visit(context.condition());
		
		if (conditionResult)
		{
			Visit(context.block(0));
		}
		else if (context.ELSE() != null)
		{
			Visit(context.block(1));
		}
		
		return null;
	}
	
	public override object VisitWhileStatement(Game.WhileStatementContext context)
	{
		while ((bool)Visit(context.condition()))
		{
			Visit(context.block());
		}

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

		variables[name] = value;

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
		string text = context.GetText();

		// temp
		GD.Print("Evaluating predicate: " + text);

		// temp result
		return false;
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

		if (!variables.ContainsKey(name))
		{
			throw new Exception($"Undefined variable: {name}");
		}

		int value = variables[name];

		GD.Print($"Using variable {name} = {value}");

		return value;
	}
}
