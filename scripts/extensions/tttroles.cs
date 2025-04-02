function WinCondition_new(%name,%winBlocking,%winOnTimeup,%displayName,%winSound)
{
	if(isObject(%name))
	{
		%name.delete();
	}

	%new = new ScriptObject(%name)
	{
		superClass = WinCondition_Basic;
		winBlocking = %winBlocking;
		winOnTimeup = %winOnTimeup;
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
	if(%count == 0)
	{
		return false;
	}

	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%others,%i);
		if(!isObject(%client.player) || !%client.winCondition.winBlocking)
		{
			continue;
		}

		return false;
	}
	return true;
}

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

function WinCondition_Basic::IsLiving(%obj)
{
	%clients = %obj.hasSameWinCondition();
	%count = getWordCount(%clients);

	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%clients,%i);
		if(!isObject(%client.player))
		{
			continue;
		}
		return true;
	}
	return false;
}

function WinCondition_Survivor::isMiskill(%obj,%target)
{
	return false;
}

function Role_Create(%name,%shortname,%color,%wincondition,%components,%description)
{
	%role = new ScriptObject()
	{
		class = "Role";
		
		name = %name;
		shortname = %shortname;
		color = %color;
		winCondition = %wincondition;

		components = %components;

		description = %description;
	};
	return %role;
}

function Role::Instance(%role)
{
	%holder = ComponentHolder_Create(%role.components);
	%holder.data = %role;
	return %holder;
}

if(!isObject($RoleGroups))
{
	$RoleGroups = new ScriptGroup();
}

function RoleGroup_Find(%name)
{
	return $RoleGroups.name[%name];
}

function RoleGroup_Create(%name,%chatcolor,%time,%list,%description)
{
	%set = new ScriptGroup()
	{
		name = %name;
		class = "RoleGroup";
		list = %list;
		description = %description;
		time = %time;
		defaultChatColor = %chatColor;
	};

	return %set;
}

function RoleGroup::OnAdd(%obj)
{
	if(isObject($RoleGroups.name[%name]))
	{
		$RoleGroups.name[%name].delete();
	}
	$RoleGroups.name[%obj.name] = %obj;
	$RoleGroups.add(%obj);
}

function RoleGroup::OnRemove(%obj)
{
	$RoleGroups.name[%obj.name] = "";
	$RoleGroups.remove(%obj);
}

function RoleGroup::Role(%obj,%role)
{
	%obj.add(%role);
}

function RoleGroup::SetRoles(%obj,%clients)
{
	%ActiveRoleGroup = new ScriptGroup(){class = "ActiveRoleGroup";};

	%count = %obj.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%inst = %obj.getObject(%i).instance();
		%ActiveRoleGroup.Set(%inst);
	}

	%count = getWordCount(%clients);
	%roles = getWords(%obj.list,0,%count-1);
	for(%i = 0; %i < %count; %i++)
	{
		%client = getWord(%clients,%i);

		%r = getRandom(0,(%count-%i)-1);
		%ActiveRoleGroup.SetRole(getWord(%roles,%r),%client);
		%roles = removeWord(%roles,%r);
	}

	%count = %ActiveRoleGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%ActiveRoleGroup.getObject(%i).StartCallback("OnGiven");
	}
	
	%count = %obj.getCount();
	for(%a = 0; %a < %count; %a++)
	{
		%role = %ActiveRoleGroup.getObject(%a);
		%data = %role.data;
		%rolename = %data.name;
		%withrole = %ActiveRoleGroup.WithRole(%roleName);
		%withrolecount = getWordCount(%withrole);
		%withoutrole = %ActiveRoleGroup.WithoutRole(%roleName);
		%withoutrolecount = getWordCount(%withoutrole);
		%t = %data.color;
		%hsv = rgb2hsv(%t);
		%s = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.2,getWord(%hsv,2));
		%ls = hsv2rgb(getWord(%hsv,0),getWord(%hsv,1) * 0.5,getWord(%hsv,2));
		%names = "";
		for(%i = 0; %i < %withrolecount; %i++)
		{
			%client = getWord(%withrole,%i);
			%names = %names TAB %client.fakeName;

			%client.schedule(0,"chatMessage","<color:"@%s@">You are a <color:"@%s@">"@%rolename@"<color:"@%s@">! Use /gamehelp for your objective.");
		}
		%names = ltrim(%names);

		if(%data.rolechat)
		{
			%msg = "<color:"@%s@">Your fellow <color:"@%t@">"@%rolename@"s <color:"@%s@">are: <color:"@%t@">";

			%badge = "["@%data.shortname@"]";

			for(%i = 0; %i < %withrolecount; %i++)
			{
				%client = getWord(%withrole,%i);
				%client.chatMessage(%msg @ stringList(trim(strReplace(%names,%client.fakeName,"You")),"\t",", ","and"));

				for(%j = 0; %j < %withrolecount; %j++)
				{
					%roleclient = getWord(%withrole,%j);
					%client.inspectInfo[%roleclient] = "<br><br><br><font:impact:50><color:"@%t@">X<br><font:palatino linotype:23>Fellow "@%rolename@"<br>";
					%client.namecolor[%roleclient] = %ls;
					%client.badge[%roleClient] = %badge;
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
					%client.badge[%currClient] = %badge;
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

	return %ActiveRoleGroup;
}

function ActiveRoleGroup::Set(%obj,%role)
{
	%obj._[%role.data.name] = %role;
	%obj.add(%role);
}

function ActiveRoleGroup::NameGet(%obj,%name)
{
	return %obj._[%name];
}

function ActiveRoleGroup::SetRole(%obj,%index,%client)
{
	%role = %obj.getObject(%index);
	%data = %role.data;

	if(%client.role !$= "")
	{
		%n = %client.role.data.name;
		%obj.roleClients[%n] = trim(strReplace(%obj.roleClients[%n] @ " ",%client @ " ",""));
	}

	%client.role = %role;
	%client.revealBadge = "["@%data.shortName@"]";
	WinCondition_set(%client,%data.wincondition);
	%client.credits = %data.credits;
	%n = %data.name;
	%c = "<color:"@%data.color@">";
	%client.print = "<just:left><font:Palatino Linotype:22>\c3ROLE\c6: <font:Palatino Linotype:45>"@%c@getSubStr(%n,0,1)
	@"<font:Palatino Linotype:43>"@getSubStr(%n,1,strLen(%n)-1);

	if(%obj.roleClients[%n] $= "")
	{
		%obj.roleClients[%n] = %client;
	}
	else
	{
		%obj.roleClients[%n] = %obj.roleClients[%n] SPC %client;
	}
}

function ActiveRoleGroup::WithRole(%obj,%name)
{
	return %obj.roleClients[%name];
}

function ActiveRoleGroup::WithoutRole(%obj,%name)
{
	%ignoreRole = %obj._[%name].getId();
	%count = %obj.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%role = %obj.getObject(%i);
		if(%ignoreRole == %role.getId())
		{
			continue;
		}

		%clients = %clients SPC %obj.roleClients[%role.data.name];
	}
	return ltrim(%clients);
}

function ActiveRoleGroup::WinCheck(%obj,%timeUp)
{
	%count = %obj.getCount();
	%winners = "";
	for(%i = 0; %i < %count; %i++)
	{
		%winCondition = %obj.getObject(%i).data.winCondition;
		if(%won[%winCondition] || (!%winCondition.hasWon() && (!%winCondition.winOnTimeup || !%timeUp)))
		{
			continue;
		}
		%won[%winCondition] = true;
		%winners = %winners SPC %winCondition;
	}
	
	%winners = ltrim(%winners);

	if(%winners $= "")
	{
		if(%timeup)
		{
			return "WinCondition_None";
		}
		return "";
	}

	for(%i = 0; %i < %count; %i++)
	{
		%winCondition = %obj.getObject(%i).data.winCondition;
		if(%won[%winCondition] || %winCondition.winBlocking || !%wincondition.IsLiving())
		{
			continue;
		}
		%won[%winCondition] = true;
		%winners = %winners SPC %winCondition;
	}
	return %winners;
}

function serverCmdGameHelp(%client)
{
	%string = %client.minigame.roleGroup.description NL %client.role.data.description;
	%count = getRecordCount(%string);
	for(%i = 0; %i < %count; %i++)
	{
		%client.chatMessage("\c6"@getRecord(%string,%i));
	}
}

function Traitor_OnKill(%obj,%target,%deadclient)
{
	//check if credits are earned and add to the timer
 	$BBB::rTimeLeft += %obj.data.hasteModeAdd;

	%mini = BBB_Minigame;
	%non = %obj.getGroup().WithoutRole(%obj.data.name);
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
	%with = %obj.getGroup().WithRole(%obj.data.name);
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

function TraitorWar_OnGiven(%obj) //demote over people to survivor
{
	%activeRoleGroup = %obj.getGroup();
	%mine = %activeRoleGroup.WithRole(%obj.data.name);
	%r = %activeRoleGroup.WithRole("Red Traitor");
	%g = %activeRoleGroup.WithRole("Blue Traitor");
	%b = %activeRoleGroup.WithRole("Green Traitor");
	%max = getMax(getMin(getMin(getWordCount(%r),getWordCount(%g)),getWordCount(%b)),1);
	%count = getWordCount(%mine);
	%demotion = %count - %max;
	for(%i = 0; %i < %demotion; %i++)
	{
		%r = getRandom(0,(%count-%i)-1);
		%selection = getWord(%mine,%r);
		%mine = removeWord(%mine,%r);
		talk(%r SPC %selection);
		%activeRoleGroup.SetRole(3,%selection);
	}
}

function Innocent_OnGiven(%obj) //promote people to detective
{
	%minigame = BBB_Minigame.getId();
	%activeRoleGroup = %obj.getGroup();
	%players = %minigame.numPlayers;
	%detectives = mFloor(%players / 8);

	%innos = %obj.getGroup().WithRole(%obj.data.name);
	%innocount = getWordCount(%innos);
	for(%i = 0; %i < %detectives; %i++)
	{
		%r = getRandom(0,(%innocount-%i)-1);
		%selection = getWord(%innos,%r);
		%innos = removeWord(%innos,%r);
		
		%activeRoleGroup.SetRole(2,%selection);
	}
}

function TTT_CreateGroups()
{
	WinCondition_new("WinCondition_None",true,true,"\c8NO ONE");
	WinCondition_new("WinCondition_Innocent",true,true,"\c2INNOCENTS");
	WinCondition_new("WinCondition_Survivor",false,true,"\c3SURVIVORS");
	WinCondition_new("WinCondition_Traitor",true,false,"\c0TRAITORS","Traitor_Win");
	WinCondition_new("WinCondition_Traitor2",true, false,"\c2TRAITORS","Traitor_Win");
	WinCondition_new("WinCondition_Traitor3",true, false,"\c1TRAITORS","Traitor_Win");

	%group = RoleGroup_Create("Default",hsv2rgb(120,0.5,1),60000 * 3 ,"0 1 1 1 1 1 1"SPC
	"1 1 1 0"SPC
	"1 1 1 0"SPC
	"1 1 1 0"SPC
	"1 1 1 0"SPC
	"1 1 1 0"SPC
	"1 1 1 0",
	"Default."NL
	"1 Traitor per 4 players and 1 Detective per 8 players."NL
	"Everyone else vs. the Traitors, only the Traitors know eachother and everyone knows the Detectives.");

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%role = Role_Create("Traitor","T","ff0000","WinCondition_Traitor",%component,
	"You are a traitor to the terrorists."NL
	"Kill all Innocents and Detectives."NL
	"You have access to numerous shop items and some starting credits, drop the shop item in your inventory or us /shop.");
	%role.shop = $TTTInventory::TraitorShopMain;
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.hasteMode = true;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35;
	%role.hasteModeAdd = 20000;
	%group.role(%role);

	%component = Component_Create("Innocent");
	%component.callback("OnGiven","Innocent_OnGiven");
	%role = Role_Create("Innocent","I","00ff00","WinCondition_Innocent",%component,
	"You are an innocent terrorist."NL
	"Kill all Traitors."NL
	"Help your detectives figure out who the Traitors are and stay alive");
	%role.shop = "";
	%role.credits = 0;
	%role.rolechat = false;
	%role.rolebillboard = "";
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.hasteMode = false;
	%group.role(%role);

	%role = Role_Create("Detective","D","0000ff","WinCondition_Innocent","",
	"You are an innocent terrorist with special privelages."NL
	"Kill all Traitors."NL
	"You start with armor and have access to numerous shop items and some starting credits, drop the shop item in your inventory or us /shop.");
	%role.shop = $TTTInventory::DetectiveShopMain;
	%role.credits = 1;
	%role.rolechat = true;
	%role.rolebillboard = "";
	%role.publicbillboard = detectiveBillboard;
	%role.startingItems = "BodyArmorItem DNAScannerItem";
	%role.hasteMode = false;
	%group.role(%role);

	%group = RoleGroup_Create("Traitor War",hsv2rgb(60,0.5,1),60000 * 5,"0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1 2 0 1",
	"Traitor War."NL
	"3 teams of traitors and neutral survivors."NL
	"You don't know who you're fighting but you know they all must die.");

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%component.callback("OnGiven","TraitorWar_OnGiven");
	%role = Role_Create("Red Traitor","RT","ff0000","WinCondition_Traitor",%component,
	"You are a red traitor one of many."NL
	"Kill all other teams of traitors."NL
	"You have access to numerous shop items and some starting credits, drop the shop item in your inventory or us /shop.");
	%role.shop = $TTTInventory::TraitorShopMain;
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.hasteMode = false;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35;
	%role.hasteModeAdd = 0;
	%group.role(%role);

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%component.callback("OnGiven","TraitorWar_OnGiven");
	%role = Role_Create("Green Traitor","GT","00ff00","WinCondition_Traitor2",%component,
	"You are a green traitor one of many."NL
	"Kill all other teams of traitors."NL
	"You have access to numerous shop items and some starting credits, drop the shop item in your inventory or us /shop.");
	%role.shop = $TTTInventory::TraitorShopMain;
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard2;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.hasteMode = false;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35;
	%role.hasteModeAdd = 0;
	%group.role(%role);

	%component = Component_Create("Traitor");
	%component.callback("OnKill","Traitor_OnKill");
	%component.callback("OnGiven","TraitorWar_OnGiven");
	%role = Role_Create("Blue Traitor","BT","0000ff","WinCondition_Traitor3",%component,
	"You are a blue traitor one of many."NL
	"Kill all other teams of traitors."NL
	"You have access to numerous shop items and some starting credits, drop the shop item in your inventory or us /shop.");
	%role.shop = $TTTInventory::TraitorShopMain;
	%role.credits = 3;
	%role.rolechat = true;
	%role.rolebillboard = traitorAVBillboard3;
	%role.publicbillboard = "";
	%role.startingItems = "";
	%role.hasteMode = false;
	%role.creditGain = 1;
	%role.creditDeadPercent = 0.35;
	%role.hasteModeAdd = 0;
	%group.role(%role);

	%role = Role_Create("Survivor","S","ffff00","WinCondition_Survivor","",
	"You are a survivor alone."NL
	"Survive until the end even at the cost of other survivors.");
	%role.shop = "";
	%role.credits = 0;
	%role.rolechat = false;
	%role.rolebillboard = "";
	%role.publicbillboard = "";
	%role.startingItems = "BodyArmorItem";
	%role.hasteMode = false;
	%group.role(%role);
}