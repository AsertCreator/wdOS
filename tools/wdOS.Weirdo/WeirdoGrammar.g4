/*
	written by asertcreator, 2023
*/

grammar WeirdoGrammar;

program
    : (function|entrypoint_directive|global_directive)* EOF
    ;
    
expression: expression (ADD|SUB|MUL|DIV) expression
	| expression (AND|OR|XOR|DEQ|NEQ) expression
	| NOT expression
	| function_call_expression
	| string_literal
	| OBJECT
    | INT
	| TRUE
	| FALSE
	| argument_expression
	| ID
    ;

function_name
	: ID
	;

function_arg
	: ID
	;

function_call_expression
    : CALL LPAREN function_name (COMMA expression)* RPAREN
	;
	
function_icall_expression
    : ICALL LPAREN INT RPAREN
	;
	
push_expression
    : PUSH LPAREN expression RPAREN
	;
	
popl_expression
    : POPL LPAREN ID RPAREN
	;

argument_expression
	: ARGUMENT LPAREN ID RPAREN
	;
	
string_literal
	: STRING_LITERAL
	;

global_directive
	: GLOBAL ID SEMI
	;
	
entrypoint_directive
	: ENTRYPOINT (LPAREN)? ID (RPAREN)? SEMI
	;
    
statement
    : LOCAL ID (EQ expression)?
	| if_statement
	| print_statement
	| function_call_expression
	| function_icall_expression
	| ID EQ expression
	| ID LBRACK expression RBRACK EQ expression
	| RETURN expression
	| push_expression
	| popl_expression
    ;
	
print_statement
	: PRINT LPAREN expression RPAREN
	;
	
if_statement
	: IF (LPAREN)? expression (RPAREN)? LCURLY (statement SEMI)* RCURLY
	;
    
arg_list
    : (function_arg)? (COMMA function_arg)*
    ;
    
function
    : FUNCTION function_name LPAREN arg_list RPAREN LCURLY (statement (SEMI)?)* RCURLY
    ;
    
AND : '&' ;
OR : '|' ;
XOR : '^' ;
NOT : '!' ;
EQ : '=' ;
DEQ : '==' ;
NEQ : '!=' ;
DOT : '.' ;
COMMA : ',' ;
SEMI : ';' ;
LPAREN : '(' ;
RPAREN : ')' ;
LCURLY : '{' ;
RCURLY : '}' ;
LBRACK : '[' ;
RBRACK : ']' ;
ADD : '+' ;
SUB : '-' ;
MUL : '*' ;
DIV : '/' ;
FUNCTION : 'function' ;
ENTRYPOINT : 'entrypoint' ;
LOCAL : 'local' ;
RETURN : 'return' ;
OBJECT : 'object' ;
PRINT : 'print' ;
GLOBAL : 'global' ;
TRUE : 'true' ;
FALSE : 'false' ;
NULL : 'null' ;
UNDEFINED : 'undefined' ;
ARGUMENT : 'argument' ;
DBQOUTE : '"' ;
QUOTE : '\'' ;
CALL : 'call' ;
ICALL : '__internal_call' ;
PUSH : '__push' ;
POPL : '__pop_local' ;
IF : 'if' ;

INT : ('-')?[0-9]+ ;
ID: [a-zA-Z_][a-zA-Z_0-9]* ;
WS: [ \t\n\r\f]+ -> skip ;
COMMENT: '/*' .*? '*/' -> skip ;
LINE_COMMENT: '//' ~[\r\n]* -> skip ;
STRING_LITERAL : '"' ('""' | ~'"' )* '"' ;