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
	%closestRastor = ($Radar::AngleCutoff * 2) + 1;

	%plrPos = %player.getPosition();
	%plrEyeVec = %player.getEyeVector();

	%count = %player.radarCount;
	for(%i = 0; %i < %count; %i++)
	{
		%objPos = %player.radarPos[%i];
		%relVec = vectorSub(%objPos,%plrPos);
		%dist = vectorLen(%relVec);

		%horzAngle = mATan(getWord(%relVec,0),getWord(%relVec,1)) - mATan(getWord(%plrEyeVec,0),getWord(%plrEyeVec,1));
		if(%horzAngle < 0)
		{
			%horzAngle += $pi * 2;
		}
		%horzAngle *= 180 / $PI;
		%horzAngle = (%horzAngle + 180) % 360;

		if(%horzAngle < (180 - $Radar::AngleCutoff))
		{
			%isLeftPoints = true;
		}
		else if(%horzAngle > (180 + $Radar::AngleCutoff))
		{
			%isRightPoints = true;
		}
		else
		{
			//get vert angle
			%relAngle = mACos(getWord(%relVec,2) / vectorLen(%relVec));
			%plrAngle = mACos(getWord(%plrEyeVec,2) / vectorLen(%plrEyeVec));
			%vertAngle = (%relAngle - %plrAngle) * 180 / $PI;

			//this point is within our view rastorize it within our size
			%rastorLoc = mRound((%horzAngle - (180 - $Radar::AngleCutoff)) / ($Radar::AngleCutoff * 2) * $Radar::DisplaySize);

			//pevent multiple points taking up the same place
			//we will squish if there are 5 points there already
			%squish = 5;
			while(%rastor[%rastorLoc] !$= "" && %safety == 0)
			{
				%rastorLoc++;
				%squish--;
			}

			%rastor[%rastorLoc] = mRound(%dist) SPC %vertAngle;
			if(mAbs(%rastorLoc - ($Radar::DisplaySize / 2)) < mAbs(%closestRastor - ($Radar::DisplaySize / 2)))
			{
				%closestRastor = %rastorLoc;
			}
		}
	}

	//loop through the rastor array and make a display string
	%string = $Radar::DisplayFont @ "\c8";
	for(%i = 0; %i <= $Radar::DisplaySize; %i++)
	{
		%point = %rastor[%i];
		if(%point $= "")
		{
			%string = %string @ "|";
		}
		else
		{
			%dist = getWord(%point,0);
			%vertAngle = getWord(%point,1);
			%char = "O";
			//special value that scales when you are parralel with the point
			%selection = 5 * (10 / %dist);
			if(mABS(%vertAngle + %selection) > %selection)
			{
				if(%vertAngle > 0)
				{
					%char = "_";
				}
				else
				{
					%char = "^";
				}
			}

			%color = "\c6";
			if(%dist > 30)
			{
				%color = "\c7";
			}

			%font = %color;
			if(%closestRastor == %i)
			{
				//this is the closest to display info from
				%closestDist = %dist @ "tu";
				%string = %string @ $Radar::DisplayBoldFont @ "\c4" @ %char @ $Radar::DisplayFont @ "\c8";
			}
			else
			{
				%string = %string @ %color @ %char @ "\c8";
			}
		}
	}
	%left = "[";
	%right = "]";
	if(%isLeftPoints)
	{
		%left = "<";
	}
	if(%isRightPoints)
	{
		%right = ">";
	}

	%player.client.centerPrint("\c7refreshing in " @ mRound(((%player.lastRadarCheck + $Pref::Radar::UpdateTime) - getSimTime()) / 1000) @ "..." NL %left @ %string @ "<font:palatino linotype:25>" @ %right NL "\c4" @ %closestDist);
}

function Player::updateRadarPositions(%player)
{
	if((getSimTime() - %player.lastRadarCheck) < $Pref::Radar::UpdateTime && %player.lastRadarCheck !$= "")
	{
		return;
	}
	%player.lastRadarCheck = getSimTime();
	echo("egg");
	
	// get pos
	initContainerRadiusSearch(%player.getPosition(), $Pref::Radar::SearchRadius, $TypeMasks::PlayerObjectType);		
	%c = 0;
	while(isObject(%currObj = containerSearchNext()))
	{
		if(%currObj == %player)
		{
			continue;
		}
		// if(%col.getClassName() $= "AIPlayer")
		// 	continue;

		%player.radarPos[%c] = %currObj.getPosition();
		%c++;
	}
	%player.radarCount = %c;
}