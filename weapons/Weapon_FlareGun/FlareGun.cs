AddDamageType("Flare_Impact");
AddDamageType("Flare_Burn");
datablock ProjectileData(FlareProjectile)
{
   uiName = "Flare";	
   projectileShapeName = "./Flare Projectile.dts";
   scale = "1.4 1.4 1.4";
   directDamage        = 10;
   burnDamage          = 1;
   directDamageType    = $DamageType::Flare_Impact;
   burnDamageType    = $DamageType::Flare_Burn;
   radiusDamageType    = $DamageType::Flare_Impact;

   brickExplosionRadius = 0;
   brickExplosionImpact = true;          //destroy a brick if we hit it directly?
   brickExplosionForce  = 10;
   brickExplosionMaxVolume = 1;          //max volume of bricks that we can destroy
   brickExplosionMaxVolumeFloating = 2;  //max volume of bricks that we can destroy if they aren't connected to the ground

   impactImpulse	     = 100;
   verticalImpulse	  = 100;
   explosion           = gunExplosion;
   particleEmitter     = ""; //bulletTrailEmitter;

   muzzleVelocity      = 200;
   velInheritFactor    = 1;

   armingDelay         = 00;
   lifetime            = 6000;
   fadeDelay           = 3500;
   bounceElasticity    = 0.5;
   bounceFriction      = 0.20;
   isBallistic         = true;
   gravityMod = 0.5;

   hasLight    = true;
   lightRadius = 20.0;
   lightColor  = "1 0.3 0 1";
};

datablock ItemData(FlareItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./Flare Gun.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Flare Gun";
	iconName = "";
	doColorShift = false;
	colorShiftColor = "0.4 0.4 0.4 1.000";

	 // Dynamic properties defined by the scripts
	image = FlareImage;
	canDrop = true;
   
   //Ammo Guns Parameters
   typeAmmo = "Flare";
   maxAmmo = 1;
   canReload = 1;
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(FlareImage)
{
   // Basic Item properties
   shapeFile = "./Flare Gun.dts";
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
   item = FlareItem;
   ammo = " ";
   projectile = FlareProjectile;
   projectileType = Projectile;

	casing = "";
	shellExitDir        = "1.0 -1.3 1.0";
	shellExitOffset     = "0 0 0";
	shellExitVariance   = 15.0;	
	shellVelocity       = 7.0;

   //melee particles shoot from eye node for consistancy
   melee = false;
   //raise your arm up or not
   armReady = true;

   doColorShift = false;
   colorShiftColor = FlareItem.colorShiftColor;//"0.400 0.196 0 1.000";

   //casing = " ";

   // Images have a state system which controls how the animations
   // are run, which sounds are played, script callbacks, etc. This
   // state system is downloaded to the client so that clients can
   // predict state changes and animate accordingly.  The following
   // system supports basic ready->fire->reload transitions as
   // well as a no-ammo->dryfire idle state.

   // Initial start up state
   stateName[0]                    = "Activate";
   stateTimeoutValue[0]            = 0.15;
   stateSequence[0]     = "Activate";
   stateTransitionOnTimeout[0]     = "LoadCheckA";
   stateSound[0]        = weaponSwitchSound;

   stateName[1]                    = "Ready";
   stateTransitionOnNoAmmo[1] = "ReloadStart";
   stateTransitionOnTriggerDown[1] = "Fire";
   stateAllowImageChange[1]        = true;
   stateScript[1]                  = "onReady";
   stateSequence[1]          = "ready";

   stateName[2]                    = "Fire";
   stateTransitionOnTimeout[2]     = "Smoke";
   stateTimeoutValue[2]            = 0.05;
   stateFire[2]                    = true;
   stateAllowImageChange[2]        = false;
   stateScript[2]                  = "onFire";
   stateSequence[2]     = "Fire";
   stateWaitForTimeout[2]     = true;
   stateEmitter[2]         = gunFlashEmitter;
   stateEmitterTime[2]     = 0.05;
   stateEmitterNode[2]     = "muzzleNode";
   stateSound[2]        = gunShot1Sound;

   stateName[3]         = "Smoke";
   stateEmitter[3]         = gunSmokeEmitter;
   stateEmitterTime[3]     = 0.1;
   stateEmitterNode[3]     = "muzzleNode";
   stateTransitionOnTriggerUp[3] = "Wait";

   stateName[4]         = "Wait";
   stateEjectShell[2]              = true;
   stateTimeoutValue[4]    = 0.2;
   stateScript[4]                  = "onBounce";
   stateTransitionOnTimeout[4]   = "LoadCheckA";
   
   //Torque switches states instantly if there is an ammo/noammo state, regardless of stateWaitForTimeout
   stateName[5]            = "LoadCheckA";
   stateScript[5]          = "onLoadCheck";
   stateTimeoutValue[5]       = 0.01;
   stateTransitionOnTimeout[5]      = "LoadCheckB";
   
   stateName[6]            = "LoadCheckB";
   stateTransitionOnAmmo[6]      = "Ready";
   stateTransitionOnNoAmmo[6]    = "ReloadWait";
   
   stateName[7]            = "ReloadWait";
   stateTimeoutValue[7]       = 1;
   stateScript[7]          = "";
   stateTransitionOnTimeout[7]      = "ReloadStart";
   stateWaitForTimeout[7]        = true;
   
   stateName[8]            = "ReloadStart";
   stateTimeoutValue[8]       = 1.1;
   stateScript[8]          = "onReloadStart";
   stateTransitionOnTimeout[8]      = "Reloaded";
   stateWaitForTimeout[8]        = true;
   
   stateName[9]            = "Reloaded";
   stateTimeoutValue[9]       = 0.3;
   stateScript[9]          = "onReloaded";
   stateTransitionOnTimeout[9]      = "Ready";
};

function FlareImage::onFire(%this,%obj,%slot)
{
   %obj.playThread(2, shiftAway);

   %obj.toolAmmo[%obj.currTool]--;
   %obj.AmmoSpent[%obj.currTool]++;
     // %obj.client.provideAmmoUpdate(%this.item.typeAmmo);

   %projectile = %this.projectile;
   %spread = 0.0001;

   %vector = %obj.getMuzzleVector(%slot);
   %objectVelocity = %obj.getVelocity();
   %vector1 = VectorScale(%vector, %projectile.muzzleVelocity);
   %vector2 = VectorScale(%objectVelocity, %projectile.velInheritFactor);
   %velocity = VectorAdd(%vector1,%vector2);
   %x = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
   %y = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
   %z = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
   %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
   %velocity = MatrixMulVector(%mat, %velocity);
   
   %p = new (%this.projectileType)()
   {
      dataBlock = %projectile;
      initialVelocity = %velocity;
      initialPosition = %obj.getMuzzlePoint(%slot);
      sourceObject = %obj;
      sourceSlot = %slot;
      client = %obj.client;
   };
}

function FlareProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal, %velocity)
{
	parent::onCollision(%this, %obj, %col, %fade, %pos, %normal, %velocity);
	if(%col.getType() & $TypeMasks::PlayerObjectType)
		return;

	%found = false;
	initContainerRadiusSearch(%pos, 0.1, $TypeMasks::CorpseObjectType | $TypeMasks::PlayerObjectType);
	while (isObject(%col = containerSearchNext()))
	{
		if(%col.isBody)
		{
			%found = true;
			break;
		}
	}
	if(%found)
		%col.flameClear();
}
function FlareImage::onMount(%this,%obj,%slot)
{
   Parent::onMount(%this,%obj,%slot);
      //%obj.client.provideAmmoUpdate(%this.item.typeAmmo); 
}

function FlareImage::onReloadStart(%this,%obj,%slot)
{
      //%obj.client.provideAmmoUpdate(%this.item.typeAmmo);
   if(%obj.client.quantity["Flare"] >= 1)
   {
      %obj.playThread(2, shiftRight);
      serverPlay3D(block_MoveBrick_Sound,%obj.getPosition());
   }
}

function FlareImage::onReloaded(%this,%obj,%slot)
{
    if(%obj.client.quantity["Flare"] >= 1)
   {

   %obj.playThread(2, plant);
   serverPlay3D(block_plantBrick_Sound,%obj.getPosition());
   %obj.playThread(3, shiftLeft);
        if(%obj.client.quantity["Flare"] > %this.item.maxAmmo)
   {
      %obj.client.quantity["Flare"] -= %obj.AmmoSpent[%obj.currTool];
      %obj.toolAmmo[%obj.currTool] = %this.item.maxAmmo;
      %obj.AmmoSpent[%obj.currTool] = 0;
      %obj.setImageAmmo(%slot,1);
      %obj.client.provideAmmoUpdate(%this.item.typeAmmo); 
      return;
   }

   if(%obj.client.quantity["Flare"] <= %this.item.maxAmmo)
   {
      %obj.client.exchangebullets = %obj.client.quantity["Flare"];
      %obj.toolAmmo[%obj.currTool] = %obj.client.exchangebullets;
      %obj.setImageAmmo(%slot,1);
      %obj.client.quantity["Flare"] = 0;
      %obj.client.provideAmmoUpdate(%this.item.typeAmmo);
      return;
   }
}
}
package Flare
{
   function FlareProjectile::damage(%this,%obj,%col,%fade,%pos,%normal)
   {
      if(%this.directDamage <= 0)
         return;

      %damageType = $DamageType::Direct;
      if(%this.DirectDamageType)
         %damageType = %this.DirectDamageType;

      %scale = getWord(%obj.getScale(), 2);
      %directDamage = mClampF(%this.directDamage, -50, 50) * %scale;

      if(%col.getType() & $TypeMasks::PlayerObjectType)
      {
         %col.damage(%obj, %pos, 10, $DamageType::Flare_Impact);
         %col.flareBurnSche = %col.schedule(2500,FlareBurn,%obj, %pos, %this.burnDamage, $DamageType::Flare_Burn,10);
      }
      else
      {
         %col.damage(%obj, %pos, %directDamage, %damageType);
      }
      // if(%col.getClassName $= "AIPlayer" && %col.Type !$= "RealHP")
      // {
         // %bot.flameClear();
      // }
      parent::damage(%this,%obj,%col,%fade,%pos,%normal);
   }
   function player::flameClear(%obj)
   {
      %obj.setNodeColor("ALL","0 0 0 1");
      %obj.Name = "???";
      %obj.Role = "???";
      %obj.deadTime = "";
	  	%obj.SOD = "???";
			%obj.lastWords = "???";
      %obj.fingerPrints = "";
	  %obj.setDecalName("");
	  %obj.setShapeName("", 8564862);
	  %obj.burnPlayer(5);
   }
   function player::FlareBurn(%player,%obj,%pos,%damage,%type,%ticks)
   {
      %player.burning = false;
      %player.damage(%obj, %pos, 1, %type);
      %ticks--;
      if(%ticks > 0)
      {
         %player.burning = true;
         %player.flareBurnSche = %player.schedule(1000,FlareBurn,%obj,%pos,%damage,$DamageType::Flare_Burn,%ticks);
      }
   }
   function armor::onEnterLiquid(%this, %obj, %coverage, %type)
   {
      if(%type < 4 && %obj.getClassName() $= "Player" && %obj.burning)
      {
         cancel(%obj.flareBurnSche);
      }
      parent::onEnterLiquid(%this, %obj, %coverage, %type);
   }
};
activatepackage(Flare);