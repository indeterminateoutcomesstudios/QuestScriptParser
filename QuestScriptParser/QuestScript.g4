grammar QuestScript;

script: (statement)* EOF;

statement:
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
	switch = switchStatement
	'{' 
		cases += caseStatement* 
		defaultContext = defaultStatement? 
	'}';

switchStatement : 'switch' '(' switchConditionStatement = expression ')';
caseStatement : 'case' '(' caseValue = literal ')' code = codeBlockStatement;
defaultStatement : 'default' code = codeBlockStatement;

returnStatement: 'return' ('(' expression? ')')?;

continueStatement: Continue;
breakStatement: Break;

iterationStatement
    : 'do' code = codeBlockStatement 'while' '(' condition = expression ')'   #DoStatement
    | 'while' '(' condition = expression ')' code = codeBlockStatement        #WhileStatement
    | 'foreach' '(' iterationVariable = Identifier ':'
				    enumerationVariable = expression')'
				        code = codeBlockStatement                             #ForEachStatement
    | 'for' '(' iterationVariable = Identifier ','
							  iterationStart = IntegerLiteral ','
							  iterationEnd = IntegerLiteral
			  ')' code = codeBlockStatement                                   #ForStatement
    ;


ifStatement:
    'if' '(' condition = expression ')'
        ifCode = statement
    ('elseif' '(' elseifConditions += expression ')'
        elseIfCodes += statement
    )*
    ('else'
        elseCode = statement
    )?

    ;

codeBlockStatement: '{' statements += statement* '}';

argumentsList: (args += expression (',' args += expression)*)?;

//for special script syntax such as "on ready { ... script ... }
specialFunctionStatement: functionName = SpecialFunctionName ('(' argumentsList ')')? codeBlockStatement?;

firsttimeStatement: 'firsttime' firstTimeScript = codeBlockStatement ('otherwise' otherwiseScript = codeBlockStatement)?;

functionStatement: functionName = Identifier '(' argumentsList ')';

assignmentStatement: LVal = lValue '=' RVal = expression;

scriptAssignmentStatement: LVal = lValue '=>' RVal = codeBlockStatement;

arrayLiteral : '[' (elements += expression (',' elements += expression)*)? ']';

//expressions evaluate to some value...
expression:
        rValue                                                  #OperandExpression
    |   '(' expression ')'                                      #ParenthesizedExpression
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

lValue:
        Identifier              #IdentifierOperand
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

Whitespace: [ \t]+ -> channel(HIDDEN);
Comment: '/*' .*? '*/' -> channel(HIDDEN);
LineTerminator: [\r\n]+ -> channel(HIDDEN);
LineComment: '//' ~[\r\n]* -> channel(HIDDEN);