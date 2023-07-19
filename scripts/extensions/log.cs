//Planned logging
//admin actions
//damage
//kills
//roles
//shop purchases
//item interaction

//refactor:
//each log will contain a message type and fields
//this means we can reconstruct the message using tagged string construction saving string space
//also use dataInstance as a standard saving method
$Log::MaxStringLen = 10000000;

function Log_

function Log::Clear(%log)
{
	%count = %log.logNum;
	for(%i = 0; %i < %count; %i++)
	{
		%log.log[%i] = "";
	}
	%log.logNum = 0;
}

function Log::Append(%log,%string)
{
	%logNum = %log.logNum + 0;
	if($Log::MaxStringLen < strLen(%log.log[%logNum]))
	{
		%logNum = %log.logNum++;
	}

	%log.log[%logNum] = %log.log[%logNum] @ %string NL "";
}

function Log::Get(%log,%i)
{
	%c = 0;
	while(%i > (%currCount = getRecordCount(%log.log[%c])))
	{
		%i -= %currCount;
		%c++;
	}

	%log.get_c = %c
	%log.get_i = %i;
	%log.get_currCount = %currCount;
	return getRecord(%log.log[%c],%i);
}

function Log::GetNext(%log)
{
	%c = %log.get_c;
	%i = %log.get_i++;
	%currCount = %log.get_currCount;
	if(%i > %currCount)
	{
		%log.get_i = %i -= %currCount;
		%log.get_c = %c++;
		%log.get_currCount = getRecordCount(%log.log[%c]);
	}

	return getRecord(%log.log[%c],%i);
}

function Log::Log(%log,%type,%tags,%data)
{
	%log.append(%type TAB %tags TAB %data);

}

function Log::Save(%log,%filepath)
{
	%file = new fileObject();
	if(%success = %file.openForWrite(%filepath))
	{
		%count = %log.logNum;
		for(%i = 0; %i < %count; %i++)
		{
			%file.writeLine(%log.log[%i]);
		}
	}

	%file.close();
	%file.delete();
	return %succes;
}

function Log::Load(%log,%filepath)
{
	%file = new fileObject();
	if(%success = %file.openForRead(%filepath))
	{
		while(!%file.isEOF())
		{
			%log.append(%file.readLine());
		}
	}

	%file.close();
	%file.delete();
	return %succes;
}

function Log::OfType(%log,%type)
{
	//create a new log that the proper typed elements will be added to
	%returnLog = new scriptObject(){class = "Log";};

	%currRecord = %log.get(0);
	while(%currRecord !$= "")
	{
		%currType = getField(%currRecord,0);

		if(%currType $= %type)
		{
			%log.append(%currRecord);
		}

		%currRecord = %log.getNext();
	}

	return %returnLog;
}

function Log::WithTag(%log,%client)
{
	//create a new log that the elements with that client will be added to
	%returnLog = new scriptObject(){class = "Log";};

	%currRecord = %log.get(0);
	while(%currRecord !$= "")
	{
		%currTags = getField(%currRecord,1);
		%currTagCount = getWordCount(%currClients);

		for(%j = 0; %j < %currTagCount; %j++)
		{
			%currTag = getWord(%currTags,%j);

			if(%currTag $= %client)
			{
				%log.append(%currRecord);
			}
		}

		%currRecord = %log.getNext();
	}

	return %returnLog;
}