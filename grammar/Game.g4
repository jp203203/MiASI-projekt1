parser grammar Game;

options {
    tokenVocab = GameLexer;
}

// -------------------- PARSER RULES --------------------

program
    : statement* EOF
    ;

statement
    : simpleCommand
    | assignment
    | ifStatement
    | whileStatement
    | repeatStatement
    | procedureDecl
    | procedureCall
    | NEWLINE
    ;

simpleCommand
    : moveCommand
    | rotateCommand
    | takeCommand
    | dropCommand
    | shieldCommand
    ;

moveCommand
    : MOVE expr NEWLINE?
    ;

rotateCommand
    : ROTATE direction NEWLINE?
    ;

takeCommand
    : TAKE NEWLINE?
    ;

dropCommand
    : DROP NEWLINE?
    ;

shieldCommand
    : SHIELD NEWLINE?
    ;

// IF / ELSE

ifStatement
    : IF condition ':' NEWLINE? block (ELSE ':' NEWLINE? block)? ENDIF NEWLINE?
    ;

// WHILE

whileStatement
    : WHILE condition ':' NEWLINE? block ENDWHILE NEWLINE?
    ;

// REPEAT

repeatStatement
    : REPEAT expr ':' NEWLINE? block ENDREPEAT NEWLINE?
    ;

// PROCEDURES

procedureDecl
    : PROCEDURE ID '(' paramList? ')' ':' NEWLINE? block ENDPROC NEWLINE?
    ;

paramList
    : ID (',' ID)*
    ;

procedureCall
    : ID '(' argList? ')' NEWLINE?
    ;

argList
    : expr (',' expr)*
    ;

// BLOCK

block
    : statement+
    ;

// ASSIGNMENT

assignment
    : ID '=' expr NEWLINE?
    ;

// CONDITIONS

condition
    : comparison
    | predicate
    ;

comparison
    : expr compOp expr
    ;

compOp
    : EQ | NEQ | GT | LT | GE | LE
    ;

// PREDICATES

predicate
    : ITEM_TO direction
    | OBSTACLE_TO direction
    ;

direction
    : LEFT
    | RIGHT
    | UP
    | DOWN
    ;

// EXPRESSIONS

expr
    : expr op=('*'|'/') expr      # MulDivExpr
    | expr op=('+'|'-') expr      # AddSubExpr
    | INT                         # IntLiteralExpr
	| ID '(' argList? ')'         # ProcCallExpr
    | ID                          # VarExpr
    | '(' expr ')'                # ParenExpr
    ;
