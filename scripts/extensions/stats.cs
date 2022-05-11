$StatSaver::RegisteredFields = 0;
$StatSaver::RegisteredArrays = 0;

function registerSavedField(%name,%initial)
{
    %count = $StatSaver::RegisteredFields;
    for(%i = 0; %i < %count; %i++)
    {
        if($StatSaver::Field[0] $= %name)
        {
            return false;
        }
    }

    $StatSaver::Field[%count] = %name;
	$StatSaver::FieldInit[%count] = %initial;
    $StatSaver::RegisteredFields++;

    return true;
}

function registerSavedArray(%name,%initial)
{
    %count = $StatSaver::RegisteredArrays;
    for(%i = 0; %i < %count; %i++)
    {
        if($StatSaver::Array[0] $= %name)
        {
            return false;
        }
    }

    $StatSaver::Array[%count] = %name;
	$StatSaver::ArrayInit[%count] = %initial;
    $StatSaver::RegisteredArrays++;

    return true;
}

function GameConnection::SaveStats(%client)
{
	echo("Saving stats for" SPC %client.getPlayerName());

    %out = new FileObject();
    %success = %out.openForWrite("config/server/BBB/stats/" @ %client.getBLID() @ ".txt");

    if(!%success)
    {
		echo("Failed to open file");
		%out.close();
		%out.delete();
        return false;
    }

    %count = $StatSaver::RegisteredFields;
    for(%i = 0; %i < %count; %i++)
    {
        %field = $StatSaver::Field[%i];

		%value = %client.getObjField(%field);
		if(%value $= "")
		{
			%value = $StatSaver::FieldInit[%i];
		}
        
        %out.writeLine(%field TAB %value);
    }

	%count = $StatSaver::RegisteredArrays;
    for(%i = 0; %i < %count; %i++)
    {
        %array = $StatSaver::Array[%i];
		%c = 0;
		while((%value = %client.getObjField(%array @ %c)) !$= "")
		{
			if(%value $= "")
			{
				%value = $StatSaver::ArrayInit[%i];
			}
			
			%out.writeLine(%array @ %c TAB %value);
			%c++;
		}
    }

	%out.close();
	%out.delete();
    return true;
}

function GameConnection::LoadStats(%client)
{
	echo("Loading stats for" SPC %client.getPlayerName());

    %in = new FileObject();
    %success = %in.openForRead("config/server/BBB/stats/" @ %client.getBLID() @ ".txt");


    if(!%success)
    {
		echo("Failed to open file");
		%in.close();
		%in.delete();
        return false;
    }

    while(!%in.isEOF())
    {
		%line = %in.readLine();
        %field = getField(%line,0);
        %value = getField(%line,1);

        %client.setObjField(%field,%value);
    }

	//this client may have had their stats loaded into a global
	//clear the old one
	$Stats::LoadedStats[%client.getBLID()] = "";

	%in.close();
	%in.delete();
    return true;
}

function simObject::getObjField(%this, %fieldName)
{
	%firstLetter = getSubStr(%fieldName, 0, 1);
	%restLetters = getSubStr(%fieldName, 1, 256);

	switch$(%firstLetter)
	{
		case "a": return %this.a[%restLetters];
		case "b": return %this.b[%restLetters];
		case "c": return %this.c[%restLetters];
		case "d": return %this.d[%restLetters];
		case "e": return %this.e[%restLetters];
		case "f": return %this.f[%restLetters];
		case "g": return %this.g[%restLetters];
		case "h": return %this.h[%restLetters];
		case "i": return %this.i[%restLetters];
		case "j": return %this.j[%restLetters];
		case "k": return %this.k[%restLetters];
		case "l": return %this.l[%restLetters];
		case "m": return %this.m[%restLetters];
		case "n": return %this.n[%restLetters];
		case "o": return %this.o[%restLetters];
		case "p": return %this.p[%restLetters];
		case "q": return %this.q[%restLetters];
		case "r": return %this.r[%restLetters];
		case "s": return %this.s[%restLetters];
		case "t": return %this.t[%restLetters];
		case "u": return %this.u[%restLetters];
		case "v": return %this.v[%restLetters];
		case "w": return %this.w[%restLetters];
		case "x": return %this.x[%restLetters];
		case "y": return %this.y[%restLetters];
		case "z": return %this.z[%restLetters];
		case "_": return %this._[%restLetters];
	}
}

function simObject::setObjField(%this, %fieldName, %newValue)
{
	%firstLetter = getSubStr(%fieldName, 0, 1);
	%restLetters = getSubStr(%fieldName, 1, 256);

	switch$(%firstLetter)
	{
		case "a": %this.a[%restLetters] = %newValue;
		case "b": %this.b[%restLetters] = %newValue;
		case "c": %this.c[%restLetters] = %newValue;
		case "d": %this.d[%restLetters] = %newValue;
		case "e": %this.e[%restLetters] = %newValue;
		case "f": %this.f[%restLetters] = %newValue;
		case "g": %this.g[%restLetters] = %newValue;
		case "h": %this.h[%restLetters] = %newValue;
		case "i": %this.i[%restLetters] = %newValue;
		case "j": %this.j[%restLetters] = %newValue;
		case "k": %this.k[%restLetters] = %newValue;
		case "l": %this.l[%restLetters] = %newValue;
		case "m": %this.m[%restLetters] = %newValue;
		case "n": %this.n[%restLetters] = %newValue;
		case "o": %this.o[%restLetters] = %newValue;
		case "p": %this.p[%restLetters] = %newValue;
		case "q": %this.q[%restLetters] = %newValue;
		case "r": %this.r[%restLetters] = %newValue;
		case "s": %this.s[%restLetters] = %newValue;
		case "t": %this.t[%restLetters] = %newValue;
		case "u": %this.u[%restLetters] = %newValue;
		case "v": %this.v[%restLetters] = %newValue;
		case "w": %this.w[%restLetters] = %newValue;
		case "x": %this.x[%restLetters] = %newValue;
		case "y": %this.y[%restLetters] = %newValue;
		case "z": %this.z[%restLetters] = %newValue;
		case "_": %this._[%restLetters] = %newValue;
	}
}

//general stats page
//total rounds played
//total rounds won
//total kills
//miskills
//favorite gun
//favorite map
function serverCmdStats(%client,%name,%stat)
{
	if(%name $= "" || %name $= "mine")
	{
		%searchedClient = %client;
	}
	//is this client on the server?
	//we want to search by exact names so we need to make a function
	%group = ClientGroup;
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%currClient =  %group.getObject(%i);
		%currName = %currClient.getPlayerName();

		if(%currName $= %name)
		{
			%searchedClient = %currClient;
		}
	}

	if(%searchedClient $= "")
	{
		//try opening their save file by fetching a blid from our table
		%BLID = $Stats::NameToBLID[getSafeVariableName(%name)];
		talk(%BLID);
		//have we already loaded this dude?
		if(isObject($Stats::LoadedStats[%BLID]))
		{
			%searchedClient = $Stats::LoadedStats[%BLID];
		}
		else if(%BLID !$= "")
		{
			//load a fake client with stats
			%searchedClient = new GameConnection()
			{
				bl_id = %BLID;
				name = %name;
			};

			%searchedClient.loadStats();
		}
	}

	if(%searchedClient $= "")
	{
		%client.chatMessage("\c5Player couldn't be found. Please type in the full name.");
		return;
	}

	//we want to load a general page if nothing is inputted into the stat line
	if(%stat $= "")
	{
		%client.chatMessage("\c2" @ %searchedClient.getPlayerName() @ "'s stats:");
		%c = -1;
		%stat[%c++] = %searchedClient.stat_RoundsPlayed;
		%stat[%c++] = %searchedClient.stat_RoundsWon;
		%stat[%c++] = %searchedClient.stat_TotalKills;
		%stat[%c++] = %searchedClient.stat_Miskills;
		%stat[%c++] = %searchedClient.stat_FavoriteGun $= "None" ? "None" : %searchedClient.stat_FavoriteGun.uiName;
		%stat[%c++] = %searchedClient.stat_FavoriteMap;
		%c++;
		//make the numbers all the same size strings
		%largest = 0;
		for(%i = 0; %i < %c; %i++)
		{
			%len = strLen(%stat[%i]);
			if(%len > %largest)
			{
				%largest = %len;
			}
		}

		for(%i = 0; %i < %c; %i++)
		{
			while(strLen(%stat[%i]) < %largest)
			{
				%stat[%i] = " " @ %stat[%i];
			}
		}

		%c = -1;
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Rounds Played");
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Rounds Won");
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Kills");
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Miskills");
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Favorite Gun");
		%client.chatMessage("<font:consolas:20>\c6" @ %stat[%c++] @ " | Favorite Map");
	}
	else
	{
		//search for a specific stat by name or number and display it
		//if not a valid stat display all availible stats by name and number
		%found = -1;
		if(%stat $= "0" || %stat > 0)
		{
			%field = $Stats::StatName[%stat];
			%name = getField(%field,0);
			%value = getField(%field,1);

			%value = %searchedClient.getObjField(%value);
			if(%value.uiName !$= "")
			{
				%value = %value.uiName;
			}
			%stat[%found++] = %value TAB %name;
		}
		else
		{
			%c = 0;
			while((%field = $Stats::StatName[%c]) !$= "")
			{
				%name = getField(%field,0);
				%value = getField(%field,1);
				if(striPos(%name,%stat) == 0)
				{
					%value = %searchedClient.getObjField(%value);
					if(%value.uiName !$= "")
					{
						%value = %value.uiName;
					}

					%stat[%found++] = %value TAB %name;
				}
				%c++;
			}
		}
		%found++;

		if(%found > 0)
		{
			%largest = 0;
			for(%i = 0; %i < %found; %i++)
			{
				%len = strLen(getField(%stat[%i],0));
				if(%len > %largest)
				{
					%largest = %len;
				}
			}

			for(%i = 0; %i < %found; %i++)
			{
				%value = getField(%stat[%i],0);
				while(strLen(%value) < %largest)
				{
					%value = " " @ %value;
				}
				%stat[%i] = setField(%stat[%i],0,%value);
			}

			%client.chatMessage("\c2" @ %searchedClient.getPlayerName() @ "'s stats:");
			for(%i = 0; %i < %found; %i++)
			{
				%field = %stat[%i];
				%value = getField(%field,0);
				%name = getField(%field,1);

				%client.chatMessage("<font:consolas:20>\c6" @ %value @ " | " @ %name);
			}
		}
		else
		{
			%client.chatMessage("\c5Uknown stat. Try one of these:");
			%c = 0;
			while((%field = $Stats::StatName[%c]) !$= "")
			{
				%name = getField(%field,0);

				%client.chatMessage("<font:consolas:20>\c6" @ (strLen(%c) <= 1 ? " " @ %c : %c)  @ " | " @ %name);
				%c++;
			}
		}
	}
}

$c = -1;
//#kill stats (DONE)
//traitors killed
registerSavedField("stat_TraitorsKilled",0);
$Stats::StatName[$c++] = "Traitors Killed" TAB "stat_TraitorsKilled";
//innocents killed
registerSavedField("stat_InnocentsKilled",0);
$Stats::StatName[$c++] = "Innocents Killed" TAB "stat_InnocentsKilled";
//detectives killed
registerSavedField("stat_DetectivesKilled",0);
$Stats::StatName[$c++] = "Detectives Killed" TAB "stat_DetectivesKilled";
//total kills
registerSavedField("stat_TotalKills",0);
$Stats::StatName[$c++] = "Kills" TAB "stat_TotalKills";
//miskills
registerSavedField("stat_Miskills",0);
$Stats::StatName[$c++] = "Miskills" TAB "stat_Miskills";
//post round kills
registerSavedField("stat_PostRoundKills",0);
$Stats::StatName[$c++] = "Post Rounds Kills" TAB "stat_PostRoundKills";

//#round stats (DONE)
//total rounds played
registerSavedField("stat_RoundsPlayed",0);
$Stats::StatName[$c++] = "Rounds Played" TAB "stat_RoundsPlayed";
//traitor rounds played
registerSavedField("stat_TraitorRounds",0);
$Stats::StatName[$c++] = "Traitor Rounds" TAB "stat_TraitorRounds";
//innocent rounds played
registerSavedField("stat_InnocentRounds",0);
$Stats::StatName[$c++] = "Innocent Rounds" TAB "stat_InnocentRounds";
//detective rounds played
registerSavedField("stat_DetectiveRounds",0);
$Stats::StatName[$c++] = "Detective Rounds" TAB "stat_DetectiveRounds";
//total rounds won
registerSavedField("stat_RoundsWon",0);
$Stats::StatName[$c++] = "Wins" TAB "stat_TraitorsKilled";
//traitor wins
registerSavedField("stat_TraitorWins",0);
$Stats::StatName[$c++] = "Traitor Wins" TAB "stat_TraitorWins";
//innocent wins
registerSavedField("stat_InnocentWins",0);
$Stats::StatName[$c++] = "Innocent Wins" TAB "stat_InnocentWins";
//detective wins
registerSavedField("stat_DetectiveWins",0);
$Stats::StatName[$c++] = "Detective Wins" TAB "stat_DetectiveWins";
//rounds survived
registerSavedField("stat_RoundsSurvived",0);
$Stats::StatName[$c++] = "Rounds Survived" TAB "stat_RoundsSurvived";

//#favorites stats (need to save a lot of stats for these)
//favorite traitor item
registerSavedField("stat_FavoriteTraitorItem","None");
$Stats::StatName[$c++] = "Favorite Traitor Item" TAB "stat_FavoriteTraitorItem";
registerSavedArray("stat_FavoriteTraitorArray","");
//favorite detective item
registerSavedField("stat_FavoriteDetectiveItem","None");
$Stats::StatName[$c++] = "Favorite Detective Item" TAB "stat_FavoriteDetectiveItem";
registerSavedArray("stat_FavoriteDetectiveArray","");
//favorite gun
registerSavedField("stat_FavoriteGun","None");
$Stats::StatName[$c++] = "Favorite Gun" TAB "stat_FavoriteGun";
registerSavedArray("stat_FavoriteGunArray","");
//favorite target
registerSavedField("stat_FavoriteTarget","None");
$Stats::StatName[$c++] = "Favorite Target" TAB "stat_FavoriteTarget";
registerSavedArray("stat_FavoriteTargetArray","");
//favorite map
registerSavedField("stat_FavoriteMap","None");
$Stats::StatName[$c++] = "Favorite Map" TAB "stat_FavoriteMap";
registerSavedArray("stat_FavoriteMapArray","");

//challenge stats
//melee kills
//throwing knife kills

//#generic stats
//credits spent
//items dropped
//bullets fired
//bullets hit
//grenades thrown
//jumps
//bodies discovered
//times flashbanged
//times laser pointed
//times exploded
package StatSaver
{
	function GameConnection::onClientEnterGame(%client)
	{
		$Stats::NameToBLID[getSafeVariableName(%client.getPlayerName())] = %client.getBLID();
		exec("config/server/BBB/NameToId.cs");
		export("$Stats::NameToBLID*","config/server/BBB/NameToId.cs");

		%client.loadStats();
		%client.saveStats();
		%client.loadStats();
		return Parent::onClientEnterGame(%client);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if(%sourceClient.getClassName() $= "GameConnection" && %sourceClient.inBBB && %client.inBBB)
		{
			if($BBB::Round::Phase $= "Round")
			{
				%sourceClient.stat_TotalKills++;

				%role = %client.role;
				%myRole = %sourceClient.role;

				if(%role $= %myRole)
				{
					%sourceClient.stat_Miskills++;
				}
				else if(%role $= "Innocent" && %myRole $= "Detective")
				{
					%sourceClient.stat_Miskills++;
				}
				else if(%role $= "Detective" && %myRole $= "Innocent")
				{
					%sourceClient.stat_Miskills++;
				}

				%sourceClient.stat_[%role @ "sKilled"]++;
			}
			else
			{
				%sourceClient.stat_PostRoundKills++;
			}
			%name = %client.getPlayerName();

			//add this to the array
			%c = 0;
			while((%value = %sourceclient.stat_FavoriteTargetArray[%c]) !$= "")
			{
				%currtarget = getField(%value,0);
				if(%currtarget $= %name)
				{
					%ammount = getField(%value,1);
					break;
				}
				%c++;
			}
			if(%value $= "")
			{
				%currtarget = %name;
				%ammount = 0;
			}

			%sourceclient.stat_FavoriteTargetArray[%c] = %currtarget TAB %ammount + 1;

			//recalculate favorite
			%c = 0;
			%mosttarget = "";
			%mostammount = 0;
			while((%value = %sourceclient.stat_FavoriteTargetArray[%c]) !$= "")
			{
				%currtarget = getField(%value,0);
				%ammount = getField(%value,1);
				if(%ammount > %mostammount)
				{
					%mosttarget = %currtarget;
					%mostAmmount = %ammount;
				}
				%c++;
			}

			if(%mosttarget !$= "")
			{
				%sourceclient.stat_FavoriteTarget = %mosttarget;
			}
		}

		return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function BBB_Minigame::roundEnd(%so, %type)
	{
		//loop through the minigame members and increment rounds played
		%count = %so.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%client  = %so.member[%i];

			%client.stat_RoundsPlayed++;
			%role = %client.role;

			//did they win?
			%win = false;
			if(%type $= "IWin" && (%role $= "Innocent" || %role $= "Detective"))
			{
				%win = true;
			}
			else if(%type $= "TWin" && %role $= "Traitor")
			{
				%win = true;
			}

			//increment rounds played for their role
			%client.stat_[%role @ "Rounds"]++;
			if(%win)
			{
				%client.stat_[%role @ "Wins"]++;
				%client.stat_RoundsWon++;
			}
		}

		return Parent::roundEnd(%so, %type);
	}

	function BBB_Minigame::roundSetup(%so)
	{
		//did anyone survive the entire round counting post round
		//also initiate schedules to save player data
		%count = %so.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%client  = %so.member[%i];
			%player = %client.player;

			//are they alive?
			if(isObject(%player))
			{
				%client.stat_RoundsSurvived++;
			}

			//save player data
			%client.schedule(100,"SaveStats");
		}

		return Parent::roundSetup(%so);
	}

	function serverCmdBuy(%client, %search)
	{
		parent::serverCmdBuy(%client, %search);
		%image = %client.player.lastBoughtItem.getName();
		%role = %client.role;

		if(%image $= "")
		{
			return;
		}

		//add this to the array
		%c = 0;
		while((%value = %client.stat_Favorite[%role @ "Array" @ %c]) !$= "")
		{
			%currImage = getField(%value,0);
			if(%currImage $= %image)
			{
				%ammount = getField(%value,1);
				break;
			}
			%c++;
		}
		if(%value $= "")
		{
			%currImage = %image;
			%ammount = 0;
		}

		%client.stat_Favorite[%role @ "Array" @ %c] = %currImage TAB %ammount + 1;

		//recalculate favorite
		%c = 0;
		%mostimage = "";
		%mostammount = 0;
		while((%value = %client.stat_Favorite[%role @ "Array" @ %c]) !$= "")
		{
			%currImage = getField(%value,0);
			%ammount = getField(%value,1);
			if(%ammount > %mostammount)
			{
				%mostImage = %currImage;
				%mostAmmount = %ammount;
			}
			%c++;
		}

		if(%mostimage !$= "")
		{
			%client.stat_Favorite[%role @ "Item"] = %mostImage;
		}
	}

	function Player::BBB_GiveItem(%obj, %itemToGive)
	{
		%client = %obj.client;

		// Are we even trying?
		if(!isObject(%client) || !isObject(%obj) || !isObject(%itemToGive))
			return 0;

		%itemName = %itemToGive.getDatablock().getName();

		// Get slot
		%slot = "";
		%found = false;
		for(%a = 0; %a <= 1; %a++)
		{
			switch(%a)
			{
				case 0:
					%name = "Primary";
				case 1:
					%name = "Secondary";
			}

			%fieldCount = getFieldCount($BBB::Weapons_[%name]);
			for(%b = 0; %b < %fieldCount; %b++)
			{
				%field = getField($BBB::Weapons_[%name], %b);
				if(%field $= %itemName)
				{
					%found = true;
					%slot = %a;
					break;
				}

			}
			if(%found)
				break;
		}

		if(%found)
		{
			%thing = %itemName;
			//add this to the array
			%c = 0;
			while((%value = %client.stat_FavoriteGunArray[%c]) !$= "")
			{
				%curr = getField(%value,0);
				if(%curr $= %thing)
				{
					%ammount = getField(%value,1);
					break;
				}
				%c++;
			}
			if(%value $= "")
			{
				%curr = %thing;
				%ammount = 0;
			}

			%client.stat_FavoriteGunArray[%c] = %curr TAB %ammount + 1;

			//recalculate favorite
			%c = 0;
			%mostthing = "";
			%mostammount = 0;
			while((%value = %client.stat_FavoriteGunArray[%c]) !$= "")
			{
				%curr = getField(%value,0);
				%ammount = getField(%value,1);
				if(%ammount > %mostammount)
				{
					%mostthing = %curr;
					%mostAmmount = %ammount;
				}
				%c++;
			}

			if(%mostthing !$= "")
			{
				%client.stat_FavoriteGun = %mostthing;
			}
		}

		return parent::BBB_GiveItem(%obj, %itemToGive);
	}

	function serverCmdVoteMap(%client, %w0, %w1, %w2, %w3, %w4, %w5, %w6, %w8, %w9, %w10)
	{
		parent::serverCmdVoteMap(%client, %w0, %w1, %w2, %w3, %w4, %w5, %w6, %w8, %w9, %w10);
		if($BBB::Round::Phase !$= "MapVote")
			return;

		if(strstr($BBB::Vote::Data, "|" @ %client.getBLID() @ "|") > -1)
		{
			%thing = %client.lastvotedmap;
			//add this to the array
			%c = 0;
			while((%value = %client.stat_FavoriteMapArray[%c]) !$= "")
			{
				%curr = getField(%value,0);
				if(%curr $= %thing)
				{
					%ammount = getField(%value,1);
					break;
				}
				%c++;
			}
			if(%value $= "")
			{
				%curr = %thing;
				%ammount = 0;
			}

			%client.stat_FavoriteMapArray[%c] = %curr TAB %ammount + 1;

			//recalculate favorite
			%c = 0;
			%mostthing = "";
			%mostammount = 0;
			while((%value = %client.stat_FavoriteMapArray[%c]) !$= "")
			{
				%curr = getField(%value,0);
				%ammount = getField(%value,1);
				if(%ammount > %mostammount)
				{
					%mostthing = %curr;
					%mostAmmount = %ammount;
				}
				%c++;
			}

			if(%mostthing !$= "")
			{
				%client.stat_FavoriteMap = %mostthing;
			}
		}
	}
};
deactivatePackage("StatSaver");
activatePackage("StatSaver");