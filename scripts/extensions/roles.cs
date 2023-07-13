$Role::DataCount = -1;
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
	%name = "Role_Data" @ $Role::DataCount++;
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