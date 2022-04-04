// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot
// =================================================
// File Name: objects.cs
// Description: Datablocks, items, etc.
// =================================================
// Table of Contents:
//
// 1. Player
// 2. Sounds
// 3. ScriptObject
// =================================================

// =================================================
// 1. Player
// =================================================
if(!isObject(BBB_Standard_Armor))
{
	datablock PlayerData(BBB_Standard_Armor : PlayerStandardArmor)
	{
		// Standard Properties
		canJet			= false;
		canRide			= true;
		jetEnergyDrain	= 0;
		maxDamage		= 100;
		maxEnergy		= 100;
		maxTools			= 7;
		maxWeapons		= 7;
		minJetEnergy	= 0;
		paintable		= true;
		uiName			= "";

		// Speed Properties
		runEnergyDrain		= 0;
		runForce				= 48 * 90;
		maxBackwardSpeed	= 4 + 0;
		maxForwardSpeed	= 7 + 0;
		maxSideSpeed		= 6 + 0;
		minRunEnergy		= 0;

		maxBackwardCrouchSpeed	= 2 + 0.0;
		maxForwardCrouchSpeed	= 3 + 0.0;
		maxSideCrouchSpeed		= 2 + 0.0;

		jumpForce			= 12 * 90;
		jumpEnergyDrain	= 0;
		jumpDelay			= 3;
		minJumpEnergy		= 0;

		// Camera
		cameraMaxDist = 389;//7; //Default (8)
		cameraTilt = 2.38;//0.05; //Default (0.261)
		cameraVerticalOffset = 65535;//2; //Default (0.75)
		cameraHorizontalOffset = 65535;//0;

		firstPersonOnly = true;
	};
}

if(!isObject(BBB_Standard_Corpse_Armor))
{
	datablock PlayerData(BBB_Standard_Corpse_Armor : BBB_Standard_Armor) //NB: change PlayerStandardArmor to whatever will be our default playertype in the gamemode
	{
		uiName = "Corpse Player";
		canJet = 0;
		boundingBox = "5 5 1";
		crouchBoundingBox = "5 5 1";
		firstPersonOnly = 1;
	};
}

// =================================================
// 2. Sounds
// =================================================
if(!isObject(BBB_Chat_Sound))
{
	datablock AudioProfile(BBB_Chat_Sound)
	{
		fileName = $BBB::Path @ "sounds/talk.wav"; // From Server_Roleplay, thanks Zapk
		description = Audio2d;
		preload = true;
	};
}

if(!isObject(BBB_EndRound_Sound))
{
	datablock AudioProfile(BBB_EndRound_Sound)
	{
		filename		= $BBB::Path @ "sounds/gameover.wav";
		description	= Audio2d;
		preload		= true;
	};
}

if(!isObject(BBB_StartRound_Sound))
{
	datablock AudioProfile(BBB_StartRound_Sound)
	{
		filename		= $BBB::Path @ "sounds/startRound.wav";
		description	= Audio2d;
		preload		= true;
	};
}

if(!isObject(BBB_Death_Sound))
{
	datablock AudioProfile(BBB_Death_Sound)
	{
		filename		= $BBB::Path @ "sounds/death.wav";
		description	= mdMusic;
		preload		= true;
	};
}

// =================================================
// 3. ScriptObject
// =================================================
