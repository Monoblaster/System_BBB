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
$Pref::Radar::UpdateTime = 20000;
$Pref::Radar::SearchRadius = 100;

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
			else if(!%client.cleardXrayBillboardGroup)
			{
				%client.cleardXrayBillboardGroup = true;
				%client.AVBillboardGroup.Clear("xray");
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
	%player.client.centerPrint("\c7refreshing in " @ mCeil(((%player.lastRadarCheck + $Pref::Radar::UpdateTime) - getSimTime()) / 1000) @ "..." NL %left @ %string @ "<font:palatino linotype:25>" @ %right NL "\c4" @ %closestDist,2);
}

function Player::updateRadarPositions(%player)
{
	%player.client.cleardXrayBillboardGroup = false;
	
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
		%radarRole[%count] = %currObj.client.role;
		%count++;
	}

	%group = %player.client.AVBillboardGroup;
	%group.Clear("xray");
	//create new ghosts at our positions and move them there
	for(%i = 0; %i < %count; %i++)
	{
		%pos = %radarPos[%i];
		%role = %radarRole[%i];
		if(%pos !$= "")
		{
			if(%role $= "Traitor" && %player.client.role $= "Traitor")
			{
				%group.make("traitorRadarBillboard",%pos,"xray");
			}
			else
			{
				%group.make("normalRadarBillboard",%pos,"xray");
			}
		}
	}
}