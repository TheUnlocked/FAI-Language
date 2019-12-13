grammar FAILang;

/*
 * Parser Rules
 */

compileUnit
	: br earlyCalls calls br EOF
	;

expressionCompileUnit
	: br importStatement
	| br usingStatement
	| br defStatement
	| br expression
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
	: 'import' target=STRING br
	;

usingStatement
	: 'using' namespace ('with' pre='prefix' name)? br
	;

namespaceStatement
	: 'namespace' namespace br
	;

defStatement
	: update='update'? def br
	| update='update' memoize=MEMO name br
	;

def
	: memoize=MEMO? br name br L_PAREN br fparams br R_PAREN br EQ br expression
	| name br EQ br expression
	;

fparams
	: ((param br COMMA br)* param elipsis=ELIPSIS?)?
	;

callparams
	: ((arg br COMMA br)* arg)?
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
	: NAME br
	;

expression
	: where
	;

where
	: boolean (br WHERE br (def br COMMA br)* def)?
	;

boolean
	: relational (br op=( AND | OR ) br relational)?
	;

relational
	: binary (br relational_op br binary )*
	;

binary
	: prefix
	| binary br op=( MULTIPLY | DIVIDE ) br binary
	| binary br op=( PLUS | SUBTRACT | PLUS_MINUS | CONCAT ) br binary
	| binary br op=IS br binary
	;

prefix
	: (op=( NOT | SUBTRACT | PLUS_MINUS ) br)? multiplier
	;

multiplier
	: NUMBER exponent?
	| exponent
	;

exponent
	: (atom | NUMBER) (EXPONENT br prefix)?
	;

atom
	: L_PAREN br expression br R_PAREN
	| PIPE expression PIPE
	| name
	| atom br indexer
	| atom L_PAREN br callparams br R_PAREN
	| union
	| lambda
	| piecewise
	| tuple
	| vector
	| STRING
	| BOOLEAN
	| UNDEFINED
	;

lambda
	: L_PAREN memoize=MEMO? br fparams br R_PAREN br ARROW br expression
	| param elipsis=ELIPSIS? br ARROW br expression
	;

tuple
	: L_PAREN br (expression br COMMA br)* expression br COMMA? br R_PAREN
	| L_PAREN br COMMA br R_PAREN
	;

vector
	: L_ARR br (expression br COMMA br)* expression br R_ARR
	;

indexer
	: L_BRAC br
		( l_index=expression br (elipsis=ELIPSIS br r_index=expression?)?
		| (l_index=expression? br elipsis=ELIPSIS)? br r_index=expression
		) br R_BRAC
	;

piecewise
	: L_CURL br (condition br)+ (expression br OTHERWISE (br SEMI_COLON)?)?
	;

condition
	: expr=expression br IF br cond=expression br SEMI_COLON
	;

union
	: L_PAREN br (expression br PIPE br)+ expression br R_PAREN
	;

relational_op
	: EQ
	| NE
	| R_ARR
	| L_ARR
	| GE
	| LE
	;

end: DOT? br;

br: BR*;


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

BR: '\n';

WS: [ \t\r] -> channel(HIDDEN);
