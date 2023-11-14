//major refactoring:
//remove class based roles and replace with scriptobjects that hold function names for callbacks
//allows for easy reuse of callbacks without needing to copy and paste or parenting
//seperate roles from teams/team related ideas and win conditions
//a team will be it's own object and be instanced and setup on round start
//replace the RoleList idea with Modes
//each mode will have it's own rules and ideas and have it's own role lists which are independant from the rules

//modes:
//they are essentialy an easy way to swap between games within "ttt"
//i suppose this means i can implement whatever i want within this space although that has some interesting side effects
//each mode will have the following information
//onStart, called when the mode starts
//onStop, called when the mode stops
//onspawn, callback when a player is spawned
//ondamaged, callback when player is damaged
//ondamage, callback when player damages
//onkill, callback when player kills
//onkilled, callback when player is killed
//ontrigger, callback when the player activates a trigger
//pacakge, a package to activate and deactivate when the mode is active

datablock PlayerData(StandardModePlayer : PlayerStandardArmor)
{
	classname = "ModePlayer";
};

function ModePlayer::onAdd(%db,%player)
{
	if(%player.client)
	{
		Mode_onSpawn($Mode::Manager.currentMode,%player.client);
	}
}

function ModePlayer::onRemove(%db,%player)
{
	
}

function ModePlayer::onCollision(%db,%player,%obj,%vec,%speed)
{

}

function ModePlayer::onImpact(%db,%player,%obj,%vec,%speed)
{

}

function ModePlayer::Damage(%db,%victim,%source,%pos,%n,%type)
{
	Mode_onDamage($Mode::Manager.currentMode,%source,%victim);
	if(%victim.isDisabled())
	{
		Mode_onDisable($Mode::Manager.currentMode,%source,%victim);
	}
}

function ModePlayer::OnTrigger(%db,%player,%num,%val)
{
	Mode_onTrigger($Mode::Manager.currentMode,%source,%num,%val);
}

function ModeMinigame::addMember(%obj, %client)
{
	Mode_Join($Mode::Manager.currentMode,%client)
}

function ModeMinigame::removeMember(%obj, %client)
{
	Mode_Leave($Mode::Manager.currentMode,%client)
}

//need a script to manage modes
function Modes_Setup()
{
	if(!isObject($Mode::Manager))
	{
		%manager = $Mode::Manager = new ScriptGroup();
		%manager.currentMode = "";
		$manager.minigame = new ScriptObject()
		{
			superclass = "MiniGameSO";
			class = "ModeMinigame";
			owner = -1;
			numMembers = 0;

			title = "MOOD";
			colorIdx = "3";
			inviteOnly = false;
			UseAllPlayersBricks = true;
			PlayersUseOwnBricks = false;

			Points_BreakBrick = 0;
			Points_PlantBrick = 0;
			Points_KillPlayer = 0;
			Points_KillSelf = 0;
			Points_Die = 0;

			respawnTime = "-1";
			vehiclerespawntime = "10000";
			brickRespawnTime = "30000";
			playerDatablock = "StandardModePlayer";

			useSpawnBricks = true;
			fallingdamage = true;
			weapondamage = true;
			SelfDamage = true;
			VehicleDamage = true;
			brickDamage = false;

			enableWand = false;
			EnableBuilding = false;
			enablePainting = false;

			StartEquip0 = 0;
			StartEquip1 = 0;
			StartEquip2 = 0;
			StartEquip3 = 0;
			StartEquip4 = 0;
		};

		
	}
}
Modes_Setup();


function Create_Mode(%name,%data)
{
	%newMode = d(%name SPC "Mode",%data);
	$Mode::Manager.add(%newMode);
	if($Mode::Manager.currentMode $= "")
	{
		%newMode.start();
	}	
	return %newMode;
}

function Mode::Start(%o)
{
	if(isObject($Mode::Manager.currentMode))
	{
		$Mode::Manager.currentMode.stop();
	}

	$Mode::Manager.currentMode = %o;
	activatePackage(%o.pacakge);

	//call join for all players still in the gamemode
	%count = %o.minigame.numMembers;
	for(%i = 0; %i < %count; %i++)
	{
		Mode_Join(%o,%o.minigame.member[%i]);
	}
	Mode_Start(%o);
}


function Mode::Stop(%o)
{
	$Mode::Manager.currentMode = "";
	Mode_Stop(%o);

	//call leave for all players still in the gamemode
	%count = %o.minigame.numMembers;
	for(%i = 0; %i < %count; %i++)
	{
		Mode_Leave(%o,%o.minigame.member[%i]);
	}
	deactivatePackage(%o.pacakge);
}



//TTT:
package TTTPackage
{
	function Mode_onStart(%mode)
	{
		//setup and start ttt
	}

	function Mode_onStop(%mode)
	{
		//clean up and stop ttt
	}

	function Mode_Join(%mode,%client)
	{
		//called when a client joins the mode ()
	}

	function Mode_Leave(%mode,%client)
	{
		//called when a client leaves the mode
	}

	function Mode_onSpawn(%mode,%client)
	{
		//called when something with the mode class is spawned
	}

	function Mode_onDamage(%mode,%source,%target)
	{
		//called when something with the mode class takes damage
	}

	function Mode_onDisable(%mode,%source,%target)
	{
		//called when something with the mode class is disabled
	}

	function Mode_onTrigger(%mode,%source,%trigger,%state)
	{
		//called when something with the mode class activates a trigger
	}
};


%ttt = Create_Mode("TTT","package = TTTPackage");


//roles:
//each role will have the following information
//name, the name dispalyed to players
//description, a description displayed to the players
//icon, an icon name to be referenced for diaply and over head icons
//onspawn, callback when the role is first given
//ongiven, callback when the role is given
//ondamaged, callback when player is damaged
//ondamage, callback when player damages
//onkill, callback when player kills
//onkilled, callback when player is killed
//ontrigger, callback when the player activates a trigger
//onroundstart, callback when the round starts
//onroundend, callback when the round ends
//onroundtick, callback every round tick

return;

$Data::DataCount = -1;
function d(%class,%data)
{
	%e = "";
	%start = 0;
	while((%sep = strPos(%data,"=",%start)) != -1)
	{
		%end = strPos(%data,"\n",%sep);
		if(%end == -1)
		{
			%end = strLen(%data) - 1;
		}
		%valLen = %end - %sep;
		%nameLen = %sep - %start;

		%name = trim(getSubStr(%data,%start,%nameLen));
		%val = trim(getSubStr(%data,%sep + 1,%valLen));

		%e = %e @ %name @ "=\"" @ %val @ "\";";

		%start = %end + 1;
	}

	//delete old data if it exists
	%name = "Role_Data" @ $Data::DataCount++;
	if(isObject(%name))
	{
		%name.delete();
	}

	eval("%obj = new scriptObject(%name){class=getWord(%class,0)@getWord(%class,1);superClass=getWord(%class,1);"@%e@"};");
	return %name.getName();
}

function Roles_DefineCallbacks(%name)
{
	eval("function "@%name@"::onGiven(){return \"\";}");
	eval("function "@%name@"::onDamaged(){return \"\";}");
	eval("function "@%name@"::onDamage(){return \"\";}");
	eval("function "@%name@"::onKill(){return \"\";}");
	eval("function "@%name@"::onKilled(){return \"\";}");
	eval("function "@%name@"::onTrigger(){return \"\";}");
	eval("function "@%name@"::onRoundStart(){return \"\";}");
	eval("function "@%name@"::onRoundEnd(){return \"\";}");
	eval("function "@%name@"::onRoundTick(){return \"\";}");
	eval("function "@%name@"::onSpawn(){return \"\";}");
}
Roles_DefineCallbacks("RoleList");
Roles_DefineCallbacks("Role");


function RoleList::add(%rl,%role,%weight,%min,%max)
{
	//add new role and update vars
	%count = %rl.count + 0;
	%rl.listRole[%count] = %role;
	%rl.roleWeight[%role] = %weight;
	%rl.roleMin[%role] = %min $= "" ? 0 : %min;
	%rl.roleMax[%role] = %max $= "" ? 999999 : %max;
	%count = %rl.count++;

	%totalWeight = %rl.totalWeight += %weight;

	return %rl;
}

function RoleList::getCount(%rl)
{
	return %rl.count;
}

function RoleList::get(%rl,%i)
{
	return %rl.listRole[%i];
}

function RoleList::list(%rl,%n)
{
	%listC = 0;
	//generate the list
	%u = %n;
	%count = %rl.count;
	for(%i = 0; %i < %count; %i++)
	{
		%role = %rl.listRole[%i];
		%numRoles = getMin(%u,getMax(getMin((%rl.roleWeight[%role] / %rl.totalWeight) * %n,%rl.roleMax[%role]),%rl.roleMin[%role]));
		//collect the decimal incase there is any ambigious roles
		%tr += %numRoles - mFloor(%numRoles);
		%nr[%i] = %numRoles;
		%numRoles = mFloor(%numRoles);
		//add found number
		%u -= %numRoles;
		for(%j = 0; %j < %numRoles; %j++)
		{
			%list[%listC] = %role.getId();
			%listC++;
		}
	}
	
	//if there is are any decimals we have ambgious roles
	%aCount = mFloor(%tr);
	for(%a = 0; %a < %aCount; %a++)
	{
		%m = 0;
		for(%i = 1; %i < %count; %i++)
		{
			if(%nr[%i] > %nr[%m])
			{
				%m = %i;
			}
		}
		%nr[%m] -= 1;
		%list[%listC] = %rl.listRole[%m].getId();
		%listC++;
	}
		
	//scramble
	for(%i = 0; %i < %listC; %i++)
	{
		%r = getRandom(0,%listC - 1);
		%t = %list[%i];
		%list[%i] = %list[%r];
		%list[%r] = %t; 
	}

	//make into a word list because torque is dumb
	%s = "";
	for(%i = 0; %i < %listC; %i++)
	{
		%s = %s SPC %list[%i];
	}
	%s = trim(%s);

	return %s;
}

function Role::Tag(%r,%a)
{
	return getWord(%r.tags,getMax(%a,0));
}

function Role::Tags(%r,%a,%b)
{
	if(%b $= "")
	{
		return getWords(%r.tags,getMax(%a,0));
	}
	return getWords(%r.tags,getMax(%a,0),getMax(%b,0));
}


function Role::TagPos(%r,%tags)
{
	%rtags = %r.tags;
	%rcount = getWordCount(%rtags);
	%tcount = getWordCount(%tags);
	for(%i = 0; %i < %rcount; %i++)
	{
		%currTag = getWord(%rtags,%i);
		for(%j = 0; %j < %tCount; %j++)
		{
			if(%currTag $= getWord(%tTags,%j))
			{
				return %i;
			}
		}
	}

	return -1;
}

$ClassicRolelist = d("Classic RoleList",
	"name = Classic"NL
	"PreTime = 15000"NL
	"PostTime = 15000"NL
	"CTags = D T")

	.add(d("Traitor Role",
			"name = Traitor"NL
			"color = \c0"NL
			"teamName = Traitors"NL
			"teamColor = \c0"NL
			"tags = T"NL
			"WTags = T"NL
			"KTags = T")
		,2,1)
	.add(d("Detective Role",
			"name = Detective"NL
			"color = \c1"NL
			"teamName = Innocents"NL
			"teamColor = \c2"NL
			"tags = I D"NL
			"WTags = I"NL
			"KTags = D")
		,1)
	.add(d("Innocent Role",
			"name = Innocent"NL
			"color = \c2"NL
			"teamName = Innocents"NL
			"teamColor = \c2"NL
			"tags = I"NL
			"WTags = I"NL
			"KTags = D")
		,5);

package ClassicRoleList
{
	function GameConnection::UpdateHud(%client)
	{
		%client.CenterPrint(%client.printC.get(),10);
		%client.BottomPrint(%client.printB.get(),10,true);
	}

	function GameConnection::HealthHud(%client)
	{
		%player = %client.player;
		%client.printB.set(2,"\n<just:left><color:" @ hexLerp("00ff00","ff0000",%player.getDamagePercent()) @ ">" @ mFloor(100 - %player.getDamageLevel()) @ "%");
	}

	function GameConnection::CreditsHud(%client)
	{
		%player = %client.player
		%client.prinB.set(1,%player.Role_Credits @ "c");
	}

	function Game::ParseTags(%game)
	{
		%group = %game.playerSet;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%currPlayer = %group.getObject(%i);
			%currRole = %currPlayer.getRole();
			%cTag = %currRole.tag(%currRole.tagPos(%game.roleList.Ctags));
			
			for(%j = 0; %j < %count; %j++)
			{
				%checkPlayer = %group.getObject(%j);
				%checkRole = %checkPlayer.getRole();
				if(%i == %j)
				{
					continue;
				}

				%currPlayer.Roles_Chat[%checkPlayer.getId()] = %cTag !$= "" && %checkRole.tag(%checkRole.tagPos(%game.roleList.Ctags)) $= %cTag;
				%currPlayer.Roles_Win[%checkPlayer.getId()] = %checkRole.tagPos(%currRole.wTags) != -1;
				%currPlayer.Roles_Known[%checkPlayer.getId()] = %checkRole.tagPos(%currRole.kTags) != -1;
			}
		}
	}

	function ClassicRoleList::OnRoundStart(%rl,%game)
	{
		%game.ParseTags();
		//generate known players string for everyone
		%group = %game.playerSet;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%s = "";
			%currPlayer = %group.getObject(%i);
			%client = %currPlayer.client;
			if(isObject(%client))
			{
				continue;
			}

			for(%j = 0; %j < %count; %j++)
			{
				%checkPlayer = %group.getObject(%j);
				if(%currPlayer.Roles_Known[%checkPlayer])
				{
					%s = %s SPC %checkPlayer.getRole().color @ %checkPlayer.getPlayerName() SPC "(" @ %checkPlayer.getRole().name @ ")";
					//TODO: add their role to the player list
				}
			}
			%s = trim(%s);
			%s = addListSeperators(%s);
			
			%client.chatMessage("\c6Known Players: " @ %s);
		}

		//set round timer
		%game.timer.set(60000 * 5);
	}

	function ClassicRoleList::OnRoundTick(%rl,%game)
	{
		%phase = %game.phase;

		//update display
		%group = clientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%client = %group.getObject(%i);
			if(%phase !$= "")
			{
				%client.printB.set(0,"<just:left>\c4" @ %phas);
			}
			%client.printB.set(3,"<just:right>\c3" @ getTimeString(mRound(%game.timer.get() / 1000)));
			%client.UpdateHud();
		}
	}

	function ClassicRoleList::OnRoundReset(%rl,%game)
	{

	}

	function ClassicRoleList::OnSpawn(%rl,%game,%player)
	{
		%client = %player.client;
		if(isObject(%client))
		{
			%client.HealthHud();
		}
	}

	function ClassicRoleList::OnGiven(%rl,%game,%player,%role)
	{
		%client = %player.client;
		if(isObject(%client))
		{
			%client.printB.set(0,"<just:left>" @ %role.color @ %role.name);
		}
	}

	function ClassicRoleList::OnDamage(%rl,%game,%player,%source,%pos,%val,%type)
	{	
		%client = %player.client;
		if(isObject(%client))
		{
			%client.HealthHud();
		}
	}

	function ClassicRoleList::OnDeath(%rl,%game,%player,%source,%pos,%val,%type)
	{

	}

	function ClassicRoleList::OnTrigger(%rl,%game,%player,%num,%val)
	{

	}

	function TraitorRole::OnGiven(%r,%game,%list,%player,%role)
	{
		if(%currPlayer == %player)
		{
			%currPlayer.Role_Credits = 3;
			%player.client.CreditsHud();
		}

		//TODO: SETUP SHOP
	}

	function TraitorRole::OnDamage(%r,%game,%list,%victim,%source,%pos,%n,%type)
	{	

	}

	function TraitorRole::OnDeath(%r,%game,%list,%victim,%source,%pos,%n,%type)
	{
		//get remaining innocents
		%d = 0;
		%t = 0;
		%playerList = %game.PlayerList();
		%count = %playerList.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%player = %playerList.getObject(%i);
			if(%player.Game_role.tagPos("I") != -1)
			{
				if(%player.isDisabled())
				{
					%d++;
				}
				%t++;
			}
		}

		%deathReward = false;
		if((%d - %game.get("Traitor_PrevDeaths")) / %t >= 0.35)
		{
			%deathReward = true;
		}

		%count = %list.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%player = %list.getObject(%i);
			if(%player.isDisabled())
			{
				continue;
			}

			if(%deathReward)
			{
				%player.Role_Credits++;
			}

			if(%player.client == GetSourceClient(%source) && %victim.tagPos("D") != -1)
			{
				%player.role_Credits++;
			}
			%player.client.CreditsHud();
		}

		%game.get("Traitor_PrevDeaths",%d)
	}

	function TraitorRole::OnTrigger(%r,%game,%list,%player,%num,%val)
	{

	}

};