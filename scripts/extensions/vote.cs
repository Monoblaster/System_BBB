
function vote_start(%func,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l)
{
	if($CurrentVote)
	{
		return false;
	}

	$CurrentVote = new ScriptObject()
	{
		func = %func;
		typeName[%count++-1] = %a;
		typeNumber[%a] = %count-1;
		typeName[%count++-1] = %b;
		typeNumber[%b] = %count-1;
		typeName[%count++-1] = %c;
		typeNumber[%c] = %count-1;
		typeName[%count++-1] = %d;
		typeNumber[%d] = %count-1;
		typeName[%count++-1] = %e;
		typeNumber[%e] = %count-1;
		typeName[%count++-1] = %f;
		typeNumber[%f] = %count-1;
		typeName[%count++-1] = %g;
		typeNumber[%g] = %count-1;
		typeName[%count++-1] = %h;
		typeNumber[%h] = %count-1;
		typeName[%count++-1] = %i;
		typeNumber[%i] = %count-1;
		typeName[%count++-1] = %j;
		typeNumber[%j] = %count-1;
		typeName[%count++-1] = %k;
		typeNumber[%k] = %count-1;
		typeName[%count++-1] = %l;
		typeNumber[%l] = %count-1;
	}
	return true;
}

function vote_end()
{
	%obj = $CurrentVote;
	if(!isObject($CurrentVote))
	{
		return false;
	}

	
	%winners = 0;
	%winningTotal = %obj.typetotal[0];
	%count = 1;
	while(%obj.typeName[%count] !$= "")
	{
		%currTotal = %obj.typetotal[%count];
		if(%winningTotal < %currTotal)
		{
			%winners = %count;
			%winningTotal = %currTotal;
			continue;
		}

		if(%winningTotal == %currTotal)
		{
			%winners = %winers SPC %count;
		}
		%count ++;
	}

	$CurrentVote = "";
	return %winners TAB %winningTotal;
}

function Vote_addCrossUniqueVote(%client,%type,%number)
{
	%obj = $CurrentVote;

	if(%obj.voted[%client.getBLID()])
	{
		%client.chatMessage("You cannot vote again");
		return;
	}

	%number = %obj.typeNumber[%type];
	%name = %type
	if(%obj.typeName[%type] !$= "")
	{
		%name = %obj.typeName[%type];
		%number  = %type;
	}

	if(%number $= "")
	{
		%client.chatMessage(%type SPC "is not an option");
		return;
	}

	%obj.voted[%client.getBLID()] = true;
	%obj.typetotal[%number] += 1;

	%client.chatMessage("You have voted for" SPC %type);
}

function serverCmdYes(%client)
{
	call($CurrentVote.func,%client,"yes",1);
}

function serverCmdNo(%client)
{
	call($CurrentVote.func,%client,"no",1);
}