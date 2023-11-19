
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
	%count = %minigame.numPlayers;
	%ContestedList = "";
	for(%i = 0; %i < %count; %i++)
	{
		%client = %minigame.players[%i];
		if(!isObject(%client.refundContestor))
		{
			continue;
		}

		%contestedList = %contestedList TAB %client SPC %client.refundContestor;
		%client.refundContestor = "";
	}

	Oopsies_ContestedVote(%contestedList);
}

function Oopsies_ContestedVote(%contestedList)
{
	if(getFieldCount(%contestedList) == 0)
	{
		Oopsies_PointsCheck();
		return;
	}

	%contestor = getWord(getField(%contestedList,0),0);
	%killer = getWord(getField(%contestedList,0),1);

	if(!isObject(%contestor) || !isObject(%killer))
	{
		Oopsies_ContestedVote(removeField(%contestedList,0));
		return;
	}

	chatMessageAll(0,'%1 is contesting %2\'s kill on them. If this vote passes the killer will get no oopsie.',%contestor.getPlayerName(),%killer.getPlayerName());
	chatMessageAll(0,'/yes if you agree with %1',%contestor.getPlayerName());
	chatMessageAll(0,'/no if you agree with %2',%killer.getPlayerName());
	vote_start("Vote_addCrossUniqueVote","yes","no");

	schedule(15000,0,"Oopsies_ContestedVoteEnd",%killer.dataInstance($TTT::Data),%contestedList);
}

function Oopsies_ContestedVoteEnd(%killerData,%contestedList)
{
	%killer = getWord(getField(%contestedList,0),1);


	if(!isObject(%killer))
	{
		%killerData.refundoopsies -= 1;
		Oopsies_ContestedVote(removeField(%contestedList,0));
		return;
	}

	%temp = vote_end();
	%winners = getField(%temp,0);
	%winningTotal = getField(%temp,1);
	if(getWordCount(%winners) > 1)
	{
		chatMessageAll(0,'Tied with %2 votes. %1 gets no refund', %killer.getPlayerName(),%winningTotal);
		%killerData.refundoopsies -= 1;
		Oopsies_ContestedVote(removeField(%contestedList,0));
		return;
	}

	if(%winners == 0)
	{
		chatMessageAll(0,'"Yes" won with %2 votes. %1 gets no refund', %killer.getPlayerName(),%winningTotal);
		%killerData.refundoopsies -= 1;
		Oopsies_ContestedVote(removeField(%contestedList,0));
		return;
	}

	if(%winners == 1)
	{
		chatMessageAll(0,'"No" won with %2 votes. %1 gets a refund', %killer.getPlayerName(),%winningTotal);
	}

	Oopsies_ContestedVote(removeField(%contestedList,0));
}

function Oopsies_PointsCheck()
{
	%minigame = BBB_Minigame;
	%count = %minigame.numPlayers;
	for(%i = 0; %i < %count; %i++)
	{
		%client = %minigame.players[%i];

		%amount = %client.dataInstance($TTT::Data).oopsies;
		%refunded = %client.dataInstance($TTT::Data).refundoopsies;

		if(%refunded > 0)
		{
			%addedRefund = mFloor(%amount + %refunded,3) - %amount;
			%client.chatMessage("You were refunded" SPC %addedRefund SPC "oopsies!");
			%client.dataInstance($TTT::Data).refundoopsies = 0;
			%client.dataInstance($TTT::Data).oopsies += %addedRefund;
		}

		if(!%client.dataInstance($TTT::Data).oopsiesChange && %amount != 3)
		{
			%client.chatMessage("You gained an oopsie!");
			%client.dataInstance($TTT::Data).oopsies = mFloor(%amount + 1,3);
		}
		%client.dataInstance($TTT::Data).oopsiesChange = false;

		
		if(%amount < 0)
		{
			%slayAmount = (-%amount) + 1 + %client.slayed;
			%message = '%1 ran out of oopsies. They are out for %2 Round.';
			if(%slayAmount > 1)
			{
				%message = '%1 ran out of oopsies. They are out for %2 Rounds.';
			}

			messageAll('MsgAdminForce', %message, %client.getPlayerName(), %slayAmount);

			%client.slayed = %slayAmount;

			%player = %client.player;
			if(isObject(%player))
			{
				%player.kill();
			}
			
			%client.dataInstance($TTT::Data).oopsies = 1;
		}
		else
		{
			%client.chatMessage("You have" SPC %amount SPC "oopsies left. Good luck!");
		}

		%client.roundOopsies = 0;
	}
}

$KillType::Valid = 0;
$KillType::Invalid = 1;
$KillType::Uknown = 2;
function Oopsies_KillCheck(%player,%target)
{
	%type = %client.winCondition.getKillType(%player,%target);
	if(%type == $KillType::Invalid)
	{
		%player.client.AddOopsies(-1);
		return;
	}

	if(%type == $KillType::Uknown)
	{
		%player.client.AddOopsies(-1);
		%target.client.AddOopsies(-1);

		%target.client.OopsieRefundPrompt(%player.client);
	}
}

$TTT::Data = 1;
function GameConnection::AddOopsies(%client,%amount)
{
	%client.dataInstance($TTT::Data).oopsies += %amount;
	%client.dataInstance($TTT::Data).oopsiesChange = true;

	if(%amount < 0 && !%client.slayed)
	{
		%client.roundOopsies += %amount;

		if(%client.roundOopsies >= 3)
		{
			messageAll('MsgAdminForce', '%1 went on an oopsie driven rampage. They are out for 3 Rounds.', %client.getPlayerName());

			%client.slayed = 4;

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

function GameConnection::OopsieRefundPrompt(%client,%killer)
{
	%client.refundKiller = %killer;
	%client.chatMessage("You and your killer both lost an oopsie!");
	%client.chatMessage("Use /refund to return both of the oopsies");
	%client.chatMessage("Use /contest to start a server vote at the end of the round to not return your killer's oopsie");
}

function serverCmdRefund(%client)
{
	if(%client.refundKiller !$= "")
	{
		if(!isObject(%client.refundKiller))
		{
			%client.dataInstance($TTT::Data).refundoopsies += 1;
			%client.refundKiller = "";
			return;
		}

		%client.dataInstance($TTT::Data).refundoopsies += 1;
		%client.refundKiller.dataInstance($TTT::Data).refundoopsies += 1;

		%client.refundKiller = "";
	}
}

function serverCmdContest(%client)
{
	if(%client.refundKiller !$= "")
	{
		if(!isObject(%client.refundKiller))
		{
			%client.dataInstance($TTT::Data).refundoopsies += 1;
			%client.refundKiller = "";
			return;
		}

		%client.dataInstance($TTT::Data).refundoopsies += 1;
		%client.refundKiller.dataInstance($TTT::Data).refundoopsies += 1;

		%client.refundContestor = %client.refundKiller;

		%client.refundKiller = "";
	}
}

$ValidState::Invalid = 0;
$ValidState::Previously = 1;
$ValidState::Callout = 2;
$ValidState::Criminal = 3;
$ValidState::CriminalCallout = 3;

function Player::SetValidState(%player,%target,%state)
{
	if(%player.validStateFor[%target] !$= "" && %player.validStateFor[%target] != %state)
	{
		%statelist = %player.validState[%state];
		%count = getWordCount(%statelist);
		for(%i = 0; %i < %count; %i++)
		{
			if(getWord(%statelist,%i) == %target)
			{
				removeWord(%statelist,%i);
				break;
			}
		}
	}

	%player.validStatePlayers[%state] = %player.validState[%state] SPC %target;
	%player.validStateFor[%target] = %state SPC getSimTime();
	if(%state == $ValidState::Criminal && !%player.CriminalDemotionLoop.isEventPending())
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
			removeWord(%i);
			%i--;
			continue;
		}

		if(%target.isDisabled())
		{
			%player.setValidState(%target,$ValidState::Previously);
			continue;
		}

		//can see feet
		if(containerRayCast(%target.getEyePoint(), %player.getPosition(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		//can see eyes
		if(containerRayCast(%target.getEyePoint(), %player.getEyePoint(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		//can see center
		if(containerRayCast(%target.getEyePoint(), %player.getHackPosition(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		%player.setValidState(%target,$ValidState::Previously);
	}

	%stateList = %player.validStatePlayers[$ValidState::CriminalCallout];
	%count = getWordCount(%statelist);
	for(%i = 0; %i < %count; %i++)
	{
		%target = getWord(%stateList, %i);

		// sanity
		if(!isObject(%target))
		{
			removeWord(%i);
			%i--;
			continue;
		}

		if(%target.isDisabled())
		{
			%player.setValidState(%target,$ValidState::Previously);
			continue;
		}

		//can see feet
		if(containerRayCast(%target.getEyePoint(), %player.getPosition(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		//can see eyes
		if(containerRayCast(%target.getEyePoint(), %player.getEyePoint(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		//can see center
		if(containerRayCast(%target.getEyePoint(), %player.getHackPosition(), $TypeMasks::FxBrickObjectType, %target))
		{
			continue;
		}

		%player.setValidState(%target,$ValidState::Callout);
	}

	//continue loop if we still have players on us
	if(getWordCount(%player.validStatePlayers[$ValidState::Criminal]) != 0)
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
		if(isObject(%player))
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
		if(isObject(%player))
		{	
			//make sure we aren't demoting a callout
			%newState = $ValidState::Criminal;
			if(%source.isValidState(%player,$ValidState::Callout))
			{
				%newState = $ValidState::CriminalCallout;
			}

			//can see feet
			if(containerRayCast(%player.getEyePoint(), %source.getPosition(), $TypeMasks::FxBrickObjectType, %player))
			{
				%source.SetValidState(%player,%newState);
				continue;
			}

			//can see eye
			if(containerRayCast(%player.getEyePoint(), %source.getEyePoint(), $TypeMasks::FxBrickObjectType, %player))
			{
				%source.SetValidState(%player,%newState);
				continue;
			}

			//can see center
			if(containerRayCast(%player.getEyePoint(), %source.getHackPosition(), $TypeMasks::FxBrickObjectType, %player))
			{
				%source.SetValidState(%player,%newState);
				continue;
			}
		}
	}
}

function Oopsies_DoCallout(%target,%caller)
{
	%target.SetValidState(%player,$ValidState::Callout);
}

// Visible incriminating actions will make the player hard valid to other players within los
// Audible incriminating actions will make the player soft valid to all players
package TTT_Oopsies
{
	function Armor::OnTrigger(%db,%player,%trigger,%active)
	{
		%image = %player.getMountedImage(0);
		if($BBB::Round::Phase $= "Round" && isObject(%image) && !%image.TTT_notWeapon)
		{
			%currState = %player.getImageState(0);
			%count = 0;
			while(%image.stateName[%count] !$= "")
			{
				if(%image.stateName[%count] $= %currState)
				{
					if(%image.stateTransitionOnTriggerDown[%count] !$= "")
					{
						if(!%image.TTT_isSilent)
						{
							Oopsies_DoAudibleEvent(%player);
						}
						Oopsies_DoVisibleEvent(%source);
					}
					break;
				}
				%count++;
			}
		}
		return parent::OnTrigger(%db,%player,%trigger,%active);
	}

	function Armor::Damage(%db, %target, %source, %pos, %damage, %damageType)
	{
		if($BBB::Round::Phase $= "Round")
		{
			Oopsies_DoVisibleEvent(%source);
		}
		
		return parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if(%client.inBBB && $BBB::Round::Phase $= "Round")
		{
			Oopsies_KillCheck(%sourceClient.player,%client.player);
		}
		
		return parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}
};
activatePackage("TTT_Oopsies");