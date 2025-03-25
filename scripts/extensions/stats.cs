$Stats::StatFiles = "config/server/BBB/stats/";

function registerSavedField(%name,%initial)
{
	%name = getSafeVariableName(%name);
    %count = $Stats::RegisteredFields + 0;
    for(%i = 0; %i < %count; %i++)
    {
        if($Stats::Field[0] $= %name)
        {
            return false;
        }
    }

    $Stats::Field[%count] = %name;
	$Stats::FieldInit[%count] = %initial;
    $Stats::RegisteredFields++;

    return true;
}

function registerSavedArray(%name,%initial)
{
	%name = getSafeVariableName(%name);
    %count = $Stats::RegisteredArrays + 0;
    for(%i = 0; %i < %count; %i++)
    {
        if($Stats::Array[0] $= %name)
        {
            return false;
        }
    }

    $Stats::Array[%count] = %name;
	$Stats::ArrayInit[%count] = %initial;
    $Stats::RegisteredArrays++;

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

function registerStat(%statField,%fieldInit,%array,%arrayInit)
{
	if($Stats::StatName[%statField] !$= "")
	{
		return;
	}

	$Stats::StatName[%statField] = $Stats::StatCount + 0;
	$Stats::Stat[$Stats::StatCount + 0] = %statField;
	$Stats::StatCount++;

	registerSavedField("stat_" @ %statField,%fieldInit);
	if(%array !$= "")
	{
		registerSavedArray("stat_" @ %statField SPC "Array",%arrayInit);
	}	
}

function GameConnection::SetStat(%client,%field,%value)
{
	%client.stat_[getSafeVariableName(%field)] = %value;
	return %client;
}

function GameConnection::GetStat(%client,%field)
{
	return %client.stat_[getSafeVariableName(%field)];
}

function GameConnection::AddStat(%client,%field,%value)
{
	%client.setStat(%field,%client.getStat(%field) + %value);
	return %client;
}

function GameConnection::AddStatArray(%client,%stat,%key,%value)
{
	%array = getSafeVariableName(%stat SPC "Array");
	%c = 0;
	while((%field = %client.stat_[%array @ %c]) !$= "")
	{
		%curr = getField(%field,0);
		if(%curr $= %key)
		{
			%amount = getField(%field,1);
			break;
		}
		%c++;
	}
	if(%field $= "")
	{
		%curr = %key;
		%amount = 0;
	}
	%client.stat_[%array @ %c] = %curr TAB %amount + %value;

	%c = 0;
	%mostKey = "";
	%mostamount = 0;
	while((%field = %client.stat_[%array @ %c]) !$= "")
	{
		%curr = getField(%field,0);
		%amount = getField(%field,1);
		if(%amount >= %mostamount)
		{
			%mostKey = %curr;
			%mostamount = %amount;
		}
		%c++;
	}

	if(%mostKey !$= "")
	{
		%client.stat_[getSafeVariableName(%stat)] = %mostKey;
	}
}

function GameConnection::SaveStats(%client)
{
	echo("Saving stats for" SPC %client.name);

    %out = new FileObject();
    %success = %out.openForWrite($Stats::StatFiles @ %client.bl_id @ ".txt");

    if(!%success)
    {
		echo("Failed to open file retying...");
		%out.close();
		%out.delete();
		%client.schedule(100,"saveStats");
        return false;
    }

    %count = $Stats::RegisteredFields;
    for(%i = 0; %i < %count; %i++)
    {
        %field = $Stats::Field[%i];

		%value = %client.getObjField(%field);
		if(%value $= "")
		{
			%value = $Stats::FieldInit[%i];
		}
        
        %out.writeLine(%field TAB %value);
    }

	%count = $Stats::RegisteredArrays;
    for(%i = 0; %i < %count; %i++)
    {
        %array = $Stats::Array[%i];
		%c = 0;
		while((%value = %client.getObjField(%array @ %c)) !$= "")
		{
			if(%value $= "")
			{
				%value = $Stats::ArrayInit[%i];
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
	echo("Loading stats for" SPC %client.name);

    %in = new FileObject();
    %success = %in.openForRead($Stats::StatFiles @ %client.bl_id @ ".txt");

	//talk(isFile($Stats::StatFiles @ %client.bl_id @ ".txt"));
    if(!%success)
    {
		echo("Failed to open file retrying...");
		%in.close();
		%in.delete();
		%client.schedule(100,"loadStats");
        return false;
    }

    while(!%in.isEOF())
    {
		%line = %in.readLine();
        %field = getField(%line,0);
        %value = getFields(%line,1);

        %client.setObjField(%field,%value);
    }

	%in.close();
	%in.delete();
    return true;
}

//usage: [name or BLID blid or me] [stat]
$c = -1;
//general stats page
//total rounds played
$Stats::GeneralStatsPage[$c++] = "Rounds";
//total rounds won
$Stats::GeneralStatsPage[$c++] = "Wins";
//total kills
$Stats::GeneralStatsPage[$c++] = "Kills";
//miskills
$Stats::GeneralStatsPage[$c++] = "Miskills";
//favorite gun
$Stats::GeneralStatsPage[$c++] = "Gun";
//favorite map
$Stats::GeneralStatsPage[$c++] = "Map";

function serverCmdStats(%client,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15)
{	
	if(%a0 $= "")
	{
		%client.chatMessage("\c5Welcome to stats. \c3[full player name, \"BLID\" blid, \"me\", or \"stats\"] \c4[stat]");
		return;
	}

	if(%a0 $= "STATS")
	{
		%c = 0;
		while((%name = $Stats::Stat[%c]) !$= "")
		{
			%c++;
			%client.chatMessage("<font:consolas:20>\c6" @ (strLen(%c) <= 1 ? " " @ %c : %c) @ " | " @ %name);
			
		}
		return;
	}
	else if(%a0 $= "ME")
	{
		%blid = %client.bl_id;
		%stat = %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5;
	}
	else if(%a0 $= "BLID")
	{
		//we are using a blid search ez
		%blid = %a1;
		%stat = %a2 SPC %a3 SPC %a4 SPC %a5 SPC %a6;
	}
	else
	{
		//go until you find a valid name
		%c = 0;
		%name = %a0;
		while((%blid = $Stats::NameToBLID[getSafeVariableName(%name)]) $= "" && %c < 16)
		{
			%c++;
			%name = %name SPC %a[%c];
		}

		//stat string out of remaining
		%stat = %a[%c++] SPC %a[%c++] SPC %a[%c++] SPC %a[%c++] SPC %a[%c++];
	}

	%stat = trim(%stat);

	//does this blid have stats?
	%searchedClient = $Stats::BLIDToStats[%blid];
	if(!isObject(%searchedClient))
	{
		%client.chatMessage("\c5Uknown Player.");
		return;
	}

	//we want to load a general page if nothing is inputted into the stat line
	if(%stat $= "")
	{
		%c = 0;
		while((%name = $Stats::GeneralStatsPage[%c]) !$= "")
		{
			%value = %searchedClient.getStat(%name);
			if(%value.uiName !$= "" && %value == 0)
			{
				%value = %value.uiName;
			}
			%stat[%c] = %value TAB %name;
			%found++;
			%c++;
		}
	}
	else
	{
		//search for a specific stat by name or number and display it
		//if not a valid stat display all availible stats by name and number
		%found = -1;
		if(%stat > 0)
		{
			%name = $Stats::Stat[%stat - 1];

			%value = %searchedClient.getStat(%name);
			if(%value.uiName !$= "" && %value == 0)
			{
				%value = %value.uiName;
			}
			%stat[%found++] = %value TAB %name;
		}
		else
		{
			%c = 0;
			while((%name = $Stats::Stat[%c]) !$= "")
			{
				if(striPos(%name,%stat) == 0)
				{
					%value = %searchedClient.getStat(%name);
					if(%value.uiName !$= "" && %value == 0)
					{
						%value = %value.uiName;
					}

					%stat[%found++] = %value TAB %name;
				}
				%c++;
			}
		}
		%found++;
	}

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

		%client.chatMessage("\c2" @ %searchedClient.name @ "'s stats:");
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
		
		%client.chatMessage("\c5Uknown stat. Use \"/stats stats\" to see availible stats.");
	}
}

$c = -1;
//kill stats
// 1 stat for each roll showing how many times you have killed that role
//miskills
registerStat("Miskills",0);

//win stats
//1 stat for each win condition showing how many times you have won with it

//#favorites stats (need to save a lot of stats for these)
//favorite item from shop for each role
//favorite primary gun
//favorite secondary gun
//favorite melee
//favorite grenade
registerStat("Gun","None",true,"");
//favorite target
registerStat("Target","None",true,"");
//favorite map
registerStat("Map","None",true,"");

//challenge stats
//melee kills
registerStat("Melee Kills",0);
//throwing knife kills
registerStat("Throwing Knife Kills",0);

//#generic stats
//bullets fired
registerStat("Bullets Fired",0);
//bullets hit
registerStat("Bullets Hit",0);
//grenades thrown
registerStat("Grenades Thrown",0);


//load all player stats into global variables
//these global variables will be used for stat displays and leaderboards
//they are all updated at the end of every round
function loadAllStats()
{
	if($Stats::StatSet)
	{
		$Stats::StatSet.deleteall();
		$Stats::StatSet.delete();
	}
	$Stats::StatSet = new SimSet();

	%file = findFirstFile($Stats::StatFiles @ "*");
	while(%file !$= "")
	{
		%blid = fileBase(%file);
		%client = new GameConnection()
		{
			bl_id = %blid;
			name = $Stats::BLIDToName[%blid];
		};
		%client.schedule(100,"loadStats");

		$Stats::StatSet.add(%client);
		$Stats::BLIDToStats[%blid] = %client;
		%file = findNextFile($Stats::StatFiles @ "*");
	}
}

function SaveAllStats()
{
	%so = BBB_Minigame;
	%count = %so.numMembers;
	for(%i = 0; %i < %count; %i++)
	{
		%client  = %so.member[%i];
		%player = %client.player;

		//save player data
		%client.schedule(100,"SaveStatsAndUpdate");
	}
	LeaderboardGroup.schedule(1000,"Update");
}

function GameConnection::SaveStatsAndUpdate(%client)
{
	%client.saveStats();
	%blid = %client.bl_id;
	$Stats::BLIDToStats[%blid].loadStats();
}

package StatSaver
{
	function GameConnection::onClientEnterGame(%client)
	{
		$Stats::NameToBLID[getSafeVariableName(%client.name)] = %client.bl_id;
		$Stats::BLIDToName[%client.bl_id] = %client.name;

		if(!isObject($Stats::StatSet))
		{
			exec("config/server/BBB/NameToBLID.cs");
			exec("config/server/BBB/BLIDToName.cs");
			loadAllStats();
		}

		export("$Stats::NameToBLID*","config/server/BBB/NameToBLID.cs");
		export("$Stats::BLIDToName*","config/server/BBB/BLIDToName.cs");

		%client.loadStats();
		%client.saveStats();
		%blid = %client.bl_id;
		if($Stats::BLIDToStats[%blid] $= "")
		{
			%obj = new GameConnection()
			{
				bl_id = %blid;
				name = $Stats::BLIDToName[%blid];
			};
			%obj.loadStats();

			$Stats::StatSet.add(%obj);
			$Stats::BLIDToStats[%blid] = %obj;
		}
		
		return Parent::onClientEnterGame(%client);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if(%sourceClient.getClassName() $= "GameConnection" && %sourceClient.inBBB && %client.inBBB)
		{
			if($BBB::Round::Phase $= "Round")
			{
				%sourceClient.AddStat("Kills",1);

				%role = %client.role;
				%myRole = %sourceClient.role;

				if(%role $= %myRole)
				{
					%sourceClient.AddStat("Miskills",1);
				}
				else if(%role $= "Innocent" && %myRole $= "Detective")
				{
					%sourceClient.AddStat("Miskills",1);
				}
				else if(%role $= "Detective" && %myRole $= "Innocent")
				{
					%sourceClient.AddStat("Miskills",1);
				}

				%sourceClient.AddStat(GetSubStr(%role,0,1) SPC "Killed",1);
			}
			else
			{
				%sourceClient.AddStat("PR Kills",1);
			}

			%sourceClient.AddStatArray("Target",%client.name,1);
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

			%client.AddStat("Rounds",1);
			%role = %client.role;

			%client.AddStat(GetSubStr(%role,0,1) SPC "Rounds",1);

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
			if(%win)
			{
				%client.AddStat("Wins",1);
				%client.AddStat(GetSubStr(%role,0,1) SPC "Wins",1);
				%client.stat_RoundsWon++;
			}
		}

		return Parent::roundEnd(%so, %type);
	}

	function BBB_Minigame::roundSetup(%so)
	{
		//did anyone survive the entire round counting post round
		//also initiate schedules to save player data
		SaveAllStats();
		%count = %so.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%client  = %so.member[%i];
			%player = %client.player;

			//are they alive?
			if(isObject(%player))
			{
				%client.AddStat("Survived",1);
			}
		}

		return Parent::roundSetup(%so);
	}

	function BBB_CreditBuy(%client,%item,%price,%stock)
	{
		%success = parent::BBB_CreditBuy(%client,%item,%price,%stock);

		if(isObject(%item) && %success)
		{
			%itemName = %item.getName();
			%role = %client.role;

			if(%itemName!$= "")
			{
				%client.AddStatArray(getSubStr(%role,0,1) SPC "Item",%itemName,1);
			}
		}
		
		return %success;
	}

	function ItemData::onPickup (%this, %obj, %user, %amount)
	{
		%client = %user.client;

		%itemName = %this.getName();

		// Get slot
		%slot = %this.getSlot();

		if(%slot < 2)
		{
			%client.AddStatArray("Gun",%itemName,1);
		}
		
		return parent::onPickup (%this, %obj, %user, %amount);
	}

	function serverCmdVoteMap(%client, %w0, %w1, %w2, %w3, %w4, %w5, %w6, %w8, %w9, %w10)
	{
		parent::serverCmdVoteMap(%client, %w0, %w1, %w2, %w3, %w4, %w5, %w6, %w8, %w9, %w10);
		if($BBB::Round::Phase !$= "MapVote")
			return;

		if(strstr($BBB::Vote::Data, "|" @ %client.bl_id @ "|") > -1)
		{
			%thing = %client.lastvotedmap;
			%client.AddStatArray("Map",%thing,1);
		}
	}
};
deactivatePackage("StatSaver");
activatePackage("StatSaver");