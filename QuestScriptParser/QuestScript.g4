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

statementList : statements += statement+;

//statement definitions
statement
    : blockStatement
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
	| listMutationStatement
	| dictionaryMutationStatement
	| setTimeoutStatement
	| setTimeoutIdStatement
	| setTimerScriptStatement
	| setTurnScriptStatement
	| setTurnTimeoutStatement
	| setTurnTimeoutIdStatement
    ;
 
blockStatement : OpenBraceToken blockStatements = statementList? CloseBraceToken;
codeBlock : statement;
askQuestionStatement: AskToken OpenParenToken question = singleExpression CloseParenToken;
askStatement: askQuestionStatement code = codeBlock;
firstTimeStatement: FirstTimeToken firstTimeCode = statement (OtherwiseToken otherwiseCode = statement)?;
getInputStatement: GetInputToken code = codeBlock;
onReadyStatement: OnReadyToken code = codeBlock;
pictureStatement: PictureToken OpenParenToken filename = singleExpression CloseParenToken;
playSoundStatement: PlaySoundToken OpenParenToken file = singleExpression CommaToken wait = singleExpression CommaToken loop = singleExpression CloseParenToken; //(string file, boolean wait, boolean loop)
stopSoundStatement: StopSoundToken;
undoStatement: UndoToken;

switchCaseStatement: 
	switch = switchStatement
	OpenBraceToken 
		cases += caseStatement* 
		defaultContext = defaultStatement? 
	CloseBraceToken;

switchStatement : SwitchToken OpenParenToken switchConditionStatement = statement CloseParenToken;
caseStatement : CaseToken OpenParenToken caseValue = literal CloseParenToken code = codeBlock;
defaultStatement : DefaultToken code = codeBlock;

waitStatement: WaitToken code = statement;
showMenuStatement: ShowMenuToken menuArguments = arguments code = codeBlock;
startTransactionStatement: StartTransactionToken OpenParenToken command = StringLiteralToken CloseParenToken;

ifConditionStatement: OpenParenToken condition = expressionSequence CloseParenToken;
ifStatement : IfToken conditionStatement = ifConditionStatement code = codeBlock elseIfStatement* elseStatement?;
elseIfStatement : ElseIfToken conditionStatement = ifConditionStatement code = codeBlock;
elseStatement : ElseToken code = codeBlock;

continueStatement : ContinueToken;
breakStatement : BreakToken;
  
returnStatement : ReturnToken OpenParenToken (returnValue = singleExpression)? CloseParenToken;

expressionStatement : {this.NotOpenBraceToken()}? expressionSequence;
iterationStatement
    : DoToken code = codeBlock WhileToken OpenParenToken condition = expressionSequence CloseParenToken   # DoStatement
    | WhileToken OpenParenToken condition = expressionSequence CloseParenToken code = codeBlock   # WhileStatement
    | ForEachToken OpenParenToken iterationVariable = IdentifierToken ':' 
								  enumerationVariable = IdentifierToken 
				   CloseParenToken code = codeBlock   # ForEachStatement
    | ForToken OpenParenToken iterationVariable = IdentifierToken CommaToken 
							  iterationStart = IntegerLiteralToken CommaToken 
							  iterationEnd = IntegerLiteralToken 
			   CloseParenToken code = codeBlock   # ForStatement
    ;

finishStatement: FinishToken;
 
listMutationStatement: ListToken op = (AddToken | RemoveToken) args = arguments;
dictionaryMutationStatement: DictionaryToken op = (AddToken | RemoveToken) args = arguments;

setTimeoutStatement : SetTimeoutToken OpenParenToken interval = singleExpression CloseParenToken code = blockStatement;
setTimeoutIdStatement : SetTimeoutIDToken OpenParenToken interval = singleExpression CommaToken name = singleExpression CloseParenToken  code = blockStatement;
setTimerScriptStatement: SetTimerScriptToken OpenParenToken timerScript = singleExpression CloseParenToken code = blockStatement;
setTurnScriptStatement: SetTurnScriptToken OpenParenToken turnScript = singleExpression CloseParenToken code = blockStatement;
setTurnTimeoutStatement: SetTurnTimeoutToken OpenParenToken turnCount = singleExpression CloseParenToken code = blockStatement;
setTurnTimeoutIdStatement: SetTurnTimeoutIDToken OpenParenToken turnCount = singleExpression CommaToken name = singleExpression CloseParenToken code = blockStatement;

expressionSequence  : sequenceExpressions += singleExpression (CommaToken sequenceExpressions += singleExpression)*;

arguments
    : OpenParenToken(
          argumentExpressions += singleExpression (CommaToken argumentExpressions += singleExpression)*
       )?CloseParenToken
    ;

//base expression that allows recursive traversal
singleExpression :
      val = literal																											# LiteralExpression
    | OpenParenToken expression = expressionSequence CloseParenToken														# ParenthesizedExpression
    | member = singleExpression '.' property = identifierName																# MemberDotExpression
    | '++' {this.NotLineTerminator();} unaryExpression = singleExpression													# PreIncrementExpression
    | '--' {this.NotLineTerminator();} unaryExpression = singleExpression													# PreDecreaseExpression
    | unaryExpression = singleExpression {this.NotLineTerminator();} '++'													# PostIncrementExpression
    | unaryExpression = singleExpression {this.NotLineTerminator();} '--'													# PostDecreaseExpression
    | '+' unaryExpression = singleExpression																				# UnaryPlusExpression
    | '-' unaryExpression = singleExpression																				# UnaryMinusExpression
    | NotToken negatedExpression = singleExpression																			# NotExpression
    | condition = singleExpression '?' firstValue = singleExpression ':' secondValue = singleExpression						# TernaryExpression
    | functionExpression = singleExpression arguments																		# FunctionCallExpression
    | lvalue = singleExpression op = ('*' | '/' | '%') rvalue = singleExpression											# MultiplicativeExpression
    | lvalue = singleExpression op = ('+' | '-') rvalue = singleExpression													# AdditiveExpression
    | lvalue = singleExpression '=' rvalue = singleExpression																# AssignmentOrEqualityExpression
    | lvalue = singleExpression '!='  rvalue = singleExpression																# InequalityExpression
    | lvalue = singleExpression op = ('<' | '>' | '<=' | '>=') rvalue = singleExpression									# RelationalExpression
    | lvalue = singleExpression AndToken rvalue = singleExpression															# LogicalAndExpression
    | lvalue = singleExpression OrToken rvalue = singleExpression															# LogicalOrExpression
    | lvalue = singleExpression ScriptAssignToken rvalue = statement														# ScriptAssignmentExpression
    | lvalue = singleExpression assignmentOperator rvalue = singleExpression												# CalculationAndAssignmentOperatorExpression
    | indexExpression = singleExpression '[' indexerExpression = expressionSequence ']'										# MemberIndexExpression
    | arrayLiteral																											# ArrayLiteralExpression
    | ThisToken																												# ThisExpression
    | IdentifierToken																										# IdentifierExpression
    | CloneToken IdentifierToken																							# CloneObjectExpression
    | CreateToken ExitToken args = arguments																				# CreateExitExpression
    | CreateToken TimerToken args = arguments																				# CreateTimerExpression
    | CreateToken TurnscriptToken args = arguments																			# CreateTurnscriptExpression
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

arrayLiteral : '[' values = elementList? ']';
elementList : elements += singleExpression (CommaToken elements += singleExpression)*;

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
	| ListToken
	| DictionaryToken
	| AddToken
	| RemoveToken
	| SetTimeoutToken
	| SetTimeoutIDToken
	| SetTimerScriptToken
	| SetTurnScriptToken
	| SetTurnTimeoutToken
	| AndToken
	| OrToken
	| NotToken
    ;

AndToken:							 'and';
OrToken:							 'or';
NotToken:						     'not';
ScriptAssignToken:					 '=>';
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
ListToken:							 'list';
DictionaryToken:					 'dictionary';
AddToken:						     'add';
RemoveToken:						 'remove';
SetTimeoutToken:				     'SetTimeout';
SetTimeoutIDToken:				     'SetTimeoutID';
SetTimerScriptToken:				 'SetTimerScript';
SetTurnScriptToken:					 'SetTurnScript';
SetTurnTimeoutToken:				 'SetTurnTimeout';
SetTurnTimeoutIDToken:				 'SetTurnTimeoutID';

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
