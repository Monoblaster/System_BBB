
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

		if(%client.lostOopsies !$= "")
		{
			%client.chatMessage("You lost" SPC %client.lostOopsies SPC "oopsies :(");
			%client.lostOopsies = "";
		}

		if(!%client.dataInstance($TTT::Data).oopsiesChange && %amount < 3)
		{
			%client.chatMessage("You gained an oopsie!");
			%amount = %client.dataInstance($TTT::Data).oopsies = %amount + 1;
		}
		%client.dataInstance($TTT::Data).oopsiesChange = false;

		if(%amount <= 0)
		{
			%client.slayed = 2;
			
			%message = '%1 is out of oopsies. They have a balance of %2 Oopsies.';
			messageAll('', %message, %client.getPlayerName(),%amount);

			%player = %client.player;
			if(isObject(%player))
			{
				%player.kill();
			}
		}
		else
		{
			%client.chatMessage("You have" SPC %amount SPC "oopsies left. Good luck!");
		}

		if(%amount == 1 && %client.slayed > 0)
		{
			%client.slayed = 1;
		}
			

		%client.roundOopsies = 0;
	}
}

$KillType::Valid = 0;
$KillType::Invalid = 1;
$KillType::CriminalInvalid = 2;
$KillType::Uknown = 3;
function Oopsies_KillCheck(%client,%targetclient)
{
	%type = %client.winCondition.getKillType(%client.player,%targetclient.player);
	if(%type == $KillType::Invalid)
	{
		if(%client == %targetclient || %tpye == $KillType::CriminalInvalid )
		{
			%client.AddOopsies(-1);
			return;
		}
		
		%client.AddOopsies(-2);
		return;
	}

	if(%type == $KillType::Uknown)
	{
		%client.AddOopsies(-1);
		%targetclient.AddOopsies(-1);
	}
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

		if(%client.roundOopsies <= -4)
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
$ValidState::Previously = 1;
$ValidState::Callout = 2;
$ValidState::Criminal = 3;
$ValidState::CriminalCallout = 4;

function Player::SetValidState(%player,%target,%state)
{
	%oldState = getWord(%player.validStateFor[%target],0);
	if(%oldState == %state)
	{
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
	%player.validStateFor[%target] = %state SPC getSimTime();
	if(%state == $ValidState::Criminal && !isEventPending(%player.CriminalDemotionLoop))
	{
		%player.CriminalDemotionLoop();
	}
}

function Player::isValidState(%player,%target,%state)
{
	return getWord(%player.validStateFor[%target],0) == %state;
}

function Player::getSoonestCriminal(%player,%target)
{
	%playerState = getWord(%player.validStateFor[%target],0);
	if(%playerState == getWord(%target.validStateFor[%player],0) && %playerState == $ValidState::Criminal)
	{
		if(getWord(%player.validStateFor[%target],0) - getWord(%target.validStateFor[%player],0) < 0)
		{
			return %player;
		}
		return %target;
	}
	return "";
}

function Player::getValidState(%player,%target)
{
	return getWord(%player.validStateFor[%target],0) + 0;
}

function Player::CriminalDemotionLoop(%player)
{
	%stateList = %player.validStatePlayers[$ValidState::Criminal];
	%count = getWordCount(%statelist);
	for(%i = 0; %i < %count; %i++)
	{
		%target = getWord(%stateList, %i);

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
			continue;
		}

		%player.setValidState(%target,$ValidState::Previously);
	}

	if(getWordCount(%statelist) > 0)
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
		%player = %minigame.players[%i].player;
		if(isObject(%player) && %player != %source)
		{
			//make sure this isn't demoting a state
			if(%source.isValidState(%player,$ValidState::Invalid))
			{
				%source.SetValidState(%player,$ValidState::Previously);
			}
		}
	}
}

function Oopsies_DoVisibleEvent(%source)
{
	%minigame = BBB_Minigame;
	%count = %minigame.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%player = %minigame.players[%i].player;
		if(isObject(%player) && %player != %source)
		{	
			//make sure we aren't demoting a callout
			%newState = $ValidState::Criminal;
			if(%source.isValidState(%player,$ValidState::Callout))
			{
				%newState = $ValidState::CriminalCallout;
			}

			if(Oopsies_IsVisible(%player,%source))
			{
				%source.SetValidState(%player,%newState);
				continue;
			}
		}
	}
}


function Oopsies_DoLoopingVisibleEvent(%player,%name)
{
	Oopsies_DoVisibleEvent(%player);
	%player._[%name] = schedule(100,%player,"Oopsies_DoLoopingVisibleEvent",%player,%name);	
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
			Oopsies_DoLoopingVisibleEvent(%player,"Contraband");
		}
		return parent::MountImage(%player,%image,%slot);
	}

	function Player::UnmountImage(%player,%slot)
	{
		if(%player.getMountedImage(%slot).TTT_Contraband)
		{
			Oopsies_StopLoopingVisibleEvent(%player,"Contraband");
		}
		return parent::UnmountImage(%player,%slot);
	}

	function Armor::OnTrigger(%db,%player,%trigger,%active)
	{
		if(%trigger == 0 && %active)
		{
			%image = %player.getMountedImage(0);
			if($BBB::Round::Phase $= "Round" && isObject(%image) && !%image.TTT_notWeapon)
			{
				%currState = %player.getImageState(0);
				%count = 0;
				while(%count < 32)
				{
					%statename = %image.stateName[%count];
					if(%statename $= "")
					{
						%count++;
						continue;
					}

					if(%statename $= %currState)
					{
						if(%image.stateTransitionOnTriggerDown[%count] !$= "")
						{
							if(!%image.TTT_isSilent)
							{
								Oopsies_DoAudibleEvent(%player);
							}
							Oopsies_DoVisibleEvent(%player);
						}
						break;
					}
					%count++;
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
			if(%player $= "")
			{
				return parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
			}
		}

		if($BBB::Round::Phase $= "Round")
		{
			Oopsies_DoVisibleEvent(%player);
		}
		
		return parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if(%client.inBBB && $BBB::Round::Phase $= "Round")
		{
			Oopsies_KillCheck(%sourceClient,%client);
		}
		
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


