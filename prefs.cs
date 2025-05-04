$Pref::RBloodMod::LimbReddening = false;
$Pref::BHole::FadeDelay = 250; // < i fucked up the defaults for these
$Pref::BHole::OpacityR = 0.1;  // < ^
$Pref::BHole::ColorShiftToBrick = true;
$Pref::BHole::Restrictions = false;
$Pref::AEBase::HUDPos = 3; 
$Pref::AEBase::HUD = 9;
$Pref::AEBase::laserBlind = 0.3;
$Pref::AEBase::flashlightBlind = 0.3;
$Blood::MinDecalAmt = 1;
$Blood::MaxDecalAmt = 10;
$Blood::DeathDecalAmt = 20;
trap_healthImage.healthteam = true;
grenade_remoteImage.maxActive = 8;
$pref::aebase::projectilesas = 4;
function Player::WeaponAmmoPrint(%pl, %cl, %idx, %sit)
{
	return;
}

function grenade_dynamiteImage::onFire(%this, %obj, %slot)
{
	%obj.stopAudio(2);
	%obj.playThread(2, shiftDown);
	%obj.weaponAmmoUse();
	serverPlay3D(grenade_throwSound, %obj.getMuzzlePoint(%slot));
	%projs = ProjectileFire(%this.Projectile, %obj.getMuzzlePoint(%slot), %obj.getMuzzleVector(%slot), 0, 1, %slot, %obj, %obj.client);
	for(%i = 0; %i < getFieldCount(%projs); %i++)
	{
		%proj = getField(%projs, %i);
		%proj.cookDeath = %proj.schedule((%proj.getDatablock().lifeTime * 32) - (getSimTime() - %obj.chargeStartTime[%this]), FuseExplode);
	}

	%obj.chargeStartTime[%this] = "";
}

//disable weapon ammo pickups
package aeAmmo
{
	function Armor::onCollision(%this, %obj, %col, %vec, %speed)
	{
		return parent::onCollision(%this, %obj, %col, %vec, %speed);
	}
};

package ThrowingKnifePackage
{
	function Armor::onTrigger(%this, %player, %slot, %val)
	{
		if(%player.getMountedImage(0) $= ThrowingKnifeImage.getID() && %slot $= 4 && %val)
		{
			%projectile = ThrowingKnifeThrowProjectile;
			%vector = %player.getMuzzleVector(0);
			%objectVelocity = %player.getVelocity();
			%vector1 = VectorScale(%vector, %projectile.muzzleVelocity);
			%vector2 = VectorScale(%objectVelocity, %projectile.velInheritFactor);
			%velocity = VectorAdd(%vector1,%vector2);
			%p = new Projectile()
			{
				dataBlock = %projectile;
				initialVelocity = %velocity;
				initialPosition = %player.getMuzzlePoint(0);
				sourceObject = %player;
				sourceSlot = 0;
				client = %player.client;
		};
			%currSlot = %player.realCurrTool;
			%player.tool[%currSlot] = 0;
			%player.weaponCount--;
			messageClient(%player.client,'MsgItemPickup','',%currSlot,0);
			serverCmdUnUseTool(%player.client);
			%player.unmountImage(0);
			
			serverPlay3D(ThrowingKnifeSwingSound,%player.getPosition());
			%player.playthread("3","Activate");
			MissionCleanup.add(%p);
			return %p;
		}
		Parent::onTrigger(%this, %player, %slot, %val);
	}
};

trap_healthImage.TTT_notWeapon = true;
grenade_decoyImage.TTT_notWeapon = true;
grenade_stimImage.TTT_notWeapon = true;
grenade_smokeImage.TTT_notWeapon = true;

grenade_mollyImage.TTT_Contraband = true;
grenade_dynamiteImage.TTT_Contraband = true;
grenade_remoteImage.TTT_Contraband = true;
mine_proxyImage.TTT_Contraband = true;
mine_incendiaryImage.TTT_Contraband = true;
ThrowingKnifeImage.TTT_Contraband = true;

trap_healthItem.doColorShift = false; // OXY!!

function silenceWeaponEquip()
{
	%group = dataBlockGroup.getId();
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%data = %group.getObject(%i);
		if(%data.stateSound[0] $= "" || striPos(%data.shapefile,"TierTacMelee") == -1)
		{
			continue;
		}
		%name = %data.getName();
		%newName = "Silenced"@%name;
		eval("datablock ShapeBaseImageData("@%newName@" : "@%name@"){silenced=true;stateSound[0]=\"\";};"@
		"function "@%newName@"::onFire(%this,%obj,%slot){return "@%name@"::onFire(%this,%obj,%slot);}"@
		"function "@%newName@"::onActivate(%this,%obj,%slot){return "@%name@"::onActivate(%this,%obj,%slot);}"@
		"function "@%newName@"::onPreFire(%this,%obj,%slot){return "@%name@"::onPreFire(%this,%obj,%slot);}"@
		"function "@%newName@"::onStabFire(%this,%obj,%slot){return "@%name@"::onStabFire(%this,%obj,%slot);}"@
		"function "@%newName@"::onCharge(%this,%obj,%slot){return "@%name@"::onCharge(%this,%obj,%slot);}"@
		"function "@%newName@"::TT_isRaycastCritical(%this,%obj,%slot,%col,%pos,%normal,%hit){return "@%name@"::TT_isRaycastCritical(%this,%obj,%slot,%col,%pos,%normal,%hit);}");
		%data.item.image = %newName;
	}
}
silenceWeaponEquip();

deactivatePackage("WeaponDropCharge");
function L4BIceAxeImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() >= 1.0)
		return;

	%obj.playThread(2, shiftTo);

	if(getRandom(0,1))
	{
		%this.TT_raycastExplosionBrickSound = L4BMacheteHitSoundA;
	}
	else
	{
		%this.TT_raycastExplosionBrickSound = L4BMacheteHitSoundB;
	}

	Parent::onFire(%this, %obj, %slot);
}

function L4BIceAxeImage::onActivate(%this, %obj, %slot)
{
	%obj.playthread(2, plant);
}

function L4BIceAxeImage::onPreFire(%this, %obj, %slot)
{
	%obj.playthread(2, shiftAway);
}

function L4BIceAxeImage::TT_isRaycastCritical(%this,%obj,%slot,%col,%pos,%normal,%hit)
{
	if(!isObject(%col))
		return 0;
	
	return TT_isMeleeRaycastCrit(%this,%obj,%slot,%col,%pos,%normal,%hit) || (ae_calculateDamagePosition(%col, %pos) $= "head");
}

$DataInstance::FilePath = "Config/Server/DataInstance/TTT"; // to prevent ttt from nuking deathrace saves