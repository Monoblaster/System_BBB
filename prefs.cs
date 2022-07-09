$Pref::RBloodMod::LimbReddening = false;
$Pref::BHole::FadeDelay = 250; // < i fucked up the defaults for these
$Pref::BHole::OpacityR = 0.1;  // < ^
$Pref::BHole::ColorShiftToBrick = true;
$Pref::BHole::Restrictions = false;
$Pref::AEBase::HUDPos = 3; 
$Pref::AEBase::HUD = 9;
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

//disable weapon ammo pickups
package aeAmmo
{
	function Armor::onCollision(%this, %obj, %col, %vec, %speed)
	{
		return parent::onCollision(%this, %obj, %col, %vec, %speed);
	}
};

package WeaponDropCharge
{
	function Player::Pickup(%pl, %item)
	{
		%db = %pl.getDatablock();
		if(%item.weaponCharges !$= "" && %item.canPickup && %pl.getDamagePercent() < 1.0 && minigameCanUse(%pl, %item))
		{
			%ammo = %item.weaponCharges;
			%data = %item.getDatablock();
			%empties = -1;
			for(%i = 0; %i < %pl.getDatablock().maxTools; %i++)
			{
				if(!isObject(%pl.tool[%i]))
					%empties = %empties TAB %i;
			}

			%empties = removeField(%empties, 0);

			if((%idx = %pl.itemLookup(%data)) == -1 && getFieldCount(%empties) > 0 || %data.canPickupMultiple && getFieldCount(%empties) > 0)
			{
				Parent::Pickup(%pl, %item);

				// if(isObject(%item.spawnBrick))
				// 	%item.respawn();
				// else
				// 	%item.schedule(10, delete);
				
				for(%i = 0; %i < getFieldCount(%empties); %i++)
				{
					%id = getField(%empties, %i);
					if(isObject(%itm = %pl.tool[%id]) && %itm.image.weaponUseCount > 0 && %itm == %data)
					{
						%pl.weaponCharges[%id] = %ammo;
						break;
					}
				}
			}
			else

			return;
		}
		
		return Parent::Pickup(%pl, %item);
	}
};