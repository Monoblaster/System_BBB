if(!isObject($CommandHandler))
{
	$CommandHandler = new ScriptObject(){num = 0;};
}


function CommandHandler_GetTarget(%str)
{
	if(strStr(%str,"BLID") != 0)
	{
		%wordCount = getWordCount(%str);
		for(%i = 0;%i < %wordCount && !(%target = findClientByName(getWords(%str,0,%i))); %i++){}
		%str = getWords(%str,%i + 1);
	}
	else
	{
		%target = findClientByBL_ID(getSubStr(getWord(%str,0),4,10));
		%str = getWords(%str,1);
	}

	return %target SPC %str;
}

function CommandHandler_New(%name,%function,%access,%fields,%fieldHelp,%descHelp)
{
	%e = %e @ "function serverCmd" @ %name @ "(%client,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p,%q,%r,%s,%t,%u,%v,%w,%x,%y,%z)";
	%e = %e @ "{";
		%e = %e @ "%str=trim(%b SPC %c SPC %d SPC %e SPC %f SPC %g SPC %h SPC %i SPC %j SPC %k SPC %l SPC %m SPC %n SPC %o SPC";
		%e = %e @ "%p SPC %q SPC %r SPC %s SPC %t SPC %u SPC %v SPC %w SPC %x SPC %y SPC %z);";

	//generating access restrictor
	switch(%access)
	{
	case 1:
		%e = %e @ "if(!%client.isAdmin)return\"\";";
	case 2:
		%e = %e @ "if(!%client.isSuperAdmin)return\"\";";
	case 3:
		%e = %e @ "if(!%client.isHost)return\"\";";
	}

	//generating the field code
	%c = 0;
	%fieldCount = getFieldCount(%fields);
	for(%i = 0; %i < %fieldCount; %i++)
	{
		%field = getField(%fields,%i);
		%type = getWord(%field,0);
		switch$(%type)
		{
		case "target":
			%e = %e @ "%temp = CommandHandler_GetTarget(%str);%a["@%c@"] = getWord(%temp,0);%str = getWords(%temp,1);";
			%c++;
		case "words":
			%num = getWords(%field,1);
			%e = %e @ "%a["@%c@"] = getWords(%str,0,"@%num-1@");%str = getWords(%str,"@%num@");";
			%c++;
		}
	}

		%e = %e @ %function @ "(%client,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17,%a18,%a19,%a20,%a21);";
	%e = %e @ "}";
	echo(%e);
	eval(%e);

	$CommandHandler.numToName[$CommandHandler.num] = %name;
	$CommandHandler.nameToNum[%name] = $CommandHandler.num;
	$CommandHandler.nameToHelp[%name] = %helpString;
	$CommandHandler.NumToAccess[$CommandHandler.num] = %access;
	$CommandHandler.num++;
}