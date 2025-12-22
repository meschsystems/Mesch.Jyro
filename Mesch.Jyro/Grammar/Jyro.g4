grammar Jyro;

// -----------------------
// Parser rules
// -----------------------

program
    : statement* EOF
    ;

// ---- Statements ----
statement
    : variableDecl
    | ifStmt
    | switchStmt
    | whileStmt
    | forEachStmt
    | returnStmt
    | failStmt
    | breakStmt
    | continueStmt
    | incDecStmt
    | exprStmt
    ;

variableDecl
    : VAR Identifier (COLON typeName)? (ASSIGN expression)?
    ;

typeName
    : NUMBER_T
    | STRING_T
    | BOOLEAN_T
    | OBJECT_T
    | ARRAY_T
    ;

exprStmt
    : expression
    ;

incDecStmt
    : assignmentTarget (INCR | DECR)
    ;

// ---- Control Flow ----
ifStmt
    : IF expression THEN statement*
      (ELSE IF expression THEN statement*)*
      (ELSE statement*)?
      END
    ;

switchStmt
    : SWITCH expression DO
        (CASE expression THEN statement*)*
        (DEFAULT THEN statement*)?
      END
    ;

whileStmt
    : WHILE expression DO statement* END
    ;

forEachStmt
    : FOREACH Identifier IN expression DO statement* END
    ;

returnStmt
    : RETURN expression?
    ;

failStmt
    : FAIL expression?
    ;

breakStmt
    : BREAK
    ;

continueStmt
    : CONTINUE
    ;

// ---- Expressions (C#-style precedence) ----
// Lowest to highest:
// assignment (right-assoc) < conditional ?: (right-assoc) < or < and < equality < relational < additive < multiplicative < unary < postfix/primary

expression
    : assignmentExpr
    ;

assignmentExpr
    : conditionalExpr                                     #AssignPass
    | assignmentTarget ASSIGN assignmentExpr              #AssignMake
    ;

// Assignment targets: identifiers or Data.* chains
assignmentTarget
    : Identifier (memberOrIndex)*
    | DATA (memberOrIndex)+
    ;

conditionalExpr
    : logicalOrExpr (QMARK conditionalExpr COLON conditionalExpr)?
    ;

logicalOrExpr
    : logicalAndExpr (OR logicalAndExpr)*
    ;

logicalAndExpr
    : equalityExpr (AND equalityExpr)*
    ;

equalityExpr
    : relationalExpr ((EQ | NEQ) relationalExpr)*
    ;

relationalExpr
    : additiveExpr ((LT | LE | GT | GE | ISNOT | IS) additiveExpr)*
    ;

additiveExpr
    : multiplicativeExpr ((ADD | SUB) multiplicativeExpr)*
    ;

multiplicativeExpr
    : unaryExpr ((MUL | DIV | MOD) unaryExpr)*
    ;

unaryExpr
    : (NOT | SUB) unaryExpr
    | postfixExpr
    ;

postfixExpr
    : primaryExpr (postfixSuffix)*
    ;

postfixSuffix
    : LPAREN argList? RPAREN
    | memberOrIndex
    ;

memberOrIndex
    : DOT propertyName
    | LBRACK expression RBRACK
    ;

// Property names can be identifiers or keywords
propertyName
    : Identifier
    | typeKeyword
    | TRUE
    | FALSE
    | NULL
    | VAR
    | IF
    | THEN
    | ELSE
    | END
    | SWITCH
    | DO
    | CASE
    | DEFAULT
    | WHILE
    | FOREACH
    | IN
    | RETURN
    | FAIL
    | BREAK
    | CONTINUE
    | AND
    | OR
    | NOT
    | IS
    | DATA
    ;

// ---- Primaries & literals ----
primaryExpr
    : literal
    | Identifier
    | typeKeyword
    | DATA (memberOrIndex)*
    | LPAREN expression RPAREN
    | objectLiteral
    | arrayLiteral
    ;

typeKeyword
    : NUMBER_T
    | STRING_T
    | BOOLEAN_T
    | OBJECT_T
    | ARRAY_T
    ;

literal
    : numberLiteral
    | stringLiteral
    | TRUE
    | FALSE
    | NULL
    ;

numberLiteral
    : Number
    ;

stringLiteral
    : String
    ;

objectLiteral
    : LBRACE (objectEntry (COMMA objectEntry)*)? RBRACE
    ;

objectEntry
    : (String | LBRACK expression RBRACK) COLON expression
    ;

arrayLiteral
    : LBRACK (expression (COMMA expression)*)? RBRACK
    ;

argList
    : expression (COMMA expression)*
    ;

// -----------------------
// Lexer rules
// -----------------------

// Keywords / operators (case-sensitive)
VAR        : 'var';
IF         : 'if';
THEN       : 'then';
ELSE       : 'else';
END        : 'end';
SWITCH     : 'switch';
DO         : 'do';
CASE       : 'case';
DEFAULT    : 'default';
WHILE      : 'while';
FOREACH    : 'foreach';
IN         : 'in';
RETURN     : 'return';
FAIL       : 'fail';
BREAK      : 'break';
CONTINUE   : 'continue';

TRUE       : 'true';
FALSE      : 'false';
NULL       : 'null';

AND        : 'and';
OR         : 'or';
NOT        : 'not';
ISNOT      : 'is not';
IS         : 'is';

// Types
NUMBER_T   : 'number';
STRING_T   : 'string';
BOOLEAN_T  : 'boolean';
OBJECT_T   : 'object';
ARRAY_T    : 'array';

// Reserved root
DATA       : 'Data';

// Punctuation / operators
INCR       : '++';
DECR       : '--';
EQ         : '==';
NEQ        : '!=';
LE         : '<=';
GE         : '>=';
LT         : '<';
GT         : '>';
ADD        : '+';
SUB        : '-';
MUL        : '*';
DIV        : '/';
MOD        : '%';

ASSIGN     : '=';
QMARK      : '?';
COLON      : ':';
COMMA      : ',';
DOT        : '.';
LPAREN     : '(';
RPAREN     : ')';
LBRACE     : '{';
RBRACE     : '}';
LBRACK     : '[';
RBRACK     : ']';

// Identifiers
Identifier : (Letter | '_') (Letter | Digit | '_')* ;

// Numbers: integers and decimals
Number     : Digit+ ('.' Digit+)? ;

// Strings: double-quoted with escapes
String
    : '"' ( EscapeSeq | ~["\\\r\n] )* '"'
    ;

fragment EscapeSeq
    : '\\' [btnfr"\\/]          // \b \t \n \f \r \" \\ \/
    | '\\' 'u' Hex Hex Hex Hex  // \uXXXX
    ;

fragment Letter : [A-Za-z] ;
fragment Digit  : [0-9] ;
fragment Hex    : [0-9A-Fa-f] ;

// Whitespace & comments
WS       : [ \t\r\n]+ -> skip ;
COMMENT  : '#' ~[\r\n]* -> skip ;