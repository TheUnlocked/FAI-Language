grammar FAILang;

/*
 * Parser Rules
 */

compileUnit
	: calls EOF
	;

calls
	: (call end)* call?
	;

call
	: imp
	| def
	| expression
	;

imp
	: IMPORT target=STRING
	;

def
	: update=UPDATE? memoize=MEMO? name L_PAREN fparams R_PAREN EQ expression
	| update=UPDATE? name ( EQ | ASSIGN ) expression
	| update=UPDATE memoize=MEMO name
	;

fparams
	: ((param COMMA)* param elipsis=ELIPSIS?)?
	;

callparams
	: ((arg COMMA)* arg)?
	;

arg
	: expression elipsis=ELIPSIS?
	;

name
	: NAME
	;

param
	: NAME
	;

expression
	: boolean
	;

boolean
	: relational ( op=( AND | OR ) relational)?
	;

relational
	: binary ( relational_op binary )*
	;

binary
	: prefix
	| <assoc=right> binary op=EXPONENT binary
	| binary op=( MULTIPLY | DIVIDE ) binary
	| binary op=( PLUS | SUBTRACT | PLUS_MINUS | CONCAT ) binary
	| binary op=IS binary
	;

prefix
	: op=( NOT | SUBTRACT | PLUS_MINUS )? postfix
	;

postfix
	: multiplier ( indexer )?
	;

multiplier
	: t_number=NUMBER atom?
	| atom
	;

atom
	: L_PAREN expression R_PAREN
	| PIPE expression PIPE
	| name
	| atom L_PAREN callparams R_PAREN
	| union
	| lambda
	| piecewise
	| tuple
	| vector
	| t_string=STRING
	| t_boolean=BOOLEAN
	| t_undefined=UNDEFINED
	;

lambda
	: L_PAREN memoize=MEMO? fparams R_PAREN ARROW expression
	| param elipsis=ELIPSIS? ARROW expression
	;

tuple
	: L_PAREN (expression COMMA)* expression COMMA? R_PAREN
	| L_PAREN COMMA R_PAREN
	;

vector
	: L_ARR (expression COMMA)* expression R_ARR
	;

indexer
	: L_BRAC
		( l_index=expression (elipsis=ELIPSIS r_index=expression?)?
		| (l_index=expression? elipsis=ELIPSIS)? r_index=expression
		) R_BRAC
	;

piecewise
	: L_CURL condition+ (expression OTHERWISE SEMI_COLON)?
	;

condition
	: expr=expression IF cond=expression SEMI_COLON
	;

union
	: L_PAREN (expression PIPE)+ expression R_PAREN
	;

relational_op
	: EQ
	| NE
	| R_ARR
	| L_ARR
	| GE
	| LE
	;

end
	: DOT
	;


/*
 * Lexer Rules
 */

fragment UPPERCASE
	: [A-Z]
	;
fragment LOWERCASE
	: [a-z]
	;
fragment DIGIT
	: [0-9]
	;
fragment STRING_CHAR
	: ~'\\'
	;
fragment ESC
	: '\\' 
		( 'b' 
		| 'f' 
		| 'n' 
		| 'r' 
		| 't' 
		| 'v' 
		| '\\' 
		| '"'
		)
	;
fragment E
	: 'e'
	| 'E'
	;

PLUS
	: '+'
	;
SUBTRACT
	: '-'
	;
PLUS_MINUS
	: '+-'
	;
MULTIPLY
	: '*'
	;
DIVIDE
	: '/'
	;
EXPONENT
	: '^'
	;
CONCAT
	: '||'
	;
EQ
	: '='
	;
NE
	: '~='
	;
R_ARR
	: '>'
	;
L_ARR
	: '<'
	;
GE
	: '>='
	;
LE
	: '<='
	;

AND
	: 'and'
	;
OR	
	: 'or'
	;

NOT
	: '~'
	;

L_PAREN
	: '('
	;
R_PAREN
	: ')'
	;
L_CURL
	: '{'
	;
R_CURL
	: '}'
	;
L_BRAC
	: '['
	;
R_BRAC
	: ']'
	;
COMMA
	: ','
	;
DOT
	: '.'
	;
SEMI_COLON
	: ';'
	;
PIPE
	: '|'
	;
ELIPSIS
	: '...'
	| '..'
	;
ARROW
	: '->'
	;
ASSIGN
	: ':='
	;

NUMBER
	: DIGIT* '.' DIGIT+ (E '-'? DIGIT+)?
	| DIGIT+ ('.' DIGIT+)? (E '-'? DIGIT+)?
	;
STRING
	: '"'
		( ESC
		| STRING_CHAR
		)*? '"'
	;
BOOLEAN
	: 'true'
	| 'false'
	;
UNDEFINED
	: 'undefined'
	;

LAMBDA
	: 'lambda'
	;
UPDATE
	: 'update'
	;
MEMO
	: 'memo'
	;

IF
	: 'if'
	;
OTHERWISE
	: 'otherwise'
	;

IS
	: 'is'
	;

IMPORT
	: 'import'
	;

NAME
	:
		( UPPERCASE
		| LOWERCASE
		| '_'
		)
		( UPPERCASE
		| LOWERCASE
		| '_'
		| DIGIT
		)*
	;

COMMENT
	: ('//' .*? EOF) -> channel(HIDDEN)
	;

MULTILINE_COMMENT
	: ('/*' .*? '*/') -> channel(HIDDEN)
	;

WS
	:
		( ' '
		| '\t'
		| '\r'
		| '\n'
		) -> channel(HIDDEN)
	;