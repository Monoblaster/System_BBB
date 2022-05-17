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
$Pref::Radar::UpdateTime = 20000;

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
	%player.client.centerPrint("\c7refreshing in " @ mCeil(((%player.lastRadarCheck + $Pref::Radar::UpdateTime) - getSimTime()) / 1000) @ "..." NL %left @ %string @ "<font:palatino linotype:25>" @ %right NL "\c4" @ %closestDist);
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
		%radarRole[%count] = %currObj.client.role;
		%count++;
	}

	%group = %player.client.visibleBillboardGroup;
	%group.ClearBillboards("xray");
	//create new ghosts at our positions and move them there
	for(%i = 0; %i < %count; %i++)
	{
		%pos = %radarPos[%i];
		%role = %radarRole[%i];
		if(%pos !$= "")
		{
			if(%role $= "Traitor")
			{
				%group.billboard("traitorRadarBillboard",%pos,"xray");
			}
			else
			{
				%group.billboard("normalRadarBillboard",%pos,"xray");
			}
		}
	}
}