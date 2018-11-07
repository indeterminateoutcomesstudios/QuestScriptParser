//note : adapted and modified from original to parse TextAdventures Quest script
// the original, unadapted version is a Javascript parser that can be found here : https://github.com/antlr/grammars-v4/tree/master/javascript
// keeping the original copyright notice...
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

 grammar QuestScript;

options
{
	superClass = QuestScriptBaseParser;
}

script : statementList? EOF;

//statement definitions
statementList : statement+;

statement
    : block
    | expressionStatement
    | ifStatement
    | iterationStatement
    | continueStatement
    | breakStatement
    | returnStatement
    | finishStatement
    | firstTimeStatement
    | getInputStatement
    | onReadyStatement
    | pictureStatement
    | playSoundStatement
    | showMenuStatement
    | startTransactionStatement
    | stopSoundStatement
    | undoStatement
    | switchStatement
    | waitStatement
    | askStatement
	| switchCaseStatement	
    ;
 
block : OpenBraceToken statementList? CloseBraceToken;

askQuestionStatement: AskToken OpenParenToken statement CloseParenToken;
askStatement: askQuestionStatement statement;
firstTimeStatement: FirstTimeToken statement (OtherwiseToken statement)?;
getInputStatement: GetInputToken statement;
onReadyStatement: OnReadyToken statement;
pictureStatement: PictureToken arguments;
playSoundStatement: PlaySoundToken arguments;
stopSoundStatement: StopSoundToken;
undoStatement: UndoToken;

switchCaseStatement: 
	switchStatement
	OpenBraceToken 
		caseStatement* 
		defaultStatement? 
	CloseBraceToken;

switchStatement : SwitchToken OpenParenToken statement CloseParenToken;
caseStatement : CaseToken OpenParenToken literal CloseParenToken statement;
defaultStatement : DefaultToken statement;

waitStatement: WaitToken statement;
showMenuStatement: ShowMenuToken arguments statement;
startTransactionStatement: StartTransactionToken OpenParenToken StringLiteralToken CloseParenToken;

ifConditionStatement: OpenParenToken expressionSequence CloseParenToken;
elseIfStatement : ElseIfToken ifConditionStatement statement;
elseStatement : ElseToken statement;
ifStatement : IfToken ifConditionStatement statement elseIfStatement* elseStatement?;

continueStatement : ContinueToken ({this.NotLineTerminator()}? IdentifierToken)?;
breakStatement : BreakToken ({this.NotLineTerminator()}? IdentifierToken)?;
returnStatement : ReturnToken OpenParenToken ({this.NotLineTerminator()}? expressionSequence)? CloseParenToken;
expressionStatement : {this.NotOpenBraceToken()}? expressionSequence;
iterationStatement
    : DoToken statement WhileToken OpenParenToken expressionSequence CloseParenToken                                    # DoStatement
    | WhileToken OpenParenToken expressionSequence CloseParenToken statement                                            # WhileStatement
    | ForEachToken OpenParenToken IdentifierToken ':' IdentifierToken CloseParenToken statement                         # ForEachStatement
    | ForToken OpenParenToken IdentifierToken CommaToken IntegerLiteralToken CommaToken IntegerLiteralToken CloseParenToken statement # ForStatement
    ;
finishStatement: FinishToken;

//statement elements
expressionSequence  : singleExpression (CommaToken singleExpression)*;

arguments
    : OpenParenToken(
          singleExpression (CommaToken singleExpression)*
       )?CloseParenToken
    ;

//base expression that allows recursive traversal
singleExpression :
      literal                                                                # LiteralExpression
    | OpenParenToken expressionSequence CloseParenToken                      # ParenthesizedExpression
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
    | singleExpression '=>' singleExpression                                 # ScriptAssignmentExpression
    | singleExpression assignmentOperator singleExpression                   # CalculationAndAssignmentOperatorExpression
    | singleExpression arguments                                             # FunctionCallExpression
    | singleExpression '[' expressionSequence ']'                            # MemberIndexExpression
    | arrayLiteral                                                           # ArrayLiteralExpression
    | ThisToken                                                              # ThisExpression
    | IdentifierToken                                                        # IdentifierExpression
    | CloneToken IdentifierToken                                             # CloneObjectExpression
    | CreateToken ExitToken arguments                                        # CreateExitExpression
    | CreateToken TimerToken arguments                                       # CreateTimerExpression
    | CreateToken TurnscriptToken arguments                                  # CreateTurnscriptExpression
    ;

assignmentOperator
    : '*='
    | '/='
    | '%='
    | '+='
    | '-='	
    ;

identifierName : IdentifierToken | reservedWord;

literal : numericLiteral | characterLiteral | stringLiteral | booleanLiteral | nullLiteral | arrayLiteral;

arrayLiteral : '[' CommaToken* elementList? CommaToken* ']';
elementList : singleExpression (CommaToken+ singleExpression)*;

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
    | CreateToken
    | ExitToken
    | TimerToken
    | AskToken
    | TurnscriptToken
    | FinishToken
    | FirstTimeToken
    | OtherwiseToken
    | SwitchToken
    | CaseToken
    | DefaultToken
    | UndoToken
    | WaitToken
    | CloneToken
	| ElseIfToken
	| PictureToken
    ;

CloseParenToken:					 ')';
OpenParenToken:						 '(';
CommaToken:							 ',';
OpenBraceToken:						 '{';
CloseBraceToken:					 '}';
CloneToken:                          'clone';
WaitToken:                           'wait';
SwitchToken:                         'switch';
CaseToken:                           'case';
DefaultToken:                        'default';
UndoToken:                           'undo';
StopSoundToken:                      'stop sound';
StartTransactionToken:               'start transaction';
ShowMenuToken:                       'show menu';
PlaySoundToken:                      'play sound';
PictureToken:                        'picture';
OnReadyToken:                        'on ready';
GetInputToken:                       'get input';
OtherwiseToken:                      'otherwise';
FirstTimeToken:                      'firsttime';
FinishToken:                         'finish';
TurnscriptToken:                     'turnscript';
TimerToken:                          'timer';
ExitToken:                           'exit';
AskToken:                            'ask';
BreakToken:                          'break';
DoToken:                             'do';
IfToken:                             'if';
ElseToken:                           'else';
ElseIfToken:                         'elseif';
ReturnToken:                         'return';
ContinueToken:                       'continue';
ForToken:                            'for';
WhileToken:                          'while';
ThisToken:                           'this';
CreateToken:                         'create';
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
