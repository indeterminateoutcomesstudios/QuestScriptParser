lexer grammar QuestGameLexer;

COMMENT     :   '<!--' .*? '-->';
DTD         :   '<!' .*? '>'     -> skip ;
ENTITY_REF  :   '&' Name ';' ;
CHAR_REF    :   '&#' DIGIT+ ';'
            |   '&#x' HEXDIGIT+ ';'
            ;
CDATA_START :   '<![CDATA[' -> pushMode(INSIDE_TAG_CDATA);
SEA_WS      :   (' '|'\t'|'\r'? '\n')+;

OPEN        :   '<'                     -> pushMode(INSIDE_TAG) ;
SPECIAL_OPEN:   '<?' Name               -> more, pushMode(PROC_INSTR) ;

SCRIPT        :   ~[<&]+ ;        // match any 16 bit char other than < and &

mode INSIDE_TAG_CDATA;
CDATA_END   : ']]>'	-> popMode;
ESCAPED_SCRIPT : .  -> more;

mode INSIDE_TAG;

CLOSE       :   '>'                     -> popMode ;
SPECIAL_CLOSE:  '?>'                    -> popMode ; // close <?xml...?>
SLASH_CLOSE :   '/>'                    -> popMode ;
SLASH       :   '/' ;
EQUALS      :   '=' ;
STRING      :   '"' ~[<"]* '"'
            |   '\'' ~[<']* '\''
            ;
Name        :   NameStartChar NameChar* ;
S           :   [ \t\r\n]               -> skip ;

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

// ----------------- Handle <? ... ?> ---------------------
mode PROC_INSTR;

PI          :   '?>'                    -> popMode ; // close <?...?>
IGNORE      :   .                       -> more ;