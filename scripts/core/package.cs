// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot
// =================================================
// File Name: package.cs
// Description: Hooks for players, weapons, or other.
// =================================================
// Table of Contents:
//
// 1. Namespaceless
// 2. Armor
// 3. fxDTSBrick
// 4. GameConnection
// 5. MinigameSO
// 6. Player
// 7. ServerCMD
// =================================================

// =================================================
// 1. Namespaceless
// =================================================
function SimObject::onCameraEnterOrbit(%obj, %camera) {}
function SimObject::onCameraLeaveOrbit(%obj, %camera) {}

function Projectile::FuseExplode(%proj)
{
	%db = %proj.getDatablock();
	%vel = %proj.getVelocity();
	%pos = %proj.getPosition();
	%sObj = %proj.sourceObject;
	%sSlot = %proj.sourceSlot;
	%cli = %proj.client;

	%proj.delete();

	if(vectorLen(%vel) == 0)
		%vel = "0 0 0.1";
	
	%p = new Projectile()
	{
		dataBlock = %db;
		initialVelocity = %vel;
		initialPosition = %pos;
		sourceObject = %sObj;
		sourceSlot = %sSlot;
		client = %cli;
		sourceImage = %proj.sourceImage;
		sourceItem = %proj.sourceItem;
	};
	
	MissionCleanup.add(%p);

	%p.explode();
}

package BBB_Namespaceless
{
	//only pop items when we want them to
	function Item::schedulePop(%obj)
	{
		
		if($BBB::ItemPop)
		{
			Parent::schedulePop(%obj);
		}
		else
		{
			%obj.schedule($Game::Item::PopTime,"schedulePop");
		}
	}

	function Player::WeaponAmmoUse(%player)
	{
		%image = %player.getMountedImage(0);
		%player.lastBBBUsedImage = %image;
		%item = %player.tool[%player.currTool];
		 %player.lastBBBUsedItem = %item;
		parent::WeaponAmmoUse(%player);
	}

	function Projectile::onAdd(%projectile)
	{
		%return = Parent::onAdd(%projectile);
		if(isObject(%projectile))
		{
			%player = %projectile.sourceObject;


			if(isObject(%player))
			{
				if(%player.getClassName() $= "Player")
				{
					if(%projectile.sourceImage $= "")
					{
						%image = %player.getMountedImage(0);
						if(!isObject(%image))
						{
							%image = %player.lastBBBUsedImage;
						}
						%projectile.sourceImage = %image;
					}

					if(%projectile.sourceItem $= "")
					{
						%item = %player.tool[%player.currTool];
		
						if(!isObject(%item))
						{
							%item = %player.lastBBBUsedItem;
						}

						%projectile.sourceItem = %item;
					}
				}
			}
		}
		return %r;
	}
};
activatePackage(BBB_Namespaceless);
//overwrite so grenades inherit sourceimage
// =================================================
// 2. Armor
// =================================================
package BBB_Armor
{
	function Armor::Damage(%data, %victim, %hitter, %pos, %damage, %damageType)
	{
		Parent::Damage(%data, %victim, %hitter, %pos, %damage, %damageType);
		%client = %victim.client;
		if(!%client.inBBB)
			return;
		if(isObject(%victim))
		{
			%health = mCeil(100 - %victim.getDamageLevel());
			%healthText = "\c3" @ %health;
			BBB_TimerLoop_ForceUpdate(%client, "bottomPrint", 2, %healthText);

			if(%health > 80)
				%victim.setShapeNameColor("0 1 0");
			else if(%health > 60)
				%victim.setShapeNameColor("0.7 1 0.1");
			else if(%health > 40)
				%victim.setShapeNameColor("1 1 0");
			else if(%health > 20)
				%victim.setShapeNameColor("1 0.5 0");
			else
				%victim.setShapeNameColor("1 0 0");
		}
	}

	function Player::Pickup(%pl, %item)
	{
		if(%item == AE_AmmoItem.getID())
			return 1;
		
		Parent::Pickup(%pl, %item);
	}

	function Armor::onCollision(%this, %obj, %col, %vec, %speed)
	{
		if(isObject(%col))
		{
			%client = %obj.client;

			if(isObject(%client) && isObject(%obj) && isObject(AE_AmmoItem))
			{

				if((%col.getType() & $TypeMasks::ItemObjectType) && %col.getDatablock() != AE_AmmoItem.getID())
				{
					if(%obj.pickup(%col))
						return;
				}
				else
				{
					if(getSimTime() - %obj.lastAmmoPickupTime > 15000)
					{
						%data = %col.getDatablock();
						if(%data == AE_AmmoItem.getID())
						{
							if(%col.canPickup && %obj.getDamagePercent() < 1.0 && minigameCanUse(%obj, %col))
							{
								%col.spawnBrick = "";
								Parent::onCollision(%this, %obj, %col, %vel, %speed);
								
								if(%data == AE_AmmoItem.getID())
									%col.schedule(10, delete);
								%obj.lastAmmoPickupTime = getSimTime();
								return;
							}
						}
					}
					else
						return;
				}
			}
		}

		return parent::onCollision(%this, %obj, %col, %vec, %speed);
	}
	function Armor::onTrigger(%this, %obj, %trigger, %state)
	{

		%client = %obj.client;

		switch(%trigger)
		{
			// Fire
			case 0:
				if(%state)
				{
					if(!%obj.dropCorpse() && !isObject(%obj.getMountedImage(0)) && !isObject(%obj.meleeHand))
					{
						if(isObject(%corpse = %obj.findCorpseRayCast()))
						{
							// We use this instead of the function because if my specific unnecessary need for 'an' 'a' etc.

							messageClient(%client, '', "\c6[\c4Corpse Data\c6]");
							switch$(%corpse.role)
							{
								case "Detective":
									%rolePrint = "\c1DETECTIVE";
									%rolecolor = "\c1";
								case "Innocent":
									%rolePrint = "\c2INNOCENT";
									%rolecolor = "\c2";
								case "Traitor":
									%rolePrint = "\c0TRAITOR";
									%rolecolor = "\c0";
								default:
									%rolePrint = "???";
							}
							messageClient(%client, '', "\c6 > \c3Name\c6: " @ %corpse.name);
							messageClient(%client, '', "\c6 > \c3Role\c6: " @ %rolePrint);
							messageClient(%client, '', "\c6 > \c3Cause of death\c6: [" @ %corpse.SOD @ "\c6]");
							if(%corpse.deadTime !$= "")
							{
								%time = getSimTime() - %corpse.deadTime;
								%minutes = mFloor(%time / 60000);
								%seconds = mFloor((%time - %minutes * 60000) / 1000);
								if(%seconds < 10)
									%seconds = "0" @ %seconds;
								messageClient(%client, '', "\c6 - \c3Dead for\c6 " @ %minutes @ ":" @ %seconds @ "\c3 " @ (%minutes > 0 ? (%minutes > 1 ? "minutes" : "minute") : (%seconds > 1 || %seconds == 0 ? "seconds" : "second")) @ ".");
							}
							if(%corpse.lastWords !$= "")
								messageClient(%client, '', "\c6 - \c3Last Words: \c6\"" @ %corpse.lastWords @ "\"");
							if(%corpse.unIDed)
							{
								%corpse.displayName = %corpse.name @ "'s corpse";
								%corpse.setShapeName(%corpse.displayName, 8564862);
								//findclientbyname(lego).player.setShapeName("0", 8564862);
								%corpse.unIDed = false;
								if($BBB::Announce::BodyFound)
								{
									chatMessageAll("", "\c6" @ %obj.client.name SPC "\c4found\c6" SPC %corpse.displayName @ "\c4!" SPC "They were" @ (%corpse.role $= "Traitor" || %corpse.role $= "Detective" ? " a " : " ") @ %roleprint @ "\c4!");
									
									%client = findClientByName(%corpse.name);
									if(isObject(%client))
									{
										//mark them as dead in the player list
										secureCommandToAllTS("zbR4HmJcSY8hdRhr", 'ClientJoin',%rolecolor @ "[Dead]" SPC %client.getPlayerName(), %client, %client.getBLID (), %client.score, 0, %client.isAdmin, %client.isSuperAdmin);
									}
								}
							}

						}
					}
				}
				return parent::onTrigger(%this, %obj, %trigger, %state);

			// Jump
			case 2:
				return parent::onTrigger(%this, %obj, %trigger, %state);

			// Crouch
			case 3:
				return parent::onTrigger(%this, %obj, %trigger, %state);

			// Jet
			case 4:
				if(%state) // On Right Click
				{
					// %mounted = %obj.getMountedObject(0);
					// if(isObject(%mounted) && %mounted.corpse)
					// {
						// %mounted.dismount();
						// %mounted.addVelocity(vectorScale(%obj.getEyeVector(),10));
						// %obj.playThread(3,"root");
						// return;
					// }
					// else
					// {
						// %start = %obj.getEyePoint();
						// %targets = ($TypeMasks::FxBrickObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::VehicleObjectType);
						// %vec = %obj.getEyeVector();
						// %end = vectorAdd(%start,vectorScale(%vec,10));
						// %ray = containerRaycast(%start,%end,%targets,%obj);
						// %col = firstWord(%ray);
						// if(!isObject(%col))
							// return parent::onTrigger(%this, %obj, %trigger, %state);
						// if(!%col.corpse)
							// return parent::onTrigger(%this, %obj, %trigger, %state);

						// At this point we know its a corpse.
						// %obj.mountObject(%col,0);
						// %col.setTransform("0 0 0 0 0 -1 -1.5709");
						// %obj.playThread(3,"ArmReadyBoth");
						// return;
					// }

					if (!%obj.throwCorpse()) //No corpse to throw, try grabbing one instead
					{
						if (isObject(%corpse = %obj.findCorpseRayCast()))
						{
							%obj.grabCorpse(%corpse);
						}
					}
				}

				return parent::onTrigger(%this, %obj, %trigger, %state);

			// Anything else.
			default:
				return parent::onTrigger(%this, %obj, %trigger, %state);
		}

		return parent::onTrigger(%this, %obj, %trigger, %state);
	}

	// Playertype armor
	function BBB_Standard_Corpse_Armor::onUnMount(%this, %obj, %mount, %slot)
	{
		Parent::onUnMount(%this, %obj, %mount, %slot);
		if (!isObject(%mount.heldCorpse) || %mount.heldCorpse != %obj)
			return;
		%mount.heldCorpse = "";
		%mount.playThread(2, "root");
	}
};
activatePackage(BBB_Armor);
// =================================================
// 3. fxDTSBrick
// =================================================
package BBB_fxDTSBrick
{
	function fxDTSBrick::onActivate(%this, %obj, %client)
	{
		if(%client.role $= "Traitor")
			%this.onTraitorActivate(%client);
		else
			%this.onInnocentActivate(%client);
		return parent::onActivate(%this, %obj, %client);
	}

	function fxDTSBrick::onPlayerTouch(%this, %obj)
	{
		%client = %obj.client;
		if(isObject(%client))
		{
			if(%client.role $= "Detective" || %client.role $= "Innocent")
				%this.onInnocentTouch(%obj);
			else if(%client.role $= "Traitor")
				%this.onTraitorTouch(%obj);
		}

		if(%obj.isBody)
			%this.onCorpseTouch(%obj);

		return parent::onPlayerTouch(%this, %obj);
	}
};
activatePackage(BBB_fxDTSBrick);
// =================================================
// 4. GameConnection
// =================================================
//overwrite
// All the corpse code is made by Jack Noir
function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	//if (%client.miniGame != $DefaultMiniGame)
	//	return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	if(!%client.inBBB || $BBB::Round::Phase !$= "Round")
	{
		%client.setcontrolObject(%client.camera);
		return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	if (%sourceObject.sourceObject.isBot)
	{
		%sourceClientIsBot = 1;
		%sourceClient = %sourceObject.sourceObject;
	}

	%client.play2D(BBB_Death_Sound);

	%role = %client.role;
	//$BBB::Round::NumAlive[%role]--;
	if(%role $= "Innocent" || %role $= "Detective")
		$BBB::rTimeLeft += $BBB::Time::Bonus;

	%player = %client.player;

	if(isObject(%player))
	{
		if (isObject(%player.tempBrick))
		{
			%player.tempBrick.delete();
			%player.tempBrick = 0;
		}

		%player.changeDatablock(BBB_Standard_Corpse_Armor); //Doing this before client is nullified is important for appearance stuff
		%player.playDeathAnimation(); //...still call this because datablock switch fucks with anims

		%player.BBB_ApplyOutfit($BBB::Outfit);
		%player.displayName = "an Unidentified Body";
		%player.setShapeName("Unidentified Body", 8564862);
		%player.setShapeNameDistance(13);
		%player.setShapeNameColor("1 1 0");
		%player.isBody = true;
		%player.client = 0;
		%player.origClient = %client;
		%player.credits = %client.credits;

		// BBB Corpse data
		%player.name = %client.name;
		%player.role = %client.role;
		%player.deadTime = getSimTime();
		%player.fingerPrints = %sourceClient.player;
		if(%player.fingerPrints $= "")
		{
			%player.fingerPrints = %sourceClient.corpse;
		}
		
		%s = "???";

		if(%damageType == $DamageType::Suicide)
			%s = "Suicide";
		else if(%damageType == $DamageType::Fall)
			%s = "Broken leg";
		else if(%item = %sourceObject.sourceItem)
		{
			%s = %item.uiname;	
		}
		else if(%sourceObject.getClassName() $= "GameConnection")
		{
			%sourceplayer = %sourceObject.player;
			%item = %sourceplayer.tool[%sourceplayer.currTool];
			%s = %item.uiname;
		}

		%player.SOD = %s;//getWord(getTaggedString($DeathMessage_Murder[%damageType]), 1);
		if(%player.deadTime - %player.lastMsgTime < 2000)
			%player.lastWords = %player.lastMsg;
		%player.unIDed = true;

		if (!isObject(CorpseGroup))
			new SimSet(CorpseGroup);

		CorpseGroup.add(%player);
	}
	else
		warn("WARNING: No player object in GameConnection::onDeath() for client '" @ %client @ "'");

	if (isObject(%client.camera) || isObject(%player))
	{ // this part of the code isn't accurate
		if (%client.getControlObject() != %client.camera)
		{
			%client.camera.setMode("Corpse", %player);
			%client.camera.setControlObject(0);
			if(%client.getControlObject().getDatablock().getName() !$= "BillboardLoadingCamera")
	  		{
				%client.setControlObject(%client.camera);
			}
		}
	}

	%client.player = 0;

	%client.corpse = %player;
	
	%client.tpschecked = false;

	if ($Damage::Direct[$damageType] && getSimTime() - %player.lastDirectDamageTime < 100 && %player.lastDirectDamageType !$= "")
		%damageType = %player.lastDirectDamageType;

	if (%damageType == $DamageType::Impact && isObject(%player.lastPusher) && getSimTime() - %player.lastPushTime <= 1000)
		%sourceClient = %player.lastPusher;

	%message = '%2 killed %1';

	if (%sourceClient == %client || %sourceClient == 0)
	{
		%message = $DeathMessage_Suicide[%damageType];
		%client.corpse.suicide = true;
	}
	else
		%message = $DeathMessage_Murder[%damageType];

	// removed mid-air kills code here
	// removed mini-game kill points here

	%clientName = %client.getPlayerName();

	if (isObject(%sourceClient))
		%sourceClientName = %sourceClient.getPlayerName();
	else if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
		%sourceClientName = %sourceObject.sourceObject.name;
	else
		%sourceClientName = "";

	%client.print = "<just:left><font:Palatino Linotype:22><font:Palatino Linotype:45><color:808080>D<font:Palatino Linotype:43><color:808080>EAD";

	%colr = "ffffff";
	if(%sourceClient.role $= "Traitor")
		%colr = "ff7744";
	else if(%sourceClient.role $= "Innocent")
		%colr = "44ff44";
	else if(%sourceClient.role $= "Detective")
		%colr = "4477ff";
	
	%client.chatMessage("<font:Palatino Linotype:22><color:494949>You were killed by <color:" @ %colr @ ">" @ %sourceClient.name @ ", a(n)" SPC %sourceClient.role @ "<color:494949>.");

	//deathlogs//

	if($BBB::Round::Phase $= "Round")
	{
		%dlmsg = "<color:" @ (%sourceclient.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" @ %sourceclient.name SPC "(" @ %sourceclient.role 
		@ ") \c6killed<color:" @ (%client.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" SPC %client.name SPC "(" @ %client.role @ ")\c6 at" SPC getStringFromTime($BBB::rTimeLeft);

		if(!isObject(%sourceclient))
			%dlmsg = "<color:" @ (%cl.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" @ %client.name SPC "(" @  %client.role @ ")" SPC "died.";
		
		if(%sourceclient == %client)
			%dlmsg = "<color:" @ (%cl.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" @ %client.name SPC "(" @  %client.role @ ")" SPC "suicided.";

		$DeathLog[$DeathLogCount] = %dlmsg;
		$DeathLogCount++;
		if(%sourceClient.killLogCount $= "")
			%sourceClient.killLogCount = 0;
		
		%scr = %sourceClient.role;
		%cr = %client.role;
		if(%cr $= "Detective")
			%cr = "Innocent";
		if(%scr $= "Detective")
			%scr = "Innocent";

		if(%sourceClient.killLogCount $= "")
			%sourceClient.killLogCount = 0;
		%sourceClient.killLog[%sourceClient.killLogCount] = %dlmsg TAB (%scr $= %cr);
		%sourceClient.killLogCount++;

		if(%client.killLogCount $= "")
			%client.killLogCount = 0;
		%client.killLog[%client.killLogCount] = %dlmsg TAB (%scr $= %cr);
		%client.killLogCount++;

		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%cc = ClientGroup.getObject(%i);
			if(%cc.isAdmin)
			{
				if(%cc.getDamagePercent >= 1.0 || !%cc.inBBB)
				{
					chatMessage(%cc, '', %dlmsg);
				}
			}
		}
	}

	//the end//

	//traitor/detective rewards
	//percentage dead reward
	//get the current percent of dead innocents
	%mini = BBB_Minigame;
	%count = %mini.numPlayers;
	%totalInno = 0;
	%deadInno = 0;
	for(%i = 0; %i < %count; %i++)
	{
		%currClient = %mini.players[%i];
		if(%currClient.role $= "Innocent")
		{
			%totalInno++;
			//are they dead?
			if(!isObject(%currClient.player))
			{
				%deadInno++;
			}
		}
	}
	%percentDead = %deadInno / %totalInno;

	//is it reward time?
	if(%percentDead > $BBB::Traitor::AwardPercent + $BBB::Round::AwardPercentOffset)
	{
		//reward all traitors the ammount
		for(%i = 0; %i < %count; %i++)
		{
			%currClient = %mini.players[%i];
			if(%currClient.role $= "Traitor")
			{
				//are they dead?
				if(isObject(%currClient.player))
				{
					%currClient.chatMessage("\c6Well done. You have been awarded\c3" SPC $BBB::Traitor::AwardSize SPC "Credit\c6 for your hard work.");
					%currClient.credits += $BBB::Traitor::AwardSize;
				}
			}
		}

		$BBB::Round::AwardPercentOffset += $BBB::Traitor::AwardPercent;
	}

	//did a traitor kill the detective?
	if(%client.role $= "Detective" && %sourceClient.role $= "Traitor")
	{
		%sourceClient.chatMessage("\c6Well done. You have been awarded\c3" SPC $BBB::Traitor::DetectiveKill SPC "Credit\c6 for your hard work.");
		%sourceClient.credits += $BBB::Traitor::DetectiveKill;
	}

	//did a traitor die?
	if(%client.role $= "Traitor")
	{
		//reward all detectives the ammount
		for(%i = 0; %i < %count; %i++)
		{
			%currClient = %mini.players[%i];
			if(%currClient.role $= "Detective")
			{
				//are they dead?
				if(isObject(%currClient.player))
				{
					%currClient.chatMessage("\c6Well done. You have been awarded\c3" SPC $BBB::Detective::TraitorDead SPC "Credit\c6 for your hard work.");
					%currClient.credits += $BBB::Detective::TraitorDead;
				}
			}
		}
	}

	echo("\c4" SPC %sourceClientName SPC "(" @ %sourceClient.role @ ") killed" SPC (%clientName $= %sourceClientName ? "themselves" : %clientName @ "(" @ %client.role @ ")"));
	// removed mini-game checks here
	// removed death message print here
	// removed %message and %sourceClientName arguments
	messageClient(%client, 'MsgYourDeath', '', %clientName, '', %client.miniGame.respawnTime);
	//commandToClient(%client, 'CenterPrint', "", 1);
	BBB_Minigame.doWinCheck();
}

package BBB_GameConnection
{
	function GameConnection::unmountAllHats(%cl) {
		for(%i = 0; %i < 4; %i++)
			%cl.dismountHat(%i);
		
		if(isObject(%pl = %cl.player))
			cancel(%pl.hvisloop);
}

	function GameConnection::applyBodyParts(%client)
	{
		if(!%client.inBBB || !$BBB::Round::Active)
			return parent::applyBodyParts(%client);
		else
		{
			if(isObject(%player = %client.player))// && isObject(%image = %player.getMountedImage(2)) && %image.hatName !$= "")
			{
				if(isObject(%bot = %player.hatBot))
				{
					for(%i = 0; %i < 4; %i++)
					{
						if(isObject(%image = %bot.getMountedImage(%i)))
						{
							if ($HatMod::Nodes::showHat[%image.hatName] == false)
							{
								for(%u=0; $hat[%u] !$= ""; %u++)
									%player.hideNode($hat[%u]);
								
								for(%u=0; $accent[%u] !$= ""; %u++)
									%player.hideNode($accent[%u]);
							}
							
							if ((%list = $HatMod::Nodes::hiddenNodes[%image.hatName]) !$= "")
							{
								for (%u=0; getWord(%list,%u) !$= ""; %u++)
								{
									%player.hideNode(getWord(%list,%u));
								}
							}
						}
					}
				}
			}
		}
	}

	function GameConnection::applyBodyColors(%client)
	{
		if(!%client.inBBB || !$BBB::Round::Active)
			return parent::applyBodyColors(%client);
	}

	function GameConnection::onClientEnterGame(%client)
	{
		parent::onClientEnterGame(%client);
		if(%client.isSuperAdmin)
			%client.icon = "\c2[<color:FF7744>S-ADMIN\c2]";
		else if(%client.isAdmin)
			%client.icon = "\c2[<color:FF7744>ADMIN\c2]";
	}

	// function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damageArea)
	// {
		// commandToClient(%client, 'clearCenterPrint');
		// if(!%client.inBBB || $BBB::Round::Phase !$= "Round")
		// {
			// Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damageArea);
			// return;
		// }

		// %role = %client.role;
		// $BBB::Round::NumAlive[%role]--;

		// %obj = %client.player;

		// if(isObject(%obj))
		// {
			// %obj.playThread(2, death1);
			// %obj.setShapeName("", 0);
			// %obj.getDataBlock().onDisabled(%obj);

			// %obj.corpse = true;

			// %time = $Sim::Time;
			// %minutes = mFloor(%time / 60000);
			// %seconds = mFloor((%time - %minutes * 60000) / 1000);
			// if(%seconds < 10)
				// %seconds = "0" @ %seconds;

			// %obj.deadTime = %minutes @ ":" @ %seconds;
			// %obj.fingerPrints = %sourceObject;
			// %obj.SOD = %damageType;

			// %time = $Sim::Time;
			// %minutes = mFloor(%time / 60000);
			// %seconds = mFloor((%time - %minutes * 60000) / 1000);
			// if(%seconds < 10)
				// %seconds = "0" @ %seconds;

			// %client.corpse = new AIPlayer(botCorpse)
			// {
				// datablock = PlayerNoJet;
				// corpse=1;
				// owner=%this;
				// deadTime = getSimTime();
				// fingerPrints = %sourceObject;
				// SOD = getWord(getTaggedString($DeathMessage_Murder[%damageType]), 1);
			// };
			// missionCleanup.add(%client.corpse);

			// Apply body colors
			// %client.player = %client.corpse;
			// %client.applyBodyParts();
			// %client.applyBodyColors();
			// %client.player = %obj;

			// %client.corpse.setTransform(%obj.getTransform());
			// %client.corpse.addVelocity(%obj.getVelocity());
			// %client.corpse.playDeathCry();

			// %client.corpse.playThread(3,"death1");
		// }

		// %client.camera.setMode("Corpse",%client.corpse);
		// %client.camera.setControlObject(0);
		// %client.setControlObject(%client.camera);

		// %client.print = "<just:left><font:Palatino Linotype:22><font:Palatino Linotype:45><color:808080>D<font:Palatino Linotype:43><color:808080>EAD";

		// %obj.setTransform(9999 SPC 9999 SPC 10000);
		// %obj.removeBody();
		// BBB_Minigame.doWinCheck();
	// }

	function GameConnection::spawnPlayer(%this)
	{
		// %mini = getMiniGameFromObject(%this);
		// if(%mini.isBBB)
		// {
			// if($BBB::Round::Phase $= "Round" || $BBB::Round::Phase $= "PostRound")
				// return;
		// }
		if(isObject(%this.corpse))
			%this.corpse.delete();
		Parent::spawnPlayer(%this);
		if($BBB::Round::Active)
			%this.player.BBB_applyOutfit($BBB::Outfit);
	}

	// function GameConnection::startTalking(%this)
	// {
		// %obj = %this.player;
		// if(%obj.corpse)
			// return;
		// Parent::startTalking(%this);
	// }
};
activatePackage(BBB_GameConnection);

// =================================================
// 5. MinigameSO
// =================================================
package BBB_MinigameSO
{
	function MinigameSO::addMember(%this, %client)
	{
		parent::addMember(%this, %client);

		if(!%this.isBBB)
			return;

		%client.inBBB = true;

		if(!%client.inBBB || $BBB::Round::Phase !$= "Round")
			%client.print = $BBB::GlobalPrint;
		else
			%client.print = "<just:left><font:Palatino Linotype:22><font:Palatino Linotype:45>\c3S<font:Palatino Linotype:43>\c3PECTATING";

		if(!$BBB::Round::Active || $BBB::Round::Active $= "" )
		{
			if(%this.numMembers > 1)
				BBB_Minigame.roundSetup();
			else
				BBB_Minigame.cleanUp();
		}
	}

	function MinigameSO::checkLastManStanding(%this)
	{
		if(%this.isBBB)
			return;
		parent::checkLastManStanding(%this, %client);
	}

    function MinigameSO::removeMember(%this, %client)
    {
        parent::removeMember(%this, %client);

        if(!%this.isBBB)
            return;

        if($BBB::Round::Active)
        {
            %playerCount = BBB_Minigame.numMembers;

            if(%this.numMembers < 2)
                BBB_Minigame.cleanUp();
            else
            {
                if($BBB::Round::Phase $= "Round")
                    BBB_Minigame.doWinCheck();
            }
        }
    }

	// Credit jes00 - https://forum.blockland.us/index.php?topic=243057.0#post_ClearItemsOnReset
	function MiniGameSO::reset(%mini, %client)
	{
		parent::reset(%mini, %client);
		if(!%mini.isBBB)
			return;

		%count = MissionCleanup.getCount() - 1;

		if(%count >= 0)
		{
			for(%i = %count; %i >= 0; %i--)
			{
				%obj = MissionCleanup.getObject(%i);

				if(%obj.getClassName() $= "Item" && %obj.miniGame == %mini)
				{
					%obj.delete();
				}
			}
		}
	}
};
activatePackage(BBB_MinigameSO);

// =================================================
// 6. Player
// =================================================
package BBB_Player
{
	function Player::setHats(%pl)
	{
		if(!isObject(%cc = $BBB::Outfit::Owner))
			return;
		if(isObject(%cl = %pl.client) && isObject(%pl))
		{
			if(!%pl.isCorpse)
				%cl.unMountAllHats();

			for(%i=0;%i<4;%i++){
				%img = %cc.superhat[%i];
				if(isHat(%img))
					%pl.mountHat(%img,%i);
			}

			if(!%pl.isCorpse)
			{
				%pl.ghostingHatBot = true;
				%pl.hatVisLoop();
			}
			
			%cl.applyBodyParts();
			%cl.applyBodyColors();
		}

		return;
	}
	// Some corpse code taken from MARBLE MAN's corpse mod.
	// function Player::activateStuff(%this)
	// {
		// Parent::activateStuff(%this);

		// %client = %this.client;

		// if(isObject(%this.getMountedObject(1)))
			// return Parent::activateStuff(%this);

		// %mounted = %this.getMountedObject(0);
		// if(isObject(%mounted) && %mounted.corpse)
		// {
		  // %mounted.dismount();
		  // %mounted.addVelocity(vectorScale(%this.getEyeVector(),10));
		  // %this.playThread(3,"root");
		// }
		// else
		// {
			// %start = %this.getEyePoint();
			// %targets = ($TypeMasks::FxBrickObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::VehicleObjectType);
			// %vec = %this.getEyeVector();
			// %end = vectorAdd(%start,vectorScale(%vec,10));
			// %ray = containerRaycast(%start,%end,%targets,%this);
			// %col = firstWord(%ray);
			// if(!isObject(%col))
				// return Parent::activateStuff(%this);
			// if(!%col.corpse)
				// return Parent::activateStuff(%this);
			// At this point we know its a corpse.
			// Double click to pick up, 1000ms window.
			// if(getSimTime() - %col.lastClicked[%this] < 250)
			// {
				// cancel(%client.corpseClickSched);
				// %this.mountObject(%col,0);
				// %col.setTransform("0 0 0 0 0 -1 -1.5709");
				// %this.playThread(3,"ArmReadyBoth");

			// }
			// else // Otherwise, let's print the corpse data.
			// {
				// cancel(%client.corpseClickSched);
				// %client.corpseClickSched = schedule(250, %client, "BBB_PrintCorpseData", %client, %col);
				// %time = getSimTime() - %col.deadTime;
				// %minutes = mFloor(%time / 60000);
				// %seconds = mFloor((%time - %minutes * 60000) / 1000);
				// if(%seconds < 10)
					// %seconds = "0" @ %seconds;
				// messageClient(%client, '', "\c6[\c4Corpse Data\c6]");
				// messageClient(%client, '', "\c6 - \c3Cause of death\c6: [" @ %col.SOD @ "\c6]");
				// messageClient(%client, '', "\c6 - \c3Dead for\c6: " @ %minutes @ ":" @ %seconds);
			// }
			// %col.lastClicked[%this] = getSimTime();
		// }
	// }

	function Player::mountImage(%this, %image, %slot, %loaded, %skintag)
	{
		parent::mountImage(%this, %image, %slot, %loaded, %skintag);
		if (%slot == 0 && isObject(%this.heldCorpse))
			%this.throwCorpse();
	}

	function Player::removeBody(%this)
	{
		if(%this.isBody)
			return;

		if (isObject(%this.origClient))
			%this.origClient.corpse = "";
		if (isObject(%this.holder))
		{
			%this.holder.heldCorpse = "";
			%this.holder.playThread(2, "root");
		}
		return Parent::removeBody(%this);
	}
		// parent::RemoveBody(%this);
	// }


};
activatePackage(BBB_Player);


// =================================================
// 7. ServerCMD
// =================================================
package BBB_ServerCMD
{
	function ServerLoadSaveFile_End()
	{
		Parent::ServerLoadSaveFile_End();
		if(BBB_Minigame.numMembers > 1)
			BBB_Minigame.roundSetup();
		else
			BBB_Minigame.cleanUp();
	}

	function serverCmdMessageSent(%client, %msg)
	{
		if(!$BBB::Round::Active)
			return parent::serverCmdMessageSent(%client, %msg);

		if(%msg $= "")
		{
			return;
		}

		%msg = trim(StripMLControlChars(%msg));

		%msg = strreplace(%msg, "https://", "http://");

		if(strpos(%msg, "http://") == 0)
		{
			%msg = getSubStr(%msg, 7, strlen(%msg) - 7);
			%link = firstWord(%msg);
			%rest = restWords(%msg);

			%msg = trim("<a:" @ %link @ ">" @ %link @ "</a>" SPC %rest);
		}

		%mg = BBB_Minigame;
		// Regular Chat
		if(isObject(%client.player))
		{
			// for(%i = 0; %i < %mg.numMembers; %i++)
			// {
				// %tarClient = %mg.member[%i];
				// messageClient(%tarClient, "", %client.Icon SPC "<color:9EE09C>" @ %client.getPlayerName() @ "\c6: " @ %msg);
			// }
			//messageAll("", %client.Icon SPC "<color:9EE09C>" @ %client.getPlayerName() @ "\c6: " @ %msg);
			%type = %client.role $= "Detective" ? "<color:7DD4FF>" : "<color:9EE09C>";
			%icon = $BBB::Round::Phase $= "Round" ? "" : %client.Icon;
			//messageAll("", ($BBB::Round::Phase $= "Round" ? "" : %client.Icon) @ " " @ (%client.role $= "Detective" ? "<color:7DD4FF>" : "<color:9EE09C>") @ %client.getPlayerName() @ "\c6: " @ %msg);
			chatMessageAll (%client, '%5%6%2\c6: %4', %client.clanPrefix, %client.getPlayerName(), %client.clanSuffix, %msg, %type, %icon, %a7, %a8, %a9, %a10);
			%client.player.lastMsg = %msg;
			%client.player.lastMsgTime = getSimTime();

			%mg.playGlobalSound(BBB_Chat_Sound);
		}
		else // Dead Chat
		{
			%type = "\c6[" @ (%client.hasSpawnedOnce == 1 ? "DEAD" : "LOADING") @ "]";
			if($BBB::Round::Phase !$= "Round")
			{
				chatMessageAll (%client, '%5%6\c4%2<color:DDDDDD>: %4', %client.clanPrefix, %client.getPlayerName(), %client.clanSuffix, %msg, %type, %client.icon, %a7, %a8, %a9, %a10);
				//messageAll("", %send);
				%mg.playGlobalSound(BBB_Chat_Sound);
			}
			else
			{
				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%tarClient = ClientGroup.getObject(%i);
					%player = %tarClient.player;
					if(!isObject(%player))
					{
						chatMessageClient (%tarClient, %client,'','' ,'%5%6\c4%2<color:DDDDDD>: %4', %client.clanPrefix, %client.getPlayerName(), %client.clanSuffix, %msg, %type, %client.icon, %a7, %a8, %a9, %a10);
						//messageClient(%tarClient, "", %send);
						%tarClient.play2D(BBB_Chat_Sound);
					}
				}
			}
		}

		echo(%client.getPlayerName() @ ": " @ %msg);
	}

	function serverCmdTeamMessageSent(%client, %msg)
	{
		if(!%client.inBBB)
			return parent::serverCmdTeamMessageSent(%client, %msg);

		%obj = %client.player;

		if(!isObject(%obj))
		{
			serverCmdMessageSent(%client, %msg);
			return;
		}

		if(%msg $= "")
		{
			return;
		}

		%msg = StripMLControlChars(%msg);

		%msg = strreplace(%msg, "https://", "http://");

		if(strpos(%msg, "http://") == 0)
		{
			%msg = getSubStr(%msg, 7, strlen(%msg) - 7);
			%link = firstWord(%msg);
			%rest = restWords(%msg);

			%msg = trim("<a:" @ %link @ ">" @ %link @ "</a>" SPC %rest);
		}

		%mg = BBB_Minigame;

		if(%client.role $= "Traitor" || %client.role $= "Detective")
		{
			if(%client.role $= "Traitor")
			{
				%color = "\c0";
				%chatColor = "<color:FFD0D0>";
			}
			else
			{
				%color = "\c1";
				%chatColor = "<color:D0D0FF>";
			}
			for(%i = 0; %i < %mg.numMembers; %i++)
			{
				%tarClient = %mg.member[%i];
				%player = %tarClient.player;
				if(isObject(%player))
				{
					if(%tarClient.role $= %client.role)
					{
						%type = "\c7[" @ %color @ %client.role @ "\c7] ";
						%icon = %color;
						chatMessageClient (%tarClient, %client,'','' , '%5\c4%2%7: %4', %client.clanPrefix, %client.getPlayerName(), %client.clanSuffix, %msg, %type, %client.icon, %chatColor, %a8, %a9, %a10);
						//messageClient(%tarClient, "", "\c7[" @ %color @ %client.role @ "\c7] " @ %color @ %client.getPlayerName() @ %chatColor @ ": " @ %msg);
						%tarClient.play2D(BBB_Chat_Sound);
					}
				}
			}
		}
		else
		{
			serverCmdMessageSent(%client, %msg);
		}
	}



	function serverCmdShiftBrick(%this, %x, %y, %z)
	{
		if(%this.inBBB && isObject(%this.player) && $Sim::Time - %this.player.lastBBBmsg > 1)
		{
			%target = %this.player.BBB_TargetAPlayer();
			%this.player.lastBBBmsg = $Sim::Time;
			if(%z == -1) // numpad 1
				serverCmdMessageSent(%this, "Yes.");
			if(%x == -1) // numpad 2
				serverCmdMessageSent(%this, "No.");
			if(%z == 1) // numpad 3
				serverCmdMessageSent(%this, "Help!");
			if(%y == 1) // numpad 4
				serverCmdMessageSent(%this, "I'm with" SPC %target @ ".");
			if(%z == -3) // numpad 5
				serverCmdMessageSent(%this, "I see" SPC %target @ ".");
			if(%y == -1) // numpad 6
				serverCmdMessageSent(%this, %target SPC "acts suspicious.");
			if(%x == 1) // numpad 8
				serverCmdMessageSent(%this, %target SPC "is innocent.");
			return;
		}
		parent::serverCmdShiftBrick(%this, %x, %y, %z);
	}
	function serverCmdStartTalking(%client)
	{
		if($BBB::Round::Phase !$= "Round")
			Parent::serverCmdStartTalking(%client);
	}

	function serverCmdRotateBrick(%this, %dir)
	{
		if(%this.inBBB && isObject(%this.player) && $Sim::Time - %this.player.lastBBBmsg > 1)
		{
			%target = %this.player.BBB_TargetAPlayer();
			%this.player.lastBBBmsg = $Sim::Time;
			if(%dir == -1) // numpad 7
				serverCmdMessageSent(%this, %target SPC "is a Traitor!");
			if(%dir == 1) // numpad 9
				serverCmdMessageSent(%this, "Anyone still alive?");
			return;
		}
		parent::serverCmdRotateBrick(%this, %dir);
	}

	// For some reason this doesn't work properly??
	// function serverCmdUnUseTool(%client)
	// {
		// parent::serverCmdUnUseTool(%client);

		// if(isObject(%obj = %client.player))
			// %obj.unMountImage(0);

	// }
};
activatePackage(BBB_ServerCMD);
