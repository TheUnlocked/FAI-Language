grammar FAILang;

/*
 * Parser Rules
 */

calls
	: call* end
	;

call
	: def
	| expression
	;

def
	: update=UPDATE? memoize=MEMO? fname L_PAREN fparams R_PAREN EQ expression
	| update=UPDATE? vname EQ expression
	| update=UPDATE memoize=MEMO fname
	;

lambda
	: memoize=MEMO? LAMBDA L_PAREN fparams COLON expression R_PAREN
	;

fparams
	: ((arg COMMA)* arg)?
	;

callparams
	: ((expression COMMA)* expression)?
	;

fname
	: NAME
	;

vname
	: NAME
	;

arg
	: NAME
	;

expression
	: L_PAREN expression R_PAREN
	| union
	| expression L_PAREN callparams R_PAREN
	| prefix expression
	| expression indexer

	// Operator expressions
	| <assoc=right> expression op=EXPONENT expression
	| expression op=( MULTIPLY
					| DIVIDE
					| MODULO ) expression
	| expression op=( PLUS
					| SUBTRACT ) expression
	| expression op=( EQ
					| NE
					| R_ARR
					| L_ARR
					| GE
					| LE ) expression

	| lambda
	| cond
	| type
	| arg
	;

type
	: t_number=NUMBER
	| t_string=STRING
	| t_boolean=BOOLEAN
	| t_void=VOID
	| vector
	;

vector
	: L_ARR (expression COMMA)* expression R_ARR
	;

prefix
	: NOT
	| SUBTRACT
	;

indexer
	: L_BRAC
		( l_index=expression (elipsis=ELIPSIS r_index=expression?)?
		| (l_index=expression? elipsis=ELIPSIS)? r_index=expression
		) R_BRAC
	;

cond
	: L_CURL condition+ expression R_CURL
	;

condition
	: expression COLON expression SEMI_COLON
	;

union
	: L_PAREN (expression VERT_LINE)+ expression R_PAREN
	;

end
	: SEMI_COLON
	| EOF
	;

compileUnit
	: EOF
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
MULTIPLY
	: '*'
	;
DIVIDE
	: '/'
	;
MODULO
	: '%'
	;
EXPONENT
	: '^'
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
COLON
	: ':'
	;
SEMI_COLON
	: ';'
	;
VERT_LINE
	: '|'
	;
ELIPSIS
	: '...'
	| '..'
	;

NUMBER
	: DIGIT* '.' DIGIT+ (E '-'? DIGIT+)? 'i'?
	| DIGIT+ ('.' DIGIT+)? (E '-'? DIGIT+)? 'i'?
	| 'i'
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
VOID
	: 'void'
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
	: ('//' .*?) -> channel(HIDDEN)
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