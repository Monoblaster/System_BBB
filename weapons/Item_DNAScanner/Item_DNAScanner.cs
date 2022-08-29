

//////////
// item //
//////////
datablock ItemData(DNAScannerItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./scannerPistol.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "DNA Scanner";
	iconName = "";
	doColorShift = true;
	colorShiftColor = "0.9 0.9 0.9 1.000";

	 // Dynamic properties defined by the scripts
	image = DNAScannerImage;
	canDrop = true;
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(DNAScannerImage)
{
   // Basic Item properties
   shapeFile = "./scannerPistol.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0";
   eyeOffset = 0; //"0.7 1.2 -0.5";
   rotation = eulerToMatrix( "0 0 0" );

   // When firing from a point offset from the eye, muzzle correction
   // will adjust the muzzle vector to point to the eye LOS point.
   // Since this weapon doesn't actually fire from the muzzle point,
   // we need to turn this off.  
   correctMuzzleVector = true;

   // Add the WeaponImage namespace as a parent, WeaponImage namespace
   // provides some hooks into the inventory system.
   className = "WeaponImage";

   // Projectile && Ammo.
   item = DNAScannerItem;
   ammo = " ";
   projectile = gunProjectile;
   projectileType = Projectile;

	casing = gunShellDebris;
	shellExitDir        = "1.0 -1.3 1.0";
	shellExitOffset     = "0 0 0";
	shellExitVariance   = 15.0;	
	shellVelocity       = 7.0;

   //melee particles shoot from eye node for consistancy
   melee = false;
   //raise your arm up or not
   armReady = true;

   doColorShift = true;
   colorShiftColor = DNAScannerItem.colorShiftColor;//"0.400 0.196 0 1.000";

   //casing = " ";

   // Images have a state system which controls how the animations
   // are run, which sounds are played, script callbacks, etc. This
   // state system is downloaded to the client so that clients can
   // predict state changes and animate accordingly.  The following
   // system supports basic ready->fire->reload transitions as
   // well as a no-ammo->dryfire idle state.

   // Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.15;
	stateTransitionOnTimeout[0]       = "Ready";
	stateSound[0]					= weaponSwitchSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;
	stateSequence[1]	= "Ready";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Ready";
	stateTimeoutValue[2]            = 0.24;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]			= true;
	stateEmitterTime[2]				= 0.05;
	stateEjectShell[2]       = true;


};

function DNAScannerImage::onFire(%this,%obj,%slot)
{
	if(%obj.DNA)
	{
		if(%obj.hasDNAScanner)
		{
			%obj.client.play2D(XrayOffSound);
			%obj.hasDNAScanner = false;
			%obj.hasRadar = false;
			commandToClient(%obj.client, 'clearCenterPrint');
		}
		else
		{
			%obj.client.play2D(XrayOnSound);
			%obj.hasDNAScanner = true;
			%obj.hasRadar = true;
			commandToClient(%obj.clien, 'clearCenterPrint');
			RadarSilentLoop();
		}
	}
	else
	{
		if(%obj.getDamagePercent() < 1.0)
			%obj.playThread(2, shiftAway);	
		
		%corpse = %obj.findCorpseRayCast();
		if(isObject(%corpse))
		{
			%found = false;
			for(%i = 0; %i < getFieldCount(%corpse.fingerPrints); %i++)
			{
				%field = getField(%corpse.fingerPrints, %i);
				if(strstr(%obj.DNA, %field) < 0)
				{
					%obj.DNA = %obj.DNA @ "	" @ %field;
					%found = true;
				}
			}

			if(%found)
			{
				%obj.client.play2D(XrayOnSound);
				%obj.hasDNAScanner = true;
				%obj.hasRadar = true;
				commandToClient(%obj.clien, 'clearCenterPrint');
				RadarSilentLoop();
			}
		}
	}
}

package DNA_Scanner
{
	function Armor::onTrigger(%this, %obj, %trigger, %state)
	{
		%image = %obj.getMountedImage(0);
		if(isObject(%image))
		{
			%image = %image.getName();
		}

		if(%image $= "DNAScannerImage")
		{
			if(%trigger == 4 && %state = true && %obj.dna)
			{
				%obj.client.play2D(XrayOffSound);
				%obj.hasDNAScanner = false;
				%obj.hasRadar = false;
				commandToClient(%obj.client, 'clearCenterPrint');
				%obj.client.centerprint("\c6DNA Ejected",2);

				%obj.DNA = "";
			}
		}
		else
		{
			parent::onTrigger(%this, %obj, %trigger, %state);
		}
	}
	
	function Player::updateRadarPositions(%obj)
	{
		%obj.client.cleardXrayBillboardGroup = false;
		if(!%obj.hasDNAScanner)
		{
			return parent::updateRadarPositions(%obj);
		}
			
		if((getSimTime() - %obj.lastRadarCheck) < $Pref::Radar::UpdateTime && %obj.lastRadarCheck !$= "")
		{
			return;
		}
		%obj.lastRadarCheck = getSimTime();
		
		// get pos
		initContainerRadiusSearch(%obj.getPosition(), $Pref::Radar::SearchRadius, $TypeMasks::PlayerObjectType);		
		%count = 0;
		while(isObject(%currObj = containerSearchNext()))
		{
			if(strstr(%obj.DNA, %currObj) < 0)
			{
				continue;
			}

			if(%currObj == %obj)
			{
				continue;
			}
			// if(%col.getClassName() $= "AIobj")
			// 	continue;
			%radarPos[%count] = %currObj.getPosition();
			%count++;
		}
		%client = %Obj.client;
		%group = %client.AVBillboardGroup;
		%client.ClearXrayBillboards();
		//create new ghosts at our positions and move them there
		for(%i = 0; %i < %count; %i++)
		{
			%pos = %radarPos[%i];
			if(%pos !$= "")
			{
				%client.AddXrayBillboard(normalRadarBillboard,%pos);
			}
		}
	}
	
	function XrayImage::onFire(%this,%obj,%slot)
	{
		%obj.hasDNAScanner = false;
		parent::onFire(%this, %obj, %slot);
	}


	function servercmdDropTool(%this,%slot)
	{
		%player = %this.player;

		if(isobject(%player.tool[%slot]))
		{
		if(%player.tool[%slot].getname() $= "DNAScannerItem")
		{
			parent::servercmdDropTool(%this,%slot);

			if(%player.hasRadar && %player.hasDNAScanner) 
			{
				%player.hasRadar = false;
				%player.hasDNAScanner = false;
			}
			return;
		}
		
		
		}
		parent::servercmdDropTool(%this,%slot);
	}
};
activatePackage(DNA_Scanner);
