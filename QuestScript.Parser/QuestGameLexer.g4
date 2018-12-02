lexer grammar QuestGameLexer;

@members
{
	bool? IsScriptElement;
}

Open: '<' -> pushMode(INSIDE_TAG);
Comment :   '<!--' .*? '-->' ;
DTD : '<!' .*? '>' -> skip ;
ControlCharacters : (' '|'\t'|'\r'? '\n') -> skip;
TextContent: ~[<&]+ ;

mode INSIDE_TAG;
CloseFunction : {IsScriptElement ?? false}? '>' -> mode(FUNCTION_CONTENT);
Close       : {!(IsScriptElement ?? false)}?  '>' { IsScriptElement = null; } -> popMode;
SlashClose  :   '/>' { IsScriptElement = null; } -> popMode ;
Slash       :   '/' ;
Equal       :   '=' ;
String      :   '"' ~[<"]* '"'
            |   '\'' ~[<']* '\''
            ;
Name        :   NameStartChar NameChar* { if(IsScriptElement == null) IsScriptElement = _text == "function" || _text == "script"; };
S           :   [ \t\r\n] -> skip ;

fragment
HEXDIGIT    :   [a-fA-F0-9] ;

fragment
DIGIT       :   [0-9] ;

fragment
NameChar    :   NameStartChar
            |   '-' | '_' | '.' | DIGIT
            |   '\u00B7'
            |   '\u0300'..'\u036F'
            |   '\u203F'..'\u2040'
            ;

fragment
NameStartChar
            :   [:a-zA-Z]
            |   '\u2070'..'\u218F'
            |   '\u2C00'..'\u2FEF'
            |   '\u3001'..'\uD7FF'
            |   '\uF900'..'\uFDCF'
            |   '\uFDF0'..'\uFFFD'
;

mode FUNCTION_CONTENT;
CDataBegin: '<![CDATA[' -> skip;
CDataEnd: ']]>' -> skip;
CodeStyling:   [\t\r\n] -> skip;
CloseContent: {_input.La(1) == '/'}? '<' {IsScriptElement = null;} -> mode(INSIDE_TAG);
CodeNotEquals: {_input.La(1) == '>'}? '<' -> more;
Content: . -> more;

