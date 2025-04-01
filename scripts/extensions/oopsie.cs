
// The Oopsie system:
// You have 3 oopsies which is a currency you can spend
// If you run out of oopsies you will at the begining of next round be slayed for the deficit plus 1 and return to 1 oopsie
// If you spend 3 oopsies in 1 round you will be slain for 3 rounds and return to 1 oopsie

// Validness is hidden from players to prevent meta gaming
// Number of oopsies is also hidden during game to prevent meta gaming

// A player's hard validation can be demoted to soft validation if they lose line of sigh
// A soft valid or hard valid player must be called out before becoming hard valid permanantly
// They will only become hard valid to the person who calls them out

// If a player loses an oopsie they can be refunded by the victim if the victim chooses so
// If both players lose an oopsie the victim can forgive to return both of their oopsies
// If both players lose an oopsie the victim can revenge to petition the server to return their oopsie

// You regain a oopsie up to 3 whenever you win a round without losing an oopsie

// Situations:
// Player A is a traitor
// Player B is an innocent
// Player C is an innocent
// A kills B making a kill
// Traitors are allowed to kill innocents and detectives without validation
// C sees A killing B making A hard valid to C 
// C kills A making a valid kill
// C becomes soft valid

// Player A is a traitor
// Player B is an innocent
// A shoots at B but doesn't kill them
// A becomes hard valid to B
// B kills A making a hard valid kill
// B becomes soft valid

// Player A is a traitor
// Player B is an innocent
// A shoots at B but doesn't kill them
// A becomes hard valid to B
// A loses line of sight with B demoting valididity to soft
// B kills A making a soft valid kill
// Before A died they became hard valid with B
// This causes both A and B to lose an oopsie
// B becomes soft valid

function servercmdsetOopsies(%client,%a,%b,%c,%d,%e,%f,%g)
{
	if(!%client.isSuperAdmin)
	{
		return;
	}

	%input = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %g);
	%name = getWords(%input,0,getWordCount(%input)-2);
	%amount = getWord(%input,getWordCount(%input)-1);

	if(%amount $= "")
	{
		%client.chatMessage("Invalid ammount");
		return;
	}

	%target = findClientByName(%name);
	if(!isObject(%target))
	{
		%client.chatMessage("Invalid name");
		return;
	}

	%target.dataInstance($TTT::Data).oopsies = %amount;
	%client.chatMessage("You have set" SPC %target.getPlayerName() @ "'s oopsies to" SPC %amount);
}

function Oopsies_AdminNotification(%msg)
{
	%group = ClientGroup.getId();
	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);
		if(!%client.isAdmin)
		{
			continue;
		}

		%client.ChatMessage(%msg);
	}
}

function Oopsies_EndRound()
{
	%minigame = BBB_Minigame;
	%group = ClientGroup;
	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getobject(%i);

		if(!isObject(%client) || getMiniGameFromObject(%client) != %minigame)
		{
			continue;
		}

		%amount = %client.dataInstance($TTT::Data).oopsies;
		
		if(%minigame.numPlayers <= 4)
		{
			%amount = %client.dataInstance($TTT::Data).oopsies = 3;
			%client.chatMessage("Free oopsie rounds!");
		}
		else 
		{
			if(%client.lostOopsies !$= "")
			{
				%client.chatMessage("You lost" SPC %client.lostOopsies SPC "oopsies :(");
				Oopsies_AdminNotification(%client.getPlayername() SPC "lost" SPC %client.lostOopsies SPC "oopsies. They are now at" 
				SPC %client.dataInstance($TTT::Data).oopsies @ ".");
				%client.lostOopsies = "";
			}

			if(!%client.dataInstance($TTT::Data).oopsiesChange && %amount < 3)
			{
				%client.chatMessage("You gained an oopsie!");
				%amount = %client.dataInstance($TTT::Data).oopsies = %amount + 1;
			}

			if(%amount <= 0)
			{
				%client.slayed = 2;
				
				%message = '%1 is out of oopsies. They have a balance of %2 oopsies.';
				messageAll('', %message, %client.getPlayerName(),%amount);

				%player = %client.player;
				if(isObject(%player))
				{
					%player.kill();
				}
			}
			else
			{
				%client.chatMessage("You have" SPC %amount SPC "oopsies (" @ %amount / 2 @ " RDM credits) left. Good luck!");
			}

			if(%amount == 1 && %client.slayed > 0)
			{
				%client.slayed = 1;
			}
		}

		%client.dataInstance($TTT::Data).oopsiesChange = false;
		%client.roundOopsies = 0;
	}
}

$KillType::Valid = 0;
$KillType::Invalid = 1;
$KillType::CriminalInvalid = 2;
$KillType::Uknown = 3;
function Oopsies_KillCheck(%client,%targetclient)
{
	Oopsies_AdminNotification(%client.getPlayerName() SPC "killed" SPC %targetclient.getPlayername() SPC %targetclient.player.validStateFor[%client.player]  SPC %client.player.validStateFor[%targetClient.player]);
	if( $BBB::Round::Phase !$= "Round")
	{
		return;
	}

	%player = %client.player;
	%targetplayer = %targetclient.player;
	if(!%client.winCondition.isMiskill(%targetclient.winCondition))
	{
		return;
	}

	if(%client == %targetclient)
	{
		%client.AddOopsies(-1);
		return;
	}

	//invalid
	if(%targetPlayer.isValidState(%player,$ValidState::Invalid))
	{
		%client.AddOopsies(-3);
		return;
	}

	//the player who was latest to be criminal loses an oopsie
	%soonest = %player.getSoonestCriminal(%targetplayer);
	if(%soonest !$= "")
	{
		Oopsies_AdminNotification(%soonest.client.getPlayerName());
	}

	if(%soonest == %targetplayer)
	{
		%client.AddOopsies(-1);
		return;
	}

	if(%targetPlayer.isValidState(%player,$ValidState::Baiting))
	{
		%client.AddOopsies(-1);
		return;
	}

	Oopsies_AdminNotification("an uknown oopsie event occured");
}

function GameConnection::AddOopsies(%client,%amount)
{
	%client.dataInstance($TTT::Data).oopsies += %amount;
	%client.dataInstance($TTT::Data).oopsiesChange = true;

	if(%amount < 0)
	{
		%client.lostOopsies -= %amount;
	}

	if(%amount < 0 && !%client.slayed)
	{
		%client.roundOopsies += %amount;

		if(%client.roundOopsies <= -6 && BBB_Minigame.numPlayers > 4)
		{
			messageAll('MsgAdminForce', '%1 went on an oopsie driven rampage.', %client.getPlayerName());

			%client.slayed = 2;

			%player = %client.player;
			if(isObject(%player))
			{
				%player.kill();
			}
		}
	}
}

function GameConnection::SetOopsies(%client,%amount)
{
	%client.dataInstance($TTT::Data).oopsies = %amount;
	%client.dataInstance($TTT::Data).oopsiesChange = true;
}

$ValidState::Invalid = 0;
$ValidState::Baiting = 1;
$ValidState::Previously = 2;
$ValidState::Callout = 3;
$ValidState::CriminalInvisible = 4;
$ValidState::Criminal = 5;
$ValidState::CriminalCallout = 6;

function Player::SetValidState(%player,%target,%state)
{
	if( $BBB::Round::Phase !$= "Round")
	{
		return;
	}

	%oldState = getWord(%player.validStateFor[%target],0);
	if(%oldState == %state)
	{
		if(%state == $ValidState::CriminalInvisible)
		{
			%player.lastLOS[%target] = getSimTime();
		}
		return;
	}

	
	if(%oldState !$= "")
	{
		%statelist = %player.validStatePlayers[%oldState];
		%count = getWordCount(%statelist);
		for(%i = 0; %i < %count; %i++)
		{
			if(getWord(%statelist,%i) == %target)
			{
				%player.validStatePlayers[%oldState] = removeWord(%statelist,%i);
				break;
			}
		}
	}

	%player.validStatePlayers[%state] = ltrim(%player.validStatePlayers[%state] SPC %target);
	if(%oldState !$= "" && %state == $ValidState::CriminalCallout)
	{
		%player.validStateFor[%target] = %state SPC getWord(%player.validStateFor[%target],1);
	}
	else
	{
		%player.validStateFor[%target] = %state SPC getSimTime();
	}
	if(%state == $ValidState::Criminal || %state == $ValidState::CriminalInvisible || %state == $ValidState::Baiting)
	{	
		%player.lastLOS[%target] = getSimTime();
		if(!isEventPending(%player.CriminalDemotionLoop))
		{
			%player.CriminalDemotionLoop();
		}
	}
}

function Player::isValidState(%player,%target,%state)
{
	return getWord(%player.validStateFor[%target],0) == %state;
}

function Player::isValidStateOrHigher(%player,%target,%state)
{
	return getWord(%player.validStateFor[%target],0) >= %state;
}

function Player::isValidStateOrLower(%player,%target,%state)
{
	return getWord(%player.validStateFor[%target],0) <= %state;
}

function Player::getSoonestCriminal(%player,%target)
{
	Oopsies_AdminNotification(%player.isValidStateOrHigher(%target,$ValidState::Criminal) SPC %target.isValidStateOrHigher(%player,$ValidState::Criminal));
	if(%player.isValidStateOrHigher(%target,$ValidState::Criminal) && %target.isValidStateOrHigher(%player,$ValidState::Criminal))
	{
		if(getWord(%player.validStateFor[%target],1) < getWord(%target.validStateFor[%player],1))
		{
			return %target;
		}
		return %player;
	}

	if(%player.isValidStateOrHigher(%target,$ValidState::Criminal))
	{
		return %target;
	}

	if(%target.isValidStateOrHigher(%player,$ValidState::Criminal))
	{
		return %player;
	}
	return "";
}

function Player::getValidState(%player,%target)
{
	return getWord(%player.validStateFor[%target],0) + 0;
}

function Player::CriminalDemotionLoop(%player)
{
	%inivisibleStateList = %player.validStatePlayers[$ValidState::CriminalInvisible];
	%count = getWordCount(%inivisibleStateList);
	for(%i = 0; %i < %count; %i++)
	{
		%target = getWord(%inivisibleStateList, %i);

		// sanity
		if(!isObject(%target))
		{
			continue;
		}

		if(%target.isDisabled())
		{
			%player.setValidState(%target,$ValidState::Previously);
			continue;
		}

		if(getSimTime()-%player.lastLOS[%target] <= 30000)
		{
			continue;
		}

		%player.setValidState(%target,$ValidState::Previously);
	}

	%criminalStateList = %player.validStatePlayers[$ValidState::Criminal];
	%count = getWordCount(%criminalStateList);
	for(%i = 0; %i < %count; %i++)
	{
		%target = getWord(%criminalStateList, %i);

		// sanity
		if(!isObject(%target))
		{
			continue;
		}

		if(%target.isDisabled())
		{
			%player.setValidState(%target,$ValidState::Previously);
			continue;
		}

		if(Oopsies_IsVisible(%target,%player))
		{
			%player.lastLOS[%target] = getSimTime();
			continue;
		}

		if(getSimTime()-%player.lastLOS[%target] <= 5000)
		{
			continue;
		}

		%player.setValidState(%target,$ValidState::Previously);
	}

	%baitingStateList = %player.validStatePlayers[$ValidState::Baiting];
	%count = getWordCount(%baitingStateList);
	for(%i = 0; %i < %count; %i++)
	{
		%target = getWord(%baitingStateList, %i);

		// sanity
		if(!isObject(%target))
		{
			continue;
		}

		if(%target.isDisabled())
		{
			%player.setValidState(%target,$ValidState::Invalid);
			continue;
		}

		if(Oopsies_IsVisible(%target,%player))
		{
			continue;
		}

		%player.setValidState(%target,$ValidState::Invalid);
	}

	if(getWordCount(%criminalStateList SPC %inivisibleStateList SPC %baitingStateList) > 0)
	{
		%player.CriminalDemotionLoop = %player.schedule(100,"CriminalDemotionLoop");
	}
}

function Oopsies_DoAudibleEvent(%source)
{
	%minigame = BBB_Minigame;
	%count = %minigame.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%player = %minigame.playingClients[%i].player;
		if(isObject(%player) && %player != %source)
		{
			//make sure this isn't demoting a state
			if(%source.isValidStateOrLower(%player,$ValidState::Previously))
			{
				%source.SetValidState(%player,$ValidState::CriminalInvisible);
			}
		}
	}
}

function Oopsies_DoVisibleEvent(%source,%type)
{
	%minigame = BBB_Minigame;
	%count = %minigame.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%player = %minigame.playingClients[%i].player;
		if(isObject(%player) && %player != %source && %player.isValidStateOrLower(%target,$ValidState::Criminal))
		{	
			//make sure we aren't demoting a callout
			if(%source.isValidStateOrHigher(%player,%type))
			{
				continue;
			}

			if(Oopsies_IsVisible(%player,%source))
			{
				%source.SetValidState(%player,%type);
			}
		}
	}
}


function Oopsies_DoLoopingVisibleEvent(%player,%name,%type)
{
	Oopsies_DoVisibleEvent(%player,%type);
	%player._[%name] = schedule(100,%player,"Oopsies_DoLoopingVisibleEvent",%player,%name,%type);	
}

function Oopsies_StopLoopingVisibleEvent(%player,%name)
{
	cancel(%player._[%name]);
}


function Oopsies_IsVisible(%veiwer,%target)
{
	//can see feet
	if(!isObject(containerRayCast(%veiwer.getEyePoint(), %target.getPosition(), $TypeMasks::FxBrickObjectType)))
	{
		return true;
	}

	//can see eye
	if(!isObject(containerRayCast(%veiwer.getEyePoint(), %target.getEyePoint(), $TypeMasks::FxBrickObjectType)))
	{
		return true;
	}

	//can see center
	if(!isObject(containerRayCast(%veiwer.getEyePoint(), %target.getHackPosition(), $TypeMasks::FxBrickObjectType)))
	{
		return true;
	}
	return false;
}

$Oopsies::MeleeRange = 5;
$Oopsies::MeleeAngle = mDegToRad(45);
function Oopsies_MeleeCheck(%player)
{
	%group = ClientGroup.getId();
	%count = %group.getCount();
	%range = $Oopsies::MeleeRange;
	%angle = $Oopsies::MeleeAngle;
	for(%i = 0; %i < %count; %i++)
	{
		%target = %group.getObject(%i).player;
		if(!isObject(%player))
		{
			continue;
		}

		if(VectorDist(%player.position, %target.position) > $Oopsies::MeleeRange)
		{
			continue;
		}

		%localPos = vectorSub(%player.position,%target.position);
		if(mAcos(vectorDot(%player.getEyeVector(),%localPos)/VectorLen(%localPos)) > $Oopsies::MeleeAngle)
		{
			continue;
		}

		if(!Oopsies_IsVisible(%player,%target))
		{
			continue;
		}

		return true;
	}

	return false;
}

function Oopsies_DoCallout(%target,%caller,%minState)
{
	//do not demote 
	if(!%target.isValidState(%caller,$ValidState::CriminalCallout) && %target.getValidState(%caller) >= %minState)
	{
		%newstate = $ValidState::Callout;
		if(%target.isValidState(%caller,$ValidState::Criminal))
		{
			%newstate = $ValidState::CriminalCallout;
		}
		%target.SetValidState(%caller,%newstate);
	}
}

// Visible incriminating actions will make the player hard valid to other players within los
// Audible incriminating actions will make the player soft valid to all players
package TTT_Oopsies
{
	function Player::MountImage(%player,%image,%slot)
	{
		if(%image.TTT_Contraband)
		{
			Oopsies_DoLoopingVisibleEvent(%player,"Contraband",$ValidState::Criminal);
		}
		else
		{
			Oopsies_DoLoopingVisibleEvent(%player,"Baiting",$ValidState::Baiting);
		}
		return parent::MountImage(%player,%image,%slot);
	}

	function Player::UnmountImage(%player,%slot)
	{
		if(%player.getMountedImage(%slot).TTT_Contraband)
		{
			Oopsies_StopLoopingVisibleEvent(%player,"Contraband");
		}
		else
		{
			Oopsies_StopLoopingVisibleEvent(%player,"Baiting");
		}
		return parent::UnmountImage(%player,%slot);
	}

	function Armor::OnTrigger(%db,%player,%trigger,%active)
	{
		if(%trigger == 0 && %active)
		{
			%image = %player.getMountedImage(0);
			if(isObject(%image) && !%image.TTT_notWeapon $= "")
			{
				%count = -1;
				while(%count < 32)
				{
					%count++;
					%statename = %image.stateName[%count];
					
					if(%statename $= "")
					{
						continue;
					}

					if(%image.stateTransitionOnTriggerDown[%count] $= "")
					{
						continue;
					}

					if((%image.Melee || %image.item.AEAmmo) && !Oopsies_MeleeCheck(%player))
					{
						continue;
					}
					Oopsies_DoVisibleEvent(%player,$ValidState::Criminal);
					break;
				}
			}
		}
		
		return parent::OnTrigger(%db,%player,%trigger,%active);
	}

	function Armor::Damage(%db, %target, %source, %pos, %damage, %damageType)
	{
		%player = %source.client.player;

		if(!isObject(%player))
		{
			%player = %source;
		}
		
		while(%player.getClassName() !$= "Player")
		{
			%player = %player.sourceObject;
			if(!isObject(%player))
			{
				return parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
			}
		}

		Oopsies_DoVisibleEvent(%player,$ValidState::Criminal);
		Oopsies_DoAudibleEvent(%player);
		
		return parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		Oopsies_KillCheck(%sourceClient,%client);
		Oopsies_DoAudibleEvent(%sourceClient.player);
		
		return parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function serverCmdRotateBrick(%this, %dir)
	{
		if(%this.inBBB && isObject(%this.player))
		{
			%this.player.BBB_TargetAPlayer();
			// i hate this gamemode
			%target = %this.player.targetObj;

			if(%dir == -1 && isobject(%target))
			{
				Oopsies_DoCallout(%target,%this.player,$ValidState::Previously);
				
			}
		}
		parent::serverCmdRotateBrick(%this, %dir);
	}
};
activatePackage("TTT_Oopsies");

function BBB_ClosestPointOnLine(%p1,%p2,%point)
{
	%t = vectorDot(vectorSub(%p1,%p2),vectorSub(%p2,%point))/vectorDot(vectorSub(%p1,%p2),vectorSub(%p1,%p2));
	%g = vectorSub(%p2,vectorScale(vectorSub(%p1,%p2),%t));

	%d = vectorSub(%p1,%p2);
	%f = vectorSub(%g,%p2);
	%z = 0;
	for(%i = 0; %i < 3; %i++)
	{
		%a = getWord(%d,%i);
		%b = getWord(%f,%i);
		if(%a == 0 || %b == 0)
		{
			continue;
		}
		%z = %b / %a;
		break;
	}

	if(%z > 1)
	{
		return %p1;
	}
	else if(%z < 0)
	{
		return %p2;
	}
	else
	{
		return %g;
	}
}

function AESuppressArea(%pos, %dir, %shape, %img)
{
	%super = %img.whizzSupersonic;

	if(%shape.previousPoint $= "")
	{
		%shape.previousPoint = %shape.originPoint;
	}

	if(%shape.previousPoint $= %pos)
	{
		return;
	}

	if(%super == 0)
		%sfx = AESubsonicWhizz @ getRandom(1, 4) @ Sound;
	else if(%super == 1)
		%sfx = AESupersonicCrack @ getRandom(1, 4) @ Sound;
	else if(%super == 2)
		%sfx = AESupersonicBigCrack @ getRandom(1, 4) @ Sound;
	
	%angle = 1 - (%img.whizzAngle / 90);
	%chance = mClampF(%img.whizzChance / 100, 0, 1);
	%through = %img.whizzThrough;

	%sourcePlayer = %shape.sourceObject;
	%sourceClient = %sourcePlayer.client;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cc = ClientGroup.getObject(%i);

		%obj = %cc.getControlObject();

		if(!isObject(%obj) || %shape.suppressed[%obj])
			continue;
		
		%eye = %obj.getEyePoint();

		%linePoint = BBB_ClosestPointOnLine(%shape.previousPoint,%pos,%eye);
		%dist = vectorDist(%linePoint, %eye);
		%dot = vectorDot(%dir, vectorNormalize(vectorSub(%shape.previousPoint, %eye)));
		if(%dist < 6 && %dot <= %angle && getRandom() <= %chance)
		{
			if(%through || !isObject(containerRayCast(%linepoint, %eye, $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType)))
			{
				%shape.suppressed[%obj] = true;
				%cc.play3D(%sfx, %linePoint);
				if(isObject(%sourcePlayer))
				{
					Oopsies_DoVisibleEvent(%sourcePlayer,$ValidState::Criminal);
				}

				%p = new Projectile()
				{
					dataBlock = R_ShotgunRecoilProjectile;
					initialPosition = %eye;
				};

				%p.setScale("0.1 0.1 0.1");

				MissionCleanup.add(%p);

				%p.explode();
			}
		}
	}
	%shape.previousPoint = %pos;
}

