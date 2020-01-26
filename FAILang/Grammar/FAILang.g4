grammar FAILang;

/*
 * Parser Rules
 */

compileUnit
	: earlyCalls calls EOF
	;

expressionCompileUnit
	: importStatement
	| usingStatement
	| defStatement
	| expression
	;

earlyCalls
	: importStatement* usingStatement* namespaceStatement?
	;

calls
	: (call end)* call?
	;

call
	: defStatement
	| expression
	;

importStatement
	: 'import' target=STRING
	;

usingStatement
	: 'using' namespace
	| 'using' namespace 'with' pre='prefix' name
	;

namespaceStatement
	: 'namespace' namespace
	;

defStatement
	: update='update'? def
	| update='update' memoize=MEMO name
	;

def
	: memoize='memo'? name L_PAREN fparams R_PAREN EQ expression
	| name EQ expression
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

namespace
	: (NAME DIVIDE)* NAME
	;

name
	: NAME
	;

param
	: NAME
	;

expression
	: where
	;

where
	: boolean (WHERE (def COMMA)* def)?
	;

boolean
	: relational ( op=( AND | OR ) relational)?
	;

relational
	: binary ( relational_op binary )*
	;

binary
	: prefix
	| binary op=( MULTIPLY | DIVIDE ) binary
	| binary op=( PLUS | SUBTRACT | PLUS_MINUS | CONCAT ) binary
	| binary op=IS binary
	;

prefix
	: op=( NOT | SUBTRACT | PLUS_MINUS )? multiplier
	;

multiplier
	: t_number=NUMBER exponent?
	| exponent
	;

exponent
	: atom (EXPONENT prefix)?
	;

atom
	: L_PAREN expression R_PAREN
	| PIPE expression PIPE
	| name
	| atom indexer
	| atom L_PAREN callparams R_PAREN
	| union
	| lambda
	| piecewise
	| tuple
	| vector
	//| map
	| NUMBER
	| STRING
	| BOOLEAN
	| UNDEFINED
	;

lambda
	: L_PAREN memoize='memo'? fparams R_PAREN ARROW expression
	| param elipsis=ELIPSIS? ARROW expression
	;

tuple
	: L_PAREN (expression COMMA)* expression COMMA? R_PAREN
	| L_PAREN COMMA R_PAREN
	;

vector
	: L_ARR (expression COMMA)* expression R_ARR
	;

map
	: L_BRAC (expression ARROW expression COMMA)* expression ARROW expression R_BRAC
	;

indexer
	: L_BRAC
		( l_index=expression (elipsis=ELIPSIS r_index=expression?)?
		| (l_index=expression? elipsis=ELIPSIS)? r_index=expression
		) R_BRAC
	;

piecewise
	: L_CURL condition+ (expression OTHERWISE SEMI_COLON?)?
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
	|
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

IF
	: 'if'
	;
OTHERWISE
	: 'otherwise'
	;

IS
	: 'is'
	;

WHERE
	: 'where'
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
