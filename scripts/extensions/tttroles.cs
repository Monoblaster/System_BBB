function WinCondition_new(%name)
{
	if(isObject(%name))
	{
		%name.delete();
	}

	%new = new ScriptObject(%name)
	{
		superClass = WinCondition_Basic;
	};

	return %new;
}

function WinCondition_set(%client,%condition)
{
	if(%condition.superClass !$= "WinCondition_Basic")
	{
		warn("Non existant win condition");
		backtrace();
		return;
	}

	%client.winCondition = %condition.getId();
}

// is miskill
function WinCondition_Basic::isMiskill(%obj,%target)
{
	return %obj == %target;
}

// If everyone has the same win condition that win condition wins
function WinCondition_Basic::hasWon(%obj)
{
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(!isObject(%client.player) || %client.winCondition == %obj)
		{
			continue;
		}

		return false;
	}
	return true;
}

//helper for getting clients with the same win condition
function WinCondition_Basic::hasSameWinCondition(%obj)
{
	%clients = "";
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(%client.winCondition != %obj)
		{
			continue;
		}

		%clients = %clients SPC %client
	}
	return ltrim(%clients);
}

//helper for getting clients with a different win condition
function WinCondition_Basic::hasDifferentWinCondition(%obj)
{
	%clients = "";
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(%client.winCondition == %obj)
		{
			continue;
		}

		%clients = %clients SPC %client
	}
	return ltrim(%clients);
}


// is miskill
function WinCondition_Innocent::isMiskill(%obj,%target)
{
	return %obj == %target;
}

// If everyone has the same win condition that win condition wins
function WinCondition_Innocent::hasWon(%obj)
{
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(!isObject(%client.player) || %client.winCondition == %obj)
		{
			continue;
		}

		return false;
	}
	return true;
}

function Role_Create(%name,%shortname,%color,%components)
{
	%role = new ScriptObject()
	{
		class = "Role";
		
		name = %name;
		shortname = %shortname;
		color = %color;

		components = %components;
	};
	return %role;
}

function Role::Instance(%role)
{
	%holder = ComponentHolder_Create(%role.components);
	%holder.data = %role;
	return %holder;
}

if(!isObject($TTT::RoleGroup))
{
	$TTT::RoleGroup = new ScriptGroup()
	{
		class = "RoleGroup";
	};
}

function RoleGroup::Set(%obj,%role,%name)
{
	%obj._[%name] = %role;
	%obj.add(%role);
}

function RoleGroup::NameGet(%obj,%name)
{
	return %obj._[%name];
}

function RoleGroup::WithRole(%obj,%name)
{
	%group = ClientGroup.getId();
	%count = %group.getCount();
	%role = %obj._[%name].getId();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);
		if(%client.role != %role)
		{
			continue;
		}
		%clients = %clients SPC %client;
	}
	return lTrim(%clients);
}

function RoleGroup::WithoutRole(%obj,%name)
{
	%group = ClientGroup.getId();
	%count = %group.getCount();
	%role = %obj._[%name].getId();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);
		if(%client.role == %role)
		{
			continue;
		}
		%clients = %clients SPC %client;
	}
	return lTrim(%clients);
}

function Traitor_OnKill(%obj,%target,%deadclient)
{
	//check if credits are earned and add to the timer
	%mini = BBB_Minigame;
	%non = $TTT::ActiveRoleGroup.WithoutRole("Traitor");
	%count = getWordCount(%non);
	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%non,%i);
		if(isObject(%client.player))
		{
			continue;
		}
		%dead++;
	}
	%percentDead = %dead / %non;

	//is it reward time?
	if(%percentDead < %obj.data.creditDeadPercent * (%obj.gainedRewards + 1))
	{
		%obj.continue(%target,%deadclient);
		return;
	}

	//reward all traitors the amount
	%with = $TTT::ActiveRoleGroup.WithRole("Traitor");
	%count = getWordCount(%with);
	%gain = %obj.creditGain;
	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%with,%i);
		if(isObject(%currClient.player))
		{
			%client.chatMessage("\c6Well done. You have been awarded\c3" SPC %gain SPC "Credit\c6 for your hard work.");
			%client.credits += %gain;
		}
	}
	
	%obj.gainedRewards++;
	$BBB::rTimeLeft += %obj.hasteModeAdd;
}

//TODO: something smarter than globals
function TTT_CreateRoles()
{
	WinCondition_new("WinCondition_Innocent");
	WinCondition_new("WinCondition_Traitor");

	$TTT::RoleGroup.deleteAll();

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%role = Role_Create("Traitor","T","ff0000","WinCondition_Traitor",%component);
	%role.shop = "";
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = false;
	%role.hasteMode = true;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35
	%role.hasteModeAdd = 10000;
	$TTT::RoleGroup.set(%role,%role.name);

	%role = Role_Create("Innocent","I","00ff00","WinCondition_Innocent");
	%role.shop = "";
	%role.credits = 0;
	%role.rolechat = true;
	%role.rolebillboard = "";
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);

	%role = Role_Create("Detective","D","0000ff","WinCondition_Innocent");
	%role.shop = "";
	%role.credits = 2;
	%role.rolechat = true;
	%role.rolebillboard = "";
	%role.publicbillboard = detectiveBillboard;
	%role.startingItems = "BodyArmorItem DNAScannerItem";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);
	
	$TTT::DefaultRoleList = "Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective";
}
TTT_CreateRoles();

function TTT_PreRoleSetup()
{
	if(isObject($TTT::ActiveRoleGroup))
	{
		$TTT::ActiveRoleGroup.deleteAll();
		$TTT::ActiveRoleGroup.delete();
	}
	$TTT::ActiveRoleGroup = new ScriptGroup(class = "RoleGroup");

	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%inst = %group.getObject(%i).instance();
		$TTT::ActiveRoleGroup.Set(%inst,%inst.data.name);
	}
}

function GameConnection::TTT_SetRole(%client,%role)
{
	%client.role = $TTT::ActiveRoleGroup.NameGet(%role).getId();
	WinCondition_set(%client,%role.wincondition);
	%client.credits = %role.credits;
	%n = %role.name;
	%c = "<color:"@%role.color@">";
	%client.print = "<just:left><font:Palatino Linotype:22>\c3ROLE\c6: <font:Palatino Linotype:45>"@%c@getSubStr(%n,0,1)"<font:Palatino Linotype:43>"@%c@getSubStr(%n,1,strLen(%n)-2);
}

function TTT_PostRoleSetup()
{
	%minigame = BBB_Minigame.getId();
	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%data = %group.getObject(%i);
		%rolename = %data.name;
		%withrole = $TTT::ActiveRoleGroup.WithRole(%roleName);
		%withrolecount = getWordCount(%rolemates);
		%withoutrole = $TTT::ActiveRoleGroup.WithoutRole(%roleName);
		%withoutrolecount = getWordCount(%rolemates);
		%t = %data.color;

		%names = "";
		for(%i = 0; %i < %withrolecount; %i++)
		{
			%client = getWord(%withrole,%i);
			%names = %names TAB %client.getPlayerName();
		}
		%names = ltrim(%names);

		if(%data.rolechat)
		{
			%hsv = rgb2hsv(%t);
			%s = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2))@">";
			%ls = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2))@">";
			%msg = %s@"Your fellow "@%t@%rolename@"s "@%s@"are: \c0";

			%badge = "["@%data.shortname@"]";

			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%client.chatMessage(%msg @ bbb_addListSeperators(removeField(%names,%client.getPlayerName())));
				%client.play2D(BBB_Chat_Sound);
				for(%j = 0; %j < %withrolecount; %j++)
				{
					%roleclient = getWord(%withrole,%i);
					%client.inspectInfo[%roleclient] = "<br><br><br><font:impact:50>"@%t@"X<br><font:palatino linotype:23>"@%t@"Fellow "@%rolename@"<br>"@%t;
					%client.namecolor[%roleclient] = %ls;
					secureCommandToClient("zbR4HmJcSY8hdRhr",%client ,'ClientJoin', %badge SPC %roleclient.getPlayerName(), %roleclient, %roleclient.getBLID (), %roleclient.score, 0, %roleclient.isAdmin, %roleclient.isSuperAdmin);
				}
			}
		}

		if(%data.rolebillboard !$= "")
		{
			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				for(%j = 0; %j < %withrolecount; %j++)
				{
					%roleclient = getWord(%withrole,%i);
					if(%client == %roleclient)
					{
						continue;
					}
					BillboardMount_AddAVBillboard(%client.player.roleBBM,%roleclient.AVBillboardGroup,traitorAVBillboard,%client.getBLID());
				}
			}	
		}
		
		if(%data.publicbillboard !$= "")
		{
			%badge = "["@%data.shortname@"]";
			%msg = %s@"The "@%t@%rolename@"s "@%s@"are: \c0";
			%hsv = rgb2hsv(%t);
			%ls = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2))@">";
			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%player = %client.player;
				Billboard_ClearGhost(BillboardMount_AddBillboard(%player.roleBBM,%data.publicbillboard),%client);
				for(%j = 0; %j < %notwithrolecount; %j++)
				{
					%currclient = getWord(%notwithrole,%j);
					%client.chatMessage(%msg @ bbb_addListSeperators(%names));
					%client.play2D(BBB_Chat_Sound);
					%client.inspectInfo[%currclient] = "<br><br><br><br><br>"@%t@%rolename@"<br>"@%t;
					%client.namecolor[%currclient] = %ls;
					secureCommandToClient("zbR4HmJcSY8hdRhr",%currclient ,'ClientJoin', %badge SPC %client.getPlayerName(), %client, %client.getBLID (), %client.score, 0, %client.isAdmin, %client.isSuperAdmin);
				}
			}			
		}

		if(%data.startingItems !$= "")
		{
			%items = %data.startingItems;
			%itemCount = getWordCount(%items);
			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%player = %client.player;
				for(%j = 0; %j < %itemCount; %j++)
				{
					%player.pickup(new Item(){dataBlock = getWord(%items,%j);});
				}
			}			
		}
	}
}

function TTT_WinCheck()
{
	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	%winners = "";
	for(%i = 0; %i < %count; %i++)
	{
		%data = %group.getObject(%i);
		if(!%data.winCondition.hasWon() && (%data.winOnTimeup && $BBB::rTimeLeft > 0))
		{
			continue;
		}

		%winners = %winners SPC %data;
	}

	return lTrim(%winners);
}