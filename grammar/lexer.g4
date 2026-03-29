lexer grammar gierkalang ;

// -------------------- KEYWORDS --------------------

MOVE        : 'MOVE';
ROTATE      : 'ROTATE';
TAKE        : 'TAKE';
DROP        : 'DROP';
SHIELD      : 'SHIELD';

IF          : 'IF';
ELSE        : 'ELSE';
WHILE       : 'WHILE';
REPEAT      : 'REPEAT';

PROCEDURE   : 'PROCEDURE';

ITEM_TO     : 'ITEM_TO';
OBSTACLE_TO : 'OBSTACLE_TO';

LEFT        : 'LEFT';
RIGHT       : 'RIGHT';
UP          : 'UP';
DOWN        : 'DOWN';

ENDIF       : 'ENDIF';
ENDWHILE    : 'ENDWHILE';
ENDREPEAT   : 'ENDREPEAT';
ENDPROC     : 'ENDPROC';

// -------------------- OPERATORS --------------------

EQ          : '==';
NEQ         : '!=';
GT          : '>';
LT          : '<';
GE          : '>=';
LE          : '<=';

// -------------------- IDENTIFIERS & LITERALS --------------------

ID          : [a-zA-Z_][a-zA-Z_0-9]*;
INT         : [0-9]+;

// -------------------- COMMENTS & WHITESPACE --------------------

COMMENT
    : '#' ~[\r\n]* -> skip
    ;

WS
    : [ \t\r]+ -> skip
    ;

NEWLINE
    : '\r'? '\n'
    ;
