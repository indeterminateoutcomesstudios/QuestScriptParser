parser grammar QuestGameParser;

options { tokenVocab=QuestGameLexer; }

game: misc* element misc* EOF;

content     :   chardata?
                ((element | Comment) chardata?)* ;
 
element     : { ($ElementName).Text == "function" || ($ElementName).Text == "script" }?  
					Open ElementName = Name attribute* CloseFunction Content CloseContent Slash Name Close
			|	Open ElementName = Name attribute* Close content Open Slash Name Close
            |   Open ElementName = Name attribute* SlashClose
            ;


attribute   :   Key = Name Equal Value = String ;

chardata    :   TextContent;

misc : Comment;