// =================================================
// Server_Radar - By LegoPepper      
// =================================================
// Table of Contents:                       
//        
// 1. Preferences	
// 2. Functions					
// =================================================

// =================================================
// 1. Preferences
// =================================================
$Pref::Radar::NumBars = 100; // Number of bars
$Pref::Radar::SearchRadius = 1000; // How far to look for players
$Pref::Radar::Size = 13; // Font size
$Pref::Radar::UpdateTime = 0;

// =================================================
// 2. Functions
// =================================================
// from Bot_Hole
function RadarSilentLoop()
{
	cancel($Radar::SilentSchedule);	//prevent double schedules
	$Radar::LoopActive = true;	
	
	%user = false;	
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%obj = %client.player;
		if(isObject(%obj))
		{
			if(%obj.hasRadar)
			{
				%obj.updateRadar();
				%user = true;
			}
		}
	}
	if(%user)
		$Radar::SilentSchedule = scheduleNoQuota(100,0,RadarSilentLoop); //continue the loop
	else //otherwise
		$TP_Crosshair::LoopActive = false;
}

function Player::updateRadar(%obj)
{	
	%obj.updateRadarPositions();
	%obj.displayRadar();
}

$Radar::DisplaySize = 50;
$Radar::AngleCutoff = 100 / 2;
$Radar::DisplayFont = "<font:consolas:15>";
$Radar::DisplayBoldFont = "<font:consolas bold:20>";
function Player::displayRadar(%player)
{
	%player.client.centerPrint("\c7refreshing in " @ mRound(((%player.lastRadarCheck + $Pref::Radar::UpdateTime) - getSimTime()) / 1000) @ "..." NL %left @ %string @ "<font:palatino linotype:25>" @ %right NL "\c4" @ %closestDist);
}

function Player::updateRadarPositions(%player)
{
	if((getSimTime() - %player.lastRadarCheck) < $Pref::Radar::UpdateTime && %player.lastRadarCheck !$= "")
	{
		return;
	}
	%player.lastRadarCheck = getSimTime();
	
	// get pos
	initContainerRadiusSearch(%player.getPosition(), $Pref::Radar::SearchRadius, $TypeMasks::PlayerObjectType);		
	%count = 0;
	while(isObject(%currObj = containerSearchNext()))
	{
		if(%currObj == %player)
		{
			continue;
		}
		// if(%col.getClassName() $= "AIPlayer")
		// 	continue;
		%radarPos[%count] = %currObj.getPosition();
		%count++;
	}

	%groupCount = %player.xrayGhostGroup.getCount();
	//create new ghosts at our positions and move them there
	for(%i = 0; %i < %groupCount; %i++)
	{
		%billboard = %player.xrayGhostGroup.getObject(%i);
		%pos = %radarPos[%i];
		if(%pos !$= "")
		{
			%billboard.setTransform(%pos);
			%billboard.light.setEnable(true);
		}
		else
		{
			%billboard.light.setEnable(false);
		}
	}
}

function createRadarBillboards(%player)
{
	%ghostGroup = %player.xrayGhostGroup;
	if(!isObject(%ghostGroup))
	{
		%ghostGroup = %player.xrayGhostGroup = new scriptGroup();
	}
	else
	{
		//clear all the previous ghosts
		%groupCount = %ghostGroup.getCount();
		for(%i = %groupCount - 1; %i >= 0; %i--)
		{
			Billboard_Delete(%ghostGroup.getObject(%i));
		}
	}

	for(%i = 0; %i < 30; %i++)
	{
		%billboard = Billboard_Create("ghostRadarBillboard","FloatingBillboardPlayer",true);

		//move it to the player and ghost it there
		Billboard_GhostTo(%billboard,%player.client);
		%billboard.light.attachtoObject(bob);

		//now move it to it's final resting place
		schedule(100,0,"finishRadarBillboard",%player,%billboard);
		%billboard.setnetFlag(1,true);
	}
}

function finishRadarBillboard(%player,%billboard)
{
	%billboard.light.setDatablock("normalRadarBillboard");
	%billboard.light.attachtoObject(%billboard);
	%billboard.light.setEnable(false);
	%player.xrayGhostGroup.add(%billboard);
}