if(!isObject($NameLists))
{
    $NameLists = new ScriptGroup();
}

function NameList_Find(%name)
{
    return $NameLists.name[%name];
}

function NameList_Create(%name,%list)
{
    new ScriptObject()
    {
        class = "NameList";
        list = %list;
        name = %name;
    };
}

function NameList::OnAdd(%obj)
{
    if(isObject($NameLists.name[%obj.name]))
    {
        $NameLists.name[%obj.name].delete();
    }

    $NameLists.name[%obj.name] = %obj;
    $NameLists.add(%obj);
}

function NameList::OnRemove(%obj)
{
    $NameLists.name[%obj.name] = "";
    $NameLists.remove(%obj);
}

function NameList::SetNames(%obj,%clients)
{
    %list = %obj.list;
    %listCount = getFieldCount(%list);
    %count = getWordCount(%clients);
    for(%i = 0; %i < %count; %i++)
    {
        %client = getWord(%clients,%i);
        %r = getRandom(0,(%listCount-%i)-1);
        %name = getWord(%list,%r);
		%name = %client.getPlayerName();
        %client.fakeName = %client.getPlayerName();
		%client.player.setShapeName(%name, 8564862);
		%client.player.displayName = %name;
        %client.state = "";
        %list = removeWord(%list,%r);
        %names = %names TAB %name;
    }
    %names = lTrim(%names);
	$NameListRandomSeed = getRandom();
    NameList_Update();
}

function secureCommandToAllTS (%code, %command, %a1, %a2,%a3, %a4, %a5, %a6,%a7)
{
	%group = ClientGroup;
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);

		secureCommandToClient(%code,%client,%command,%a1,%a2,%a3,%a4,%a5,%a6,%a7);
	}
}

function NameList_Update(%reveal) //update all client names on the player list
{
	%oldSeed = getRandomSeed();
	setRandomSeed($NameListRandomSeed);
    %group = ClientGroup.getId();
    %count = %group.getCount();

	if($NameListCurrentCount > %count)
	{
		for(%i = $NameListCurrentCount-1; %i >= %count; %i--)
		{
			secureCommandToAllTS("zbR4HmJcSY8hdRhr" ,'ClientDrop', %name, %i);
		}
	}

    %indexes = "";
    for(%i = 0; %i < %count; %i++)
    {
        %indexes = %indexes SPC %i;
    }
    %indexes = lTrim(%indexes);

    for(%i = 0; %i < %count; %i++)
    {
        %r = getRandom(0,(%count-%i)-1);
        %index = getWord(%indexes,%r); 
        %indexes = removeWord(%indexes,%r);
        %client = %group.getObject(%i);
        %name = %client.getPlayerName();
		%state = "O";

		if(!%client.hasspawnedonce)
		{
			continue;
		}
		
        if(%client.fakeName !$= "")
        {
            %name = %client.fakeName;
        }
        if(%reveal && %name !$= %client.getPlayerName())
        {
            %name = %client.getPlayerName() SPC "("@%name@")";
        }
        if(%client.state !$= "")
        {
            %state = %client.state;
        }

        for(%j = 0; %j < %count; %j++)
        {
            %currClient = %group.getObject(%j);
            %badge = "";
            if(%client.badge[%currClient] !$= "")
            {
                %badge = %client.badge[%currClient]; 
            }
			if(%state $= "X" || %reveal)
			{
				%badge = %client.revealBadge;
			}
            secureCommandToClient("zbR4HmJcSY8hdRhr", %currClient ,'ClientJoin', %name, %index, %client.getBLID(), trim(%state SPC %badge), 0, %reveal && %client.isAdmin, %reveal && %client.isSuperAdmin);
        }
    }
    $NameListCurrentCount = %count;
	setRandomSeed(%oldSeed);
}

function GameConnection::startLoad(%client) //remove auto adding clients to the playerlist
{
	commandToClient(%client, 'updatePrefs');
	if (%client.getAddress() $= "local")
	{
		%client.isAdmin = 1;
		%client.isSuperAdmin = 1;
	}
	else
	{
		%client.isAdmin = 0;
		%client.isSuperAdmin = 0;
	}
	%client.score = 0;
	$instantGroup = ServerGroup;
	$instantGroup = MissionCleanup;
	echo("CADD: " @ %client @ " " @ %client.getAddress());
	echo(" +- bl_id = ", %client.getBLID());
	%autoAdmin = %client.autoAdminCheck();
	// %count = ClientGroup.getCount();
	// for (%cl = 0; %cl < %count; %cl++)
	// {
	// 	%other = ClientGroup.getObject(%cl);
	// 	if (%other != %client)
	// 	{
	// 		secureCommandToClient("zbR4HmJcSY8hdRhr", %client, 'ClientJoin', %other.getPlayerName(), %other, %other.getBLID(), %other.score, %other.isAIControlled(), %other.isAdmin, %other.isSuperAdmin);
	// 	}
	// }
	commandToClient(%client, 'NewPlayerListGui_UpdateWindowTitle', $Server::Name, $Pref::Server::MaxPlayers);
	serverCmdRequestMiniGameList(%client);
	$Server::WelcomeMessage = strreplace($Server::WelcomeMessage, ";", "");
	$Server::WelcomeMessage = strreplace($Server::WelcomeMessage, "\\'", "'");
	$Server::WelcomeMessage = strreplace($Server::WelcomeMessage, "'", "\\'");
	eval("%taggedMessage = '" @ $Server::WelcomeMessage @ "';");
	messageClient(%client, '', %taggedMessage, %client.getPlayerName());
	messageAllExcept(%client, -1, 'MsgClientJoin', '\c1%1 connected.', %client.getPlayerName());
	// secureCommandToAll("zbR4HmJcSY8hdRhr", 'ClientJoin', %client.getPlayerName(), %client, %client.getBLID(), %client.score, %client.isAIControlled(), %client.isAdmin, %client.isSuperAdmin);
    if (%autoAdmin == 0)
	{
		echo(" +- no auto admin");
	}
	else if (%autoAdmin == 1)
	{
		MessageAll('MsgAdminForce', '\c2%1 has become Admin (Auto)', %client.getPlayerName());
		echo(" +- AUTO ADMIN");
	}
	else if (%autoAdmin == 2)
	{
		MessageAll('MsgAdminForce', '\c2%1 has become Super Admin (Auto)', %client.getPlayerName());
		echo(" +- AUTO SUPER ADMIN (List)");
	}
	else if (%autoAdmin == 3)
	{
		MessageAll('MsgAdminForce', '\c2%1 has become Super Admin (Host)', %client.getPlayerName());
		echo(" +- AUTO SUPER ADMIN (ID same as host)");
	}
	if (%client.getBLID() <= -1)
	{
		error("ERROR: GameConnection::startLoad() - Client has no bl_id");
		%client.schedule(10, delete);
		return;
	}
	else if (isObject("BrickGroup_" @ %client.getBLID()))
	{
		%obj = "BrickGroup_" @ %client.getBLID();
		%client.brickGroup = %obj.getId();
		%client.brickGroup.isPublicDomain = 0;
		%client.brickGroup.abandonedTime = 0;
		%client.brickGroup.name = %client.getPlayerName();
		%client.brickGroup.client = %client;
		%quotaObject = %client.brickGroup.QuotaObject;
		if (isObject(%quotaObject))
		{
			if (isEventPending(%quotaObject.cancelEventsEvent))
			{
				cancel(%quotaObject.cancelEventsEvent);
			}
			if (isEventPending(%quotaObject.cancelProjectilesEvent))
			{
				cancel(%quotaObject.cancelProjectilesEvent);
			}
		}
	}
	else
	{
		%client.brickGroup = new SimGroup("BrickGroup_" @ %client.getBLID());
		mainBrickGroup.add(%client.brickGroup);
		%client.brickGroup.client = %client;
		%client.brickGroup.name = %client.getPlayerName();
		%client.brickGroup.bl_id = %client.getBLID();
	}
	%client.InitializeTrustListUpload();
	if ($missionRunning)
	{
		%client.loadMission();
	}
	if ($Server::PlayerCount >= $Pref::Server::MaxPlayers || getSimTime() - $Server::lastPostTime > 30 * 1000 || $Server::lastPostTime < 30 * 1000)
	{
		WebCom_PostServer();
	}
}

function GameConnection::setScore(%client, %val)
{
	%client.score = %val;
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (!%cl.playerListOpen)
		{
		}
		else
		{
			// secureCommandToClient("zbR4HmJcSY8hdRhr", %cl, 'ClientScoreChanged', mFloor(%client.score), %client);
		}
	}
}

function serverCmdOpenPlayerList(%client)
{
	%client.playerListOpen = 1;
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		// secureCommandToClient("zbR4HmJcSY8hdRhr", %client, 'ClientScoreChanged', mFloor(%cl.score), %cl);
	}
}

NameList_Create("Colors",
"White"TAB
"Gray"TAB
"Silver"TAB
"Gold"TAB
"Red"TAB
"Blue"TAB
"Beige"TAB
"Orange"TAB
"Green"TAB
"Azure"TAB
"Opal"TAB
"Olive"TAB
"Salmon"TAB
"Ochre"TAB
"Pink"TAB
"Cyan"TAB
"Indigo"TAB
"Teal"TAB
"Violet"TAB
"Maroon"TAB
"Purple"TAB
"Jade"TAB
"Lime"TAB
"Ruby"TAB
"Rose"TAB
"Amber"TAB
"Brown"TAB
"Hazel"TAB
"Drab"TAB
"Lilac"TAB
"Cobalt"TAB
"Jet");