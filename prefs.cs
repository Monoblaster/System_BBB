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

trap_healthImage.TTT_notWeapon = true;

grenade_mollyImage.TTT_Contraband = true;
grenade_dynamiteImage.TTT_Contraband = true;
grenade_remoteImage.TTT_Contraband = true;
mine_proxyImage.TTT_Contraband = true;
mine_incendiaryImage.TTT_Contraband = true;
ThrowingKnifeImage.TTT_Contraband = true;

deactivatePackage("WeaponDropCharge");