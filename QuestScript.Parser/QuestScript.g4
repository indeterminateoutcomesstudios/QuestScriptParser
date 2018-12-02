grammar QuestScript;

@parser::header{
using System;
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

caseStatement : 'case' LeftParen caseValue = literal RightParen code = statement;
defaultStatement : 'default' code = statement;

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
    |   instance = expression '[' parameter = expression ']'		#IndexerExpression
    |   left = expression op = multiplicativeOp right = expression  #MultiplicativeExpression
    |   left = expression op = additiveOp right = expression		#AdditiveExpression
    |   expr = expression op = (PlusPlus|MinusMinus)				#PostfixUnaryExpression
	|	expr = arrayLiteral											#ArrayLiteralExpression
	|	'this'														#ThisExpression
	|	instance = expression '.' method = functionStatement		#MemberMethodExpression
    |   left = expression op = relationalOp right = expression		#RelationalExpression
    |   Not expr = expression										#NotExpression
    |   left = expression And right = expression					#AndExpression
    |   left = expression Or right = expression						#OrExpression
    ;

rValue:
       
       literal					   #LiteralOperand
     | ('-')? literal              #NegativeLiteralOperand
     | lValue					   #VariableOperand
     | ('-')? lValue			   #NegativeVariableOperand
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

Keyword:
        'finish'
    |   'firsttime'
    |   'for'
    |   'foreach'
    |   'if'
    |   'return'
    |   'switch'
    |   'case'
    |   'default'
    |   'while'
    |   'undo'
    ;

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

//Continue: 'continue';
Break: 'break';

Identifier: (Letter | '_') (Letter | Digit | '_')*;

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
//IgnoreNewline: '\r'? '\n' {nesting>0}? -> skip;
