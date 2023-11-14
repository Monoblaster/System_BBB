$TTT::Asset = "Add-ons/System_BBB/assets";

function Player::SetPlayer(%p,%r)
{
	%p = %p.getId();
	%game = $Game;

	if(%p.Game_Role !$= "")
	{
		%game.playerList[%p.Game_Role].remove(%p);
	}

	if(isObject(%r))
	{
		%r = %r.getId();

		$Game.PlayerList(%r).add(%p);
	}

	%p.Game_Role = %r;
	%game.playerList().add(%p);
}

function Player::GetRole(%p)
{
	return %p.Game_Role;
}

function Game::Callback(%game,%callback,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p,%q,%r,%s,%t,%u,%v,%w,%x,%y,%z)
{
	%group = %game.RoleList;
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%currRole = %group.get(%i);
		%list = %game.PlayerList(%currRole);
		if(%list.getCount() > 0)
		{
			eval("return %currRole."@%callback@"(%game,%list,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p,%q,%r,%s,%t,%u,%v,%w,%x,%y,%z);");
		}
	}

	if(isObject($Game::RoleList))
	{
		eval("return $Game::RoleList."@%callback@"(%game,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p,%q,%r,%s,%t,%u,%v,%w,%x,%y,%z);");
	}
}

function Game::SpawnItem(%game,%brick)
{

}

function Game::SpawnPlayer(%game,%client)
{
	%player = %client.player
	if(isObject(%player) && %player.isDisabled())
	{
		%client.instantRespawn();
		return true;
	}
	return false;
}

function Game::SetRoleList(%game,%rl)
{
	%roleList = %game.roleList
	if(%game.RoleList !$= "")
	{
		deactivatePackage(%game.RoleList.class);
	}
	
	$Game::RoleList = %rl;
	activatePackage(%rl.class);
}

function Game::PlayerList(%game,%r)
{
	%game.ListGroup.add(%rl = %game.playerList[%r] = %game.playerList[%r] || new SimSet());
	return %rl;
}

function Game::Set(%game,%k,%v)
{
	%game.round.temp[%k] = %v;
	return %game;
}

function Game::Get(%game,%k)
{
	return %game.round.temp[%k];
}


function Game_Create()
{
	%game = $Game = new SriptObject(){class = "Game"};
	%game.Running = false;
	%game.listGroup = new ScriptGroup();
	%game.listGroup.add(%game.playerList = new SimSet());
	%game.SetRoleList($ClassicRolelist);
	%game.TImer = Timer_Create().setTick("Game_Tick");
	$g.minigame = new ScriptObject()
	{
		superclass = "MiniGameSO";
		class = "GameMinigame";
		owner = -1;
		numMembers = 0;

		title = "Trouble In Terrorist Town";
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
		playerDatablock = "StandardGamePlayer";

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
	MinigameGroup.add($Game::Minigame);
	$MiniGameColorTaken[$Game::Minigame.colorIdx] = 1;
	$DefaultMinigame = %Game::Minigame;
	commandToAll('AddMinigameLine', $Game::Minigame.getLine(), $Game::Minigame.getId(), $Game::Minigame.colorIdx);
}

function Game_PreStart()
{
	%game = $Game;

	%game.Running = true;
	%game.round = new ScirptObject(){};
	
	%mg = %game.Minigame;
	%count = %mg.numMembers;
	for(%i = 0; %i < %count; %i++)
	{
		if(Game_SpawnPlayer(%client = %mg.member[%i]))
		{
			%game.setPlayer(%client.player);
		}
	}
	//spawn items
	//disable damage for stuff
	//check for map vote
	//check for rolelist vote

	//set timer
	%game.Phase = "Preround";
	%game.Timer.set(%game.RoleList.preTime).setStop("Game_Start");
}

function Game_Start()
{
	%game = $Game;

	//hand out roles
	%playerList = %game.playerList();
	%count = %playerList.getCount();
	%roles = %game.RoleList.list(%count);
	for(%i = 0; %i < %count; %i++)
	{
		%game.setPlayer(%playerList.getObject(%i),getWord(%roles,%i));
	}

	//enable damage for stuff
	//clear phase and pass onto the role list
	%game.Phase = "";
	%game.Callback("onRoundStart");
}

function Game_Reset()
{
	%game = $Game;
	%game.Callback("onRoundEnd");

	%playerSet = %game.playerSet;
	%count = %playerSet.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%playerSet.getObject(%i).setRole("");
	}
	%game.round.delete(); 
	%playerSet.clear();

	%game.Phase = "Postround";
	%game.Timer.set(%game.RoleList.postTime).setStop("Game_PreStart");
}

function Game_End()
{
	%game = $Game;
	//TODO: CLEANUP
	%game.Timer.setStop("").set(0);
	%game.Running = false;
}

function Game_Tick()
{
	Game_Callback("OnRoundTick",%game);
}

datablock PlayerData(StandardGamePlayer : PlayerStandardArmor)
{
	classname = "GamePlayer";
	
	//itemslots
	ItemSlots_Tag[0] = "Primary";
	ItemSlots_EmptyItem[0] = "";
	ItemSlots_Tag[1] = "Secondary";
	ItemSlots_EmptyItem[1] = "";
	ItemSlots_Tag[2] = "Melee";
	ItemSlots_EmptyItem[2] = "";
	ItemSlots_Tag[3] = "Grenade";
	ItemSlots_EmptyItem[3] = "";
};

function GamePlayer::onAdd(%db,%player)
{
	%player.setActionThread("root");

	//TODO: PLAYER APPEARANCE
	%player.mountVehicle = 1;
	%player.setRepairRate(0);

	parent::onAdd(%db,%player);
}

function GamePlayer::onRemove(%db,%player)
{
	%client = %player.client;
	if (isObject(%client) && %client.player = %player)
	{
		%client.player = 0;
	}

	if (isObject(%player.light))
	{
		%player.light.delete();
	}

	parent::onRemove(%db,%player);
}

function GamePlayer::onCollision(%db,%player,%obj,%vec,%speed)
{
	if (%player.isDisabled())
	{
		return;
	}

	%className = %obj.getClassName();
	if (%className $= "Item")
	{
		%player.pickup(%obj);
	}
	parent::onCollision(%db,%player,%obj,%vec,%speed);
}

function GamePlayer::onImpact(%db,%player,%obj,%vec,%speed)
{
	if(%speed >= %db.minImpactSpeed * getWord(%player.getScale(),2))
	{
		%type = $DamageType::Impact;
		if (VectorDot(VectorNormalize(%vec),"0 0 1") > 0.5)
		{
			%type = $DamageType::Fall;
		}
		%player.Damage(0,VectorAdd(%player.getPosition(),%vec),%speed * %db.speedDamageScale,%type);
	}
	parent::onImpact(%db,%player,%obj,%vec,%speed);
}

function GamePlayer::Damage(%db,%victim,%source,%pos,%n,%type)
{
	parent::Damage(%db,%victim,%source,%pos,%n,%type);
	//check if damageable
	if (%victim.getState () $= "Dead" || %victim.invulnerable)
	{
		return;
	}

	//get clients
	%client = %victim.client;
	%sourceClient = Game_GetSourceClient(%source);
	%sourcePlayer = %sourceClient.player;

	if (%n > 0)
	{
		//modify damage
		if (%victim.isCrouched())
		{
			if ($Damage::Direct[%type])
			{
				%n = %n * 2.1;
			}
			else 
			{
				%n = %n * 0.75;
			}
		}
		%scale = getWord(%victim.getScale(), 2);
		%n = %n / %scale;

		//do effects
		%flash = mClampF(%victim.getDamageFlash() + ((%n / %db.maxDamage) * 2),0,0.75);
		%victim.setDamageFlash(%flash);

		if (%n > (%db.painThreshold || 7) + 0)
		{
			%victim.playPain();
		}
	}

	%victim.setDamageLevel(%victim.getDamageLevel() + %n);

	Game_Callback("OnDamage",%victim,%source,%pos,%n,%type);

	if(%victim.isDisabled())
	{
		Game_Callback("onDeath",%victim,%source,%pos,%n,%type);
	}
}

function GamePlayer::OnTrigger(%db,%player,%num,%val)
{
	Game_Callback("OnTrigger",%player,%num,%val);
	if(%num == 0 && %val && %player.getMountedImage(0) <= 0)
	{
		%player.ActivateStuff();
	}

	parent::OnTrigger(%db,%player,%num,%val);
}

package Game
{
	function GameConnection::onClientEnterGame(%client)
	{
		%client.printc = Print_Create();
		%client.printb = Print_Create();
		return parent::onClientEnterGame(%client);
	}

	function GameConnection::onClientLeaveGame(%client)
	{
		%client.printc.delete();
		%client.printb.delete();
		return parent::onClientLeaveGame(%client);
	}
};
activatePackage("Game");

function PlayerData::onAdd(){}
function PlayerData::onRemove(){}
function PlayerData::onNewDataBlock(){}
function PlayerData::onMount(){}
function PlayerData::onUnMount(){}
function PlayerData::onCollision(){}
function PlayerData::onImpact(){}
function PlayerData::onDamage(){}
function PlayerData::onEnabled(){}
function PlayerData::onDisabled(){}
function PlayerData::onDestroyed(){}
function PlayerData::onEnterLiquid(){}
function PlayerData::onLeaveLiquid(){}
function PlayerData::onTrigger(){}
function PlayerData::onPickup(){}

function GameMinigame::addMember(%this, %client)
{
	parent::addMember(%this, %client);

	if($Game.Phase $= "")
	{
		//TODO: SPECTATING
		%client.printb.set("0","<just:left>\c4Spectating");
	}
	

	if(%this.numMembers > 1 && !$Game.Running)
	{
		$Game.PreStart();
	}
}

function GameMinigame::checkLastManStanding(%this){}

function GameMinigame::removeMember(%this, %client)
{
	parent::removeMember(%this, %client);

	if(%this.numMembers < 2)
	{
		$Game.End();
	}
}

function GameMinigame::reset(%mini, %client){}