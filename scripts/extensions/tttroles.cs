function WinCondition_new(%name,%displayName,%winSound)
{
	if(isObject(%name))
	{
		%name.delete();
	}

	%new = new ScriptObject(%name)
	{
		superClass = WinCondition_Basic;
		display = %displayName;
		sound = %winSound;
	};

	return %new;
}

function WinCondition_set(%client,%condition,%winSound)
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
	%others = %obj.hasDifferentWinCondition();
	%count = getWordCount(%others);
	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%others,%i);
		if(!isObject(%client.player))
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
	%obj = %obj.getId();
	%clients = "";
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(%client.winCondition.getId() != %obj)
		{
			continue;
		}

		%clients = %clients SPC %client;
	}
	return ltrim(%clients);
}

//helper for getting clients with a different win condition
function WinCondition_Basic::hasDifferentWinCondition(%obj)
{
	%obj = %obj.getId();
	%clients = "";
	%mg = BBB_Minigame.getId();
	%count = %mg.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %mg.playingClients[%i];
		if(%client.winCondition.getId() == %obj)
		{
			continue;
		}

		%clients = %clients SPC %client;
	}
	return ltrim(%clients);
}

function Role_Create(%name,%shortname,%color,%wincondition,%components)
{
	%role = new ScriptObject()
	{
		class = "Role";
		
		name = %name;
		shortname = %shortname;
		color = %color;
		winCondition = %wincondition;

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
 	$BBB::rTimeLeft += %obj.data.hasteModeAdd;

	%mini = BBB_Minigame;
	%non = $TTT::ActiveRoleGroup.WithoutRole(%obj.data.name);
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
	%percentDead = %dead / %count;

	//is it reward time?
	if(%percentDead < %obj.data.creditDeadPercent * (%obj.gainedRewards + 1))
	{
		%obj.Continue(%target,%deadclient);
		return;
	}

	//reward all traitors the amount
	%with = $TTT::ActiveRoleGroup.WithRole(%obj.data.name);
	%count = getWordCount(%with);
	%gain = %obj.data.creditGain;
	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%with,%i);
		if(isObject(%client.player))
		{
			%client.chatMessage("\c6Well done. You have been awarded\c3" SPC %gain SPC "Credit\c6 for your hard work.");
			%client.credits += %gain;
		}
	}
	
	%obj.gainedRewards++;
}

function Innocent_OnGiven(%obj) //promote people to detective
{
	%minigame = BBB_Minigame.getId();
	%players = %minigame.numPlayers;
	%detectives = mFloor(%players / 8);

	%innos = $TTT::ActiveRoleGroup.WithRole(%obj.data.name);
	%innocount = getWordCount(%innos);
	for(%i = 0; %i < %detectives; %i++)
	{
		%r = getRandom(0,%innocount-1);
		%innocount--;
		%selection = getWord(%innos,%r);
		%innos = removeWord(%innos,%r);
		
		%selection.TTT_SetRole("Detective");
	}
}

//TODO: something smarter than globals
function TTT_CreateRoles()
{
	WinCondition_new("WinCondition_Innocent","\c2INNOCENTS");
	WinCondition_new("WinCondition_Traitor","\c0TRAITORS","Traitor_Win");

	$TTT::RoleGroup.deleteAll();

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%role = Role_Create("Traitor","T","ff0000","WinCondition_Traitor",%component);
	%role.shop = $TTTInventory::TraitorShopMain;
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = false;
	%role.hasteMode = true;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35;
	%role.hasteModeAdd = 20000;
	$TTT::RoleGroup.set(%role,%role.name);

	%component = Component_Create("Innocent");
	%component.callback("OnGiven","Innocent_OnGiven");
	%role = Role_Create("Innocent","I","00ff00","WinCondition_Innocent",%component);
	%role.shop = "";
	%role.credits = 0;
	%role.rolechat = false;
	%role.rolebillboard = "";
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);

	%role = Role_Create("Detective","D","0000ff","WinCondition_Innocent");
	%role.shop = $TTTInventory::DetectiveShopMain;
	%role.credits = 1;
	%role.rolechat = true;
	%role.rolebillboard = "";
	%role.publicbillboard = detectiveBillboard;
	%role.startingItems = "BodyArmorItem DNAScannerItem";
	%role.winOnTimeup = true;
	%role.hasteMode = false;
	$TTT::RoleGroup.set(%role,%role.name);
	
	$TTT::DefaultRoleList = "Traitor Innocent Innocent Innocent Innocent Innocent Innocent Traitor Innocent Innocent Innocent Traitor Innocent Innocent Innocent Traitor Innocent Innocent Innocent Traitor Innocent Innocent Innocent Traitor";
}
//TTT_CreateRoles();

function TTT_PreRoleSetup()
{
	if(isObject($TTT::ActiveRoleGroup))
	{
		$TTT::ActiveRoleGroup.deleteAll();
		$TTT::ActiveRoleGroup.delete();
	}
	$TTT::ActiveRoleGroup = new ScriptGroup(){class = "RoleGroup";};

	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%inst = %group.getObject(%i).instance();
		$TTT::ActiveRoleGroup.Set(%inst,%inst.data.name);
	}
}

function GameConnection::TTT_SetRole(%client,%rolename)
{
	%role = $TTT::ActiveRoleGroup.NameGet(%rolename).getId();
	%data = %role.data;
	%client.role = %role;
	WinCondition_set(%client,%data.wincondition);
	%client.credits = %data.credits;
	%n = %data.name;
	%c = "<color:"@%data.color@">";
	%client.print = "<just:left><font:Palatino Linotype:22>\c3ROLE\c6: <font:Palatino Linotype:45>"@%c@getSubStr(%n,0,1)
	@"<font:Palatino Linotype:43>"@getSubStr(%n,1,strLen(%n)-1);
}

function TTT_PostRoleSetup()
{
	%minigame = BBB_Minigame.getId();
	%group = $TTT::RoleGroup.getId();
	%count = %group.getCount();
	for(%a = 0; %a < %count; %a++)
	{
		%data = %group.getObject(%a);
		%rolename = %data.name;
		%withrole = $TTT::ActiveRoleGroup.WithRole(%roleName);
		%withrolecount = getWordCount(%withrole);
		%withoutrole = $TTT::ActiveRoleGroup.WithoutRole(%roleName);
		%withoutrolecount = getWordCount(%withoutrole);
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
			%s = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2));
			%ls = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2));
			%msg = "<color:"@%s@">Your fellow <color:"@%t@">"@%rolename@"s <color:"@%s@">are: <color:"@%t@">";

			%badge = "["@%data.shortname@"]";

			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%client.chatMessage(%msg @ stringList(trim(strReplace(%names @ "\t",%client.getPlayerName() @ "\t","You")),"\t",", ","and"));

				for(%j = 0; %j < %withrolecount; %j++)
				{
					%roleclient = getWord(%withrole,%j);
					%client.inspectInfo[%roleclient] = "<br><br><br><font:impact:50><color:"@%t@">X<br><font:palatino linotype:23>Fellow "@%rolename@"<br>";
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
					%roleclient = getWord(%withrole,%j);
					if(%client == %roleclient)
					{
						continue;
					}
					BillboardMount_AddAVBillboard(%client.player.roleBBM,%roleclient.AVBillboardGroup,%data.rolebillboard,%client.getBLID());
				}
			}	
		}
		
		if(%data.publicbillboard !$= "")
		{
			%badge = "["@%data.shortname@"]";
			%hsv = rgb2hsv(%t);
			%s = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2));
			%ls = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2));
			%msg = "<color:"@%s@">The <color:"@%t@">"@%rolename@"s <color:"@%s@">are: <color:"@%t@">";
			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%player = %client.player;
				Billboard_ClearGhost(BillboardMount_AddBillboard(%player.roleBBM,%data.publicbillboard),%client);
				for(%j = 0; %j < %withoutrolecount; %j++)
				{
					%currclient = getWord(%withoutrole,%j);
					%client.inspectInfo[%currclient] = "<br><br><br><br><br><color:"@%t@">"@%rolename@"<br>";
					%client.namecolor[%currclient] = %ls;
					secureCommandToClient("zbR4HmJcSY8hdRhr",%currclient ,'ClientJoin', %badge SPC %client.getPlayerName(), %client, %client.getBLID (), %client.score, 0, %client.isAdmin, %client.isSuperAdmin);
				}
			}

			if(%withRoleCount > 0)
			{
				for(%j = 0; %j < %withoutrolecount; %j++)
				{
					%currclient = getWord(%withoutrole,%j);
					%currclient.chatMessage(%msg @ stringList(%names,"\t",", ","and"));
				}		
			}
		}
		else if(%withrolecount > 0)
		{
			%hsv = rgb2hsv(%t);
			%s = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2));
			%ls = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2));
			if(%withrolecount == 1)
			{
				%msg = "<color:"@%s@">There is <color:"@%t@">"@%withrolecount SPC %rolename;
			}
			else
			{
				%msg = "<color:"@%s@">There are <color:"@%t@">"@%withrolecount SPC %rolename@"s";
			}
			
			if(!%data.rolechat)//display to people with the role
			{
				for(%i = 0; %i < %withrolecount; %i++)
				{
					%client = getWord(%withrole,%i);
					%client.chatMessage(%msg);
				}
			}
			
			for(%i = 0; %i < %withoutrolecount; %i++)
			{
				%client = getWord(%withoutrole,%i);
				%client.chatMessage(%msg);
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
		if(%won[%data.winCondition] || (!%data.winCondition.hasWon() && (!%data.winOnTimeup || $BBB::rTimeLeft > 0)))
		{
			continue;
		}
		%won[%data.winCondition] = true;
		%winners = %winners SPC %data.winCondition;
	}

	return lTrim(%winners);
}