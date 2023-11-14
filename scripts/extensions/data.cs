//adds in a function to create datablock like structures with scriptobjects

function Data_Clear()
{
	%count = getWordCount($Data::List);
	for(%i = 0; %i < %count; %i++)
	{
		%data = getWord(%count,%i);
		if(isObject(%data))
		{
			%data.delete();
		}
	}
	$Data::List = "";
}
Data_Clear();

function Data_New(%class,%data)
{
	%e = "";
	%start = 0;
	while((%sep = strPos(%data,"=",%start)) != -1)
	{
		%end = strPos(%data,"\n",%sep);
		if(%end == -1)
		{
			%end = strLen(%data) - 1;
		}
		%valLen = %end - %sep;
		%nameLen = %sep - %start;

		%name = trim(getSubStr(%data,%start,%nameLen));
		%val = trim(getSubStr(%data,%sep + 1,%valLen));

		%e = %e @ %name @ "=\"" @ %val @ "\";";

		%start = %end + 1;
	}

	eval("%obj = new scriptObject(){class=getWord(%class,0)@getWord(%class,1);superClass=getWord(%class,1);"@%e@"};");
	$Data::List = $Data::List SPC %obj;
	return %obj;
}