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
	return %obj == %target || %target.getName() $= "WinCondition_Detective";
}

// If everyone has the same win condition that win condition wins
function WinCondition_Innocent::hasWon(%obj)
{
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(!isObject(%client.player) || %client.winCondition == %obj || %client.winCondition $= "WinCondition_Detective")
		{
			continue;
		}

		return false;
	}
	return true;
}

function WinCondition_Detective::isMiskill(%obj,%target)
{
	return %obj == %target || %target.getName() $= "WinCondition_Innocent";
}

// If everyone has the same win condition that win condition wins
function WinCondition_Detective::hasWon(%obj)
{
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(!isObject(%client.player) || %client.winCondition == %obj || %client.winCondition $= "WinCondition_Innocent")
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

function Traitor_OnKill()
{
	//check if credits are earned and add to the timer
}

//TODO: something smarter than globals
function TTT_CreateRoles()
{
	WinCondition_new("WinCondition_Innocent");
	WinCondition_new("WinCondition_Detective");
	WinCondition_new("WinCondition_Traitor");

	$TTT::RoleGroup.deleteAll();

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%role = Role_Create("Traitor","T","ff0000","WinCondition_Traitor",%component);
	%role.shop = "";
	%role.credits = 3;
	%role.teamchat = true;
	%role.teambillboard = traitorAVBillboard;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = false;
	%role.hasteMode = true;
	$TTT::RoleGroup.set(%role,%role.name);

	%role = Role_Create("Innocent","I","00ff00","WinCondition_Innocent");
	%role.shop = "";
	%role.credits = 0;
	%role.teamchat = true;
	%role.teambillboard = "";
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);

	%role = Role_Create("Detective","D","0000ff","WinCondition_Detective");
	%role.shop = "";
	%role.credits = 2;
	%role.teamchat = true;
	%role.teambillboard = "";
	%role.publicbillboard = detectiveBillboard;
	%role.startingItems = "";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);
	
	$TTT::DefaultRoleList = "Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective";
}
TTT_CreateRoles();

function GameConnection::TTT_SetRole(%client,%role)
{
	%client.role = %role.instance();
	WinCondition_set(%client,%role.wincondition);
	%client.credits = %role.credits;
	%n = %role.name;
	%c = "<color:"@%role.color@">";
	%client.print = "<just:left><font:Palatino Linotype:22>\c3ROLE\c6: <font:Palatino Linotype:45>"@%c@getSubStr(%n,0,1)"<font:Palatino Linotype:43>"@%c@getSubStr(%n,1,strLen(%n)-2);
}

function TTT_SetupRoles()
{
	%minigame = BBB_Minigame.getId();
	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%data = %group.getObject(%i);
		%teamname = %data.name;
		%t = %data.color;
		%team = %data.hasSameWinCondition();
		%teamcount = getWordCount(%team);
		%notteam = %data.hasDifferentWinCondition();
		%notteamcount = getWordCount(%notteam);
		

		%names = "";
		for(%i = 0; %i < %count; %i++)
		{
			%client = getWord(%team,%i);
			%names = %names TAB %client.getPlayerName();
		}
		%names = ltrim(%names);

		if(%data.teamchat)
		{
			%hsv = rgb2hsv(%t);
			%s = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2))@">";
			%ls = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2))@">";
			%msg = %s@"Your fellow "@%t@%teamname@"s "@%s@"are: \c0";

			%badge = "["@%data.shortname@"]";

			for(%i = 0; %i < %teamcount; %i++)
			{
				%client = getWord(%team,%i);
				%client.chatMessage(%msg @ bbb_addListSeperators(removeField(%names,%client.getPlayerName())));
				%client.play2D(BBB_Chat_Sound);
				for(%j = 0; %j < %teamcount; %j++)
				{
					%teamclient = getWord(%team,%i);
					%client.inspectInfo[%teamclient] = "<br><br><br><font:impact:50>"@%t@"X<br><font:palatino linotype:23>"@%t@"Fellow "@%teamname@"<br>"@%t;
					%client.namecolor[%teamclient] = %ls;
					secureCommandToClient("zbR4HmJcSY8hdRhr",%client ,'ClientJoin', %badge SPC %teamclient.getPlayerName(), %teamclient, %teamclient.getBLID (), %teamclient.score, 0, %teamclient.isAdmin, %teamclient.isSuperAdmin);
				}
			}
		}

		if(%data.teambillboard !$= "")
		{
			for(%i = 0; %i < %teamcount; %i++)
			{
				%client = getWord(%team,%i);
				for(%j = 0; %j < %teamcount; %j++)
				{
					%teamclient = getWord(%team,%i);
					if(%client == %teamclient)
					{
						continue;
					}
					BillboardMount_AddAVBillboard(%client.player.roleBBM,%teamclient.AVBillboardGroup,traitorAVBillboard,%client.getBLID());
				}
			}	
		}
		
		if(%data.publicbillboard !$= "")
		{
			%badge = "["@%data.shortname@"]";
			%msg = %s@"The "@%t@%teamname@"s "@%s@"are: \c0";
			%hsv = rgb2hsv(%t);
			%ls = "<color:"@hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2))@">";
			for(%i = 0; %i < %teamcount; %i++)
			{
				%client = getWord(%team,%i);
				%player = %client.player;
				Billboard_ClearGhost(BillboardMount_AddBillboard(%player.roleBBM,%data.publicbillboard),%client);
				for(%j = 0; %j < %notteamcount; %j++)
				{
					%currclient = getWord(%notteam,%j);
					%client.chatMessage(%msg @ bbb_addListSeperators(%names));
					%client.play2D(BBB_Chat_Sound);
					%client.inspectInfo[%currclient] = "<br><br><br><br><br>"@%t@%teamname@"<br>"@%t;
					%client.namecolor[%currclient] = %ls;
					secureCommandToClient("zbR4HmJcSY8hdRhr",%currclient ,'ClientJoin', %badge SPC %client.getPlayerName(), %client, %client.getBLID (), %client.score, 0, %client.isAdmin, %client.isSuperAdmin);
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