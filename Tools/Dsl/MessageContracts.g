grammar MessageContracts;

options 
{
	language = 'CSharp2'; 
	output=AST; 
}

tokens 
{
	TypeToken;	
	CommandToken;
	EventToken;	
	MemberToken;
	BlockToken;
	DisctionaryToken;
	FragmentGroup;
	FragmentEntry;
	FragmentReference;
	ModifierDefinition;
	EntityDefinition;
}

@lexer::namespace { MessageContracts }
@parser::namespace { MessageContracts }

program	
	:	declaration+
	;
	
declaration
	: modifier_declaration
	| frag_declaration
	| type_declaration
	| entity_declaration
	;

frag_declaration 
	: LET ID '=' ID ID ';' -> ^(FragmentEntry ID ID ID);  
    
modifier_declaration
	: USING Modifier '=' ID ';' -> ^(ModifierDefinition ID Modifier);
    
	
entity_declaration
	: ENTITY ID block ';' -> ^(EntityDefinition ID block);
	
type_declaration
	: ID Modifier? block -> ^(TypeToken ID block Modifier?);
	
member 	
	:	ID ID -> ^(MemberToken ID ID)
	|	ID -> ^(FragmentReference ID)
	;

	
block
    :   lc='('
            (member (',' member)*)?
        ')'
        -> ^(BlockToken[$lc,"Block"] member*)
    ;    

USING
	: 'using';
LET
	: 'let';	
ENTITY 	:	'entity';
ID  :	('a'..'z'|'A'..'Z'|'_')('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'<'|'>'|'['|']')* ;


Modifier
	: '?'
	| '!'
	| ';'
	;


INT :	'0'..'9'+;   


COMMENT
    :   '//' ~('\n'|'\r')* '\r'? '\n' {$channel=HIDDEN;}
    |   '/*' ( options {greedy=false;} : . )* '*/' {$channel=HIDDEN;}
    ;

WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        ) {$channel=HIDDEN;}
    ;  