grammar QuestScript;

@parser::header{
using System;
}

@lexer::members{
private bool _isInsideSwitch = false;
private int _switchBracerIndentation = 0;
private bool _isInsideSwitchCodeBlock = false;
}

@parser::members{
public Dictionary<string,HashSet<string>> ObjectFields = new Dictionary<string,HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
public Dictionary<string,(string name, HashSet<string> parameters)> ObjectMethods = new Dictionary<string,(string name, HashSet<string> parameters)>(StringComparer.InvariantCultureIgnoreCase);

//value here is the parameters of the function
public Dictionary<string,(HashSet<string> parameters, string returnType)> Functions = new Dictionary<string,(HashSet<string> parameters, string returnType)>(StringComparer.InvariantCultureIgnoreCase);


private bool IsField(string instance, string fieldName)
{
    var isObjectDefined = ObjectFields.TryGetValue(instance, out var fields);
    return isObjectDefined && fields.Contains(fieldName);
}

}

script: (statement)* EOF;

statement
	:
        functionStatement
    |   assignmentStatement
    |   codeBlockStatement
    |   ifStatement
    |   specialFunctionStatement
    |   scriptAssignmentStatement
    |   firsttimeStatement //special case of syntax
    |   iterationStatement
    |   breakStatement
    |   returnStatement
	|	switchCaseStatement
	|	{ _input.La(1) == QuestScriptLexer.PlusPlus || _input.La(1) == QuestScriptLexer.MinusMinus}? postfixUnaryStatement
	|   memberMethodStatement
    ;

switchCaseStatement:
	'switch' LeftParen switchConditionStatement = expression RightParen
	'{'
		cases += caseStatement*
		defaultContext = defaultStatement?
	'}';

caseStatement : Case LeftParen caseFirstValue = literal (',' caseOtherValues += literal)* RightParen code = statement;
defaultStatement : Default code = statement;

returnStatement: 'return' (LeftParen expression? RightParen)?;

breakStatement: Break;

iterationStatement
    :
      'while' LeftParen condition = expression RightParen code = statement		           #WhileStatement
    | 'foreach' LeftParen iterationVariable = Identifier ','
				    enumerationVariable = expression RightParen
				        code = statement                             #ForEachStatement
    | 'for' LeftParen iterationVariable = Identifier ','
							  iterationStart = expression ','
							  iterationEnd = expression 
							  (',' step = expression)?
			  RightParen code = statement                                   #ForStatement
    ;


ifStatement:
    'if' LeftParen condition = expression RightParen
        ifCode = statement
    ('elseif' LeftParen elseifConditions += expression RightParen
        elseIfCodes += statement
    )*
    ('else'
        elseCode = statement
    )?
    ;

codeBlockStatement :
	'{' statements += statement* '}';

argumentsList: (args += expression (',' args += expression)*);

//for special script syntax such as "on ready { ... script ... }
specialFunctionStatement: functionName = SpecialFunctionName (LeftParen argumentsList RightParen)? codeBlockStatement?;

firsttimeStatement: 'firsttime' firstTimeScript = codeBlockStatement ('otherwise' otherwiseScript = codeBlockStatement)?;

functionStatement: functionName = Identifier (LeftParen argumentsList? RightParen)?;

assignmentStatement: LVal = lValue '=' RVal = expression;

scriptAssignmentStatement: LVal = lValue  '=>' RVal = codeBlockStatement;

arrayLiteral : '[' (elements += expression (',' elements += expression)*)? ']';

postfixUnaryStatement: expression op = (PlusPlus|MinusMinus);

memberMethodStatement: instance = expression '.' method = functionStatement;

//expressions evaluate to some value...
expression:
        rValue														#OperandExpression
    |   LeftParen expr = expression RightParen						#ParenthesizedExpression
    |   '-' expr = expression                                       #NegatedExpression
    |   instance = expression '[' parameter = expression ']'		#IndexerExpression
    |   left = expression op = multiplicativeOp right = expression  #MultiplicativeExpression
    |   left = expression op = additiveOp right = expression		#AdditiveExpression
    |   expr = expression op = (PlusPlus|MinusMinus)				#PostfixUnaryExpression
	|	expr = arrayLiteral											#ArrayLiteralExpression
	|	instance = expression '.' method = functionStatement		#MemberMethodExpression
    |   left = expression op = relationalOp right = expression		#RelationalExpression
    |   Not expr = expression										#NotExpression
    |   left = expression And right = expression					#AndExpression
    |   left = expression Or right = expression						#OrExpression
    |   val = expression 'in' source = expression                   #InExpression
    ;

rValue:
       
        literal					   #LiteralOperand
     | lValue					   #VariableOperand
     | expr = functionStatement    #FunctionOperand
    ;

lValue
	 :
        Identifier									#IdentifierOperand
    |   instance = lValue '.' member = Identifier   #MemberFieldOperand
    ;


relationalOp:
        '>'     #GreaterOp
    |   '>='    #GreaterOrEqualsOp
    |   '<'     #LesserOp
    |   '<='    #LesserOrEqualsOp
    |   '!='    #NotEqualsOp
    |   '<>'    #AlternateNotEqualsOp
    |   '='     #EqualsOp
    ;

multiplicativeOp:
        '/'     #DivOp
    |   '%'     #ModOp
    |   '*'     #ModOp
    ;

additiveOp:
        '+'     #PlusOp
    |   '-'     #MinusOp
	;

literal:
	  IntegerLiteral #IntegerLiteral
	| DoubleLiteral  #DoubleLiteral
	| StringLiteral	 #StringLiteral
	| NullLiteral	 #NullLiteral
	| BooleanLiteral #BooleanLiteral
	;


LeftParen : '(';
RightParen : ')';
LeftBracket : '[';
RightBracket : ']';

LeftCurly: '{' 
{ 
	if (_isInsideSwitch) 
	{ 
		if(_switchBracerIndentation >= 1)
		{
			_isInsideSwitchCodeBlock = true; 
		}
		_switchBracerIndentation++; 
	}
};

RightCurly: '}'
{ 
	if (_isInsideSwitch) 
	{ 
		if(_switchBracerIndentation >= 1)
		{
			_isInsideSwitchCodeBlock = false; 
		}
		_switchBracerIndentation--; 
		if(_switchBracerIndentation == 0) 
			_isInsideSwitch = false; 
	} 
};

PlusPlus: '++';
MinusMinus: '--';
Not : 'not';
And : 'and';
Or : 'or';

StringLiteral: '"' (AnyCharacterExceptSpecial | EscapeSequence)* '"';

DoubleLiteral: Digit+ '.' Digit+;
IntegerLiteral: Digit+;

NullLiteral: 'null';
BooleanLiteral: 'true' | 'false';

SpecialFunctionName:
        'get input'
    |   'list add'
    |   'list remove'
    |   'dictionary add'
    |   'dictionary remove'
    |   'create exit'
    |   'create timer'
    |   'create turnscript'
    |   'ask'
    |   'on ready'
    |   'play sound'
    |   'stop sound'
    |   'show menu'
    |   'start transaction'
    |   'undo'
    |   'wait'
    |   'SetTimeout'
    |   'SetTimeoutID'
    |   'SetTimerScript'
    |   'SetTurnScript'
    |   'SetTurnTimeout'
    |   'SetTurnTimeoutID'
    ;


fragment AnyCharacterExceptSpecial : ~["\\\r\n\u0085\u2028\u2029];
fragment EscapeSequence : '\\' ['"?abfnrtv\\];
fragment Letter : [A-Za-z];
fragment NonZeroDigit: [1-9];
fragment Digit: [0-9];

CDataBegin: '<![CDATA[' -> skip;
CDataEnd: ']]>' -> skip;

Whitespace: (' '|'\t') -> skip;
Comment: '/*' .*? '*/' -> channel(HIDDEN);
LineComment: '//' ~[\r\n]* -> channel(HIDDEN);
Newline: '\r'? '\n' -> skip;


Switch: 'switch' { _isInsideSwitch = true; };
Case: { _isInsideSwitch && !_isInsideSwitchCodeBlock }? 'case';
Default: { _isInsideSwitch && !_isInsideSwitchCodeBlock }? 'default';

Identifier: (Letter | '_') (Letter | Digit | '_')*;
