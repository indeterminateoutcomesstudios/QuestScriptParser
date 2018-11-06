/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 by Bart Kiers (original author) and Alexandre Vitorelli (contributor -> ported to CSharp)
 * Copyright (c) 2017 by Ivan Kochurkin (Positive Technologies):
    added ECMAScript 6 support, cleared and transformed to the universal grammar.
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

 //note : adapted and modified from original to parse TextAdventures Quest script
 // the original, unadapted version is a Javascript parser that can be found here : https://github.com/antlr/grammars-v4/tree/master/javascript
grammar QuestScript;

options
{
	superClass = QuestScriptBaseParser;
}

script : statementList? EOF;

//statement definitions

statement
    : block
    | expressionStatement
    | ifStatement
    | ifElseIfStatement
    | iterationStatement
    | continueStatement
    | breakStatement
    | returnStatement
    | throwStatement
    ;

block : OpenBraceToken statementList? CloseBraceToken;

statementList : statement+;

ifStatement : IfToken '(' expressionSequence ')' statement (ElseToken statement)?;
ifElseIfStatement : IfToken '(' expressionSequence ')' statement (ElseToken IfToken statement)* (ElseToken statement)?;
continueStatement : ContinueToken ({this.NotLineTerminator()}? IdentifierToken)? endOfLine;
breakStatement : BreakToken ({this.NotLineTerminator()}? IdentifierToken)? endOfLine;
returnStatement : ReturnToken '(' ({this.NotLineTerminator()}? expressionSequence)? ')' endOfLine;
expressionStatement : {this.NotOpenBrace()}? expressionSequence endOfLine;
throwStatement : ThrowToken {this.NotLineTerminator()}? expressionSequence endOfLine;
iterationStatement
    : DoToken statement WhileToken '(' expressionSequence ')' endOfLine   # DoStatement
    | WhileToken '(' expressionSequence ')' statement                     # WhileStatement
    | ForEachToken '(' IdentifierToken ':' IdentifierToken ')' statement  # ForEachStatement
    ;

//statement elements
expressionSequence  : singleExpression (',' singleExpression)*;

arguments
    : '('(
          singleExpression (',' singleExpression)*
       )?')'
    ;

//base expression that allows recursive traversal
singleExpression :
      literal                                                                # LiteralExpression
    | '(' expressionSequence ')'                                             # ParenthesizedExpression
    | singleExpression '.' identifierName                                    # MemberDotExpression
    | '++' singleExpression                                                  # PreIncrementExpression
    | '--' singleExpression                                                  # PreDecreaseExpression
    | singleExpression '++'                                                  # PostIncrementExpression
    | singleExpression '--'                                                  # PostDecreaseExpression
    | '+' singleExpression                                                   # UnaryPlusExpression
    | '-' singleExpression                                                   # UnaryMinusExpression
    | 'not' singleExpression                                                 # NotExpression
    | singleExpression ('*' | '/' | '%') singleExpression                    # MultiplicativeExpression
    | singleExpression ('+' | '-') singleExpression                          # AdditiveExpression
    | singleExpression ('<' | '>' | '<=' | '>=') singleExpression            # RelationalExpression
    | singleExpression '!='  singleExpression                                # InequalityExpression
    | singleExpression 'and' singleExpression                                # LogicalAndExpression
    | singleExpression 'or' singleExpression                                 # LogicalOrExpression
    | singleExpression '?' singleExpression ':' singleExpression             # TernaryExpression
    | singleExpression '=' singleExpression                                  # AssignmentOrEqualityExpression
    | singleExpression assignmentOperator singleExpression                   # AssignmentOperatorExpression
    | singleExpression arguments                                             # FunctionCallExpression
    | singleExpression '[' expressionSequence ']'                            # MemberIndexExpression
    | arrayLiteral                                                           # ArrayLiteralExpression
    | ThisToken                                                              # ThisExpression
    | IdentifierToken                                                        # IdentifierExpression
    | 'clone' IdentifierToken                                                # CloneObjectExpression
    ;

assignmentOperator
    : '*='
    | '/='
    | '%='
    | '+='
    | '-='
    ;

identifierName : IdentifierToken | reservedWord;

literal : numericLiteral | characterLiteral | stringLiteral | booleanLiteral | nullLiteral;

arrayLiteral : '[' ','* elementList? ','* ']';
elementList : singleExpression (','+ singleExpression)*;

numericLiteral : integerLiteral | doubleLiteral;
integerLiteral : IntegerLiteralToken;
doubleLiteral : DoubleLiteralToken;
characterLiteral : CharacterLiteralToken;
stringLiteral : StringLiteralToken;
booleanLiteral : BooleanLiteralToken;
nullLiteral : NullLiteralToken;

reservedWord : keyword | nullLiteral | booleanLiteral;

keyword :
      BreakToken
    | DoToken
    | IfToken
    | ElseToken
    | ReturnToken
    | ContinueToken
    | ForEachToken
    | ForToken
    | WhileToken
    | ThisToken
    | ThrowToken
    ;

endOfLine : EOF | {this.LineTerminatorAhead()}? | {this.CloseBrace()}?;

OpenBraceToken:						 '{';
CloseBraceToken:					 '}';

BreakToken:                          'break';
DoToken:                             'do';
IfToken:                             'if';
ElseToken:                           'else';
ReturnToken:                         'return';
ContinueToken:                       'continue';
ForToken:                            'for';
WhileToken:                          'while';
ThisToken:                           'this';
ThrowToken:                          'throw';
ForEachToken:                        'foreach';

NullLiteralToken : 'null';
BooleanLiteralToken : 'true' | 'false';

IdentifierToken : IdentifierStart IdentifierPart*;

StringLiteralToken : '"' (AnyCharacterExceptSpecial | EscapeSequence)* '"';
CharacterLiteralToken : '\'' AnyCharacterExceptSpecial '\'';
DoubleLiteralToken : Digit+ '.' Digit+;
IntegerLiteralToken : Digit+;

fragment IdentifierPart : Letter | Digit | '_';
fragment IdentifierStart : Letter | '_';
fragment AnyCharacterExceptSpecial : ~["\\\r\n\u0085\u2028\u2029];
fragment Letter : [A-Za-z];
fragment NonzeroDigit : [1-9];
fragment Digit : [0-9];
fragment EscapeSequence : '\\' ['"?abfnrtv\\];


Newline :   ('\r\n'|'\n'|'\r') -> skip;
// end of string related definitions

//stuff to ignore
Whitespace :   [ \t]+ -> skip;
LineTerminator: [\r\n\u2028\u2029];
BlockComment : '/*' .*? '*/' -> skip;
LineComment : '//' ~[\r\n]* -> skip;