grammar QuestScript;
 
@lexer::members 
{
	int nesting = 0;
}

script: (statement)* EOF;
 
statement	
	locals [HashSet<string> symbolsInScope = new HashSet<string>()]
	:
        functionStatement
    |   assignmentStatement
    |   codeBlockStatement
    |   ifStatement
    |   specialFunctionStatement
    |   scriptAssignmentStatement
    |   firsttimeStatement //special case of syntax
    |   iterationStatement
    |   continueStatement
    |   breakStatement
    |   returnStatement
	|	switchCaseStatement
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

continueStatement: Continue;
breakStatement: Break;

iterationStatement
    : 'do' code = codeBlockStatement 'while' LeftParen condition = expression RightParen   #DoStatement
    | 'while' LeftParen condition = expression RightParen code = codeBlockStatement        #WhileStatement
    | 'foreach' LeftParen iterationVariable = Identifier ':'
				    enumerationVariable = expression RightParen
					{ $statement::symbolsInScope.Add($iterationVariable.text); }
				        code = codeBlockStatement                             #ForEachStatement
    | 'for' LeftParen iterationVariable = Identifier ','
							  iterationStart = IntegerLiteral ','
							  iterationEnd = IntegerLiteral
			  { $statement::symbolsInScope.Add($iterationVariable.text); }
			  RightParen code = codeBlockStatement                                   #ForStatement
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

argumentsList: (args += expression (',' args += expression)*)?;

//for special script syntax such as "on ready { ... script ... }
specialFunctionStatement: functionName = SpecialFunctionName (LeftParen argumentsList RightParen)? codeBlockStatement?;

firsttimeStatement: 'firsttime' firstTimeScript = codeBlockStatement ('otherwise' otherwiseScript = codeBlockStatement)?;

functionStatement: functionName = Identifier LeftParen argumentsList RightParen;

assignmentStatement: LVal = lValue '=' RVal = expression { $statement::symbolsInScope.Add($LVal.text); };

scriptAssignmentStatement: LVal = lValue  '=>' RVal = codeBlockStatement;

arrayLiteral : '[' (elements += expression (',' elements += expression)*)? ']';

//expressions evaluate to some value...
expression:
        rValue                                                  #OperandExpression
    |   LeftParen expression RightParen                         #ParenthesizedExpression
    |   instance = expression '[' parameter = expression ']'    #IndexerExpression
    |   left = expression op = relationalOp right = expression  #RelationalExpression
    |   left = expression op = logicalOp right = expression     #LogicalExpression
    |   left = expression op = arithmeticOp right = expression  #ArithmeticExpression
    |   Not expr = expression                                   #NotExpression
    |   op = unaryOp expr = expression                          #PrefixUnaryExpression
    |   expr = expression op = (PlusPlus|MinusMinus)            #PostfixUnaryExpression
	|	arrayLiteral											#ArrayLiteralExpression
	|	'this'													#ThisExpression
	|	expression '.' functionStatement						#MemberMethodExpression
    ;

rValue:
       functionStatement    #FunctionOperand
     | literal              #LiteralOperand
     | lValue               #VariableOperand
    ;

lValue
	 :
        Identifier				#IdentifierOperand	 
    |   lValue '.' Identifier   #MemberFieldOperand
    ;

unaryOp:
        '+'     #UnaryPlusOp
    |   '-'     #UnaryMinusOp
    |   '++'    #PlusPlusOp
    |   '--'    #MinusMinusOp
    ;

relationalOp:
        '>'     #GreaterOp
    |   '>='    #GreaterOrEqualsOp
    |   '<'     #LesserOp
    |   '<='    #LesserOrEqualsOp
    |   '!='    #NotEqualsOp
    |   '='     #EqualsOp
    ;

logicalOp:
        'and'   #AndOp
    |   'or'    #OrOp
    ;

arithmeticOp:
        '+'     #PlusOp
    |   '-'     #MinusOp
    |   '/'     #DivOp
    |   '%'     #ModOp
    |   '*'     #ModOp
    ;

literal: numericLiteral | StringLiteral | NullLiteral | BooleanLiteral;

numericLiteral: IntegerLiteral | DoubleLiteral;


LeftParen : '(' {nesting++;} ;
RightParen : ')' {nesting--;} ;
LeftBracket : '[' {nesting++;} ;
RightBracket : ']' {nesting--;} ;
  
PlusPlus: '++';
MinusMinus: '--';
Not : 'not';
 
StringLiteral: '"' (AnyCharacterExceptSpecial | EscapeSequence)* '"';

DoubleLiteral: Digit+ '.' Digit+;
IntegerLiteral: Digit+;

NullLiteral: 'null';
BooleanLiteral: 'true' | 'false';

Keyword:
        'do'
    |   'finish'
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

Continue: 'continue';
Break: 'break';

Identifier: (Letter | '_') (Letter | Digit | '_')*;

fragment AnyCharacterExceptSpecial : ~["\\\r\n\u0085\u2028\u2029];
fragment EscapeSequence : '\\' ['"?abfnrtv\\];
fragment Letter : [A-Za-z];
fragment NonZeroDigit: [1-9];
fragment Digit: [0-9];

Whitespace: (' '|'\t') -> skip;
Comment: '/*' .*? '*/' -> channel(HIDDEN);
LineComment: '//' ~[\r\n]* -> channel(HIDDEN);
Newline: '\r'? '\n' -> skip;
//IgnoreNewline: '\r'? '\n' {nesting>0}? -> skip;
