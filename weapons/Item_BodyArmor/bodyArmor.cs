datablock ItemData(BodyArmorItem : HammerItem)
{
	uiName = "Body Armor";
	iconName = "./icon_TacticalVest";
};

datablock AudioProfile(BodyArmorEquipSound)
{
  filename = "./CombatGear_Equip.wav";
  description = Audio2d;
  preload = true;
};

function BodyArmorItem::OnPickup(%this,%item,%player,%amount)
{
	%client = %player.client;
	if(isObject(%client))
	{
		%client.play2D(BodyArmorEquipSound);
	}
	
	%player.DamageMultiplier = 0.50 ;

	%item.schedule(0,"delete");
}

//outside of a pacakge to prevent package ordering weirdness
function Armor::Damage (%data, %obj, %sourceObject, %position, %damage, %damageType)
{
	%multiplier = 1;
	if(%obj.DamageMultiplier !$= "")
	{
		%multiplier = %obj.DamageMultiplier;
	}

	%damage *= %multiplier;

	if (%obj.getState () $= "Dead")
	{
		return;
	}
	if (getSimTime () - %obj.spawnTime < $Game::PlayerInvulnerabilityTime && !%obj.hasShotOnce)
	{
		return;
	}
	if (%obj.invulnerable)
	{
		return;
	}
	if (%obj.isMounted () && %damageType != $DamageType::Suicide && %data.rideAble == 0)
	{
		%mountData = %obj.getObjectMount ().getDataBlock ();
		if ($Damage::Direct[%damageType])
		{
			if (%mountData.protectPassengersDirect)
			{
				return;
			}
		}
		else if (%mountData.protectPassengersRadius)
		{
			return;
		}
	}
	if ($Damage::Direct[%damageType] == 1)
	{
		%obj.lastDirectDamageType = %damageType;
		%obj.lastDirectDamageTime = getSimTime ();
	}
	%obj.lastDamageType = %damageType;
	if (getSimTime () - %obj.lastPainTime > 300)
	{
		%obj.painLevel = %damage;
	}
	else 
	{
		%obj.painLevel += %damage;
	}
	%obj.lastPainTime = getSimTime ();
	if (%obj.isCrouched ())
	{
		if ($Damage::Direct[%damageType])
		{
			%damage = %damage * 2.1;
		}
		else 
		{
			%damage = %damage * 0.75;
		}
	}
	%scale = getWord (%obj.getScale (), 2);
	%damage = %damage / %scale;
	%obj.applyDamage (%damage);
	%location = "Body";
	%client = %obj.client;
	if (isObject (%sourceObject))
	{
		if (%sourceObject.getClassName () $= "GameConnection")
		{
			%sourceClient = %sourceObject;
		}
		else 
		{
			%sourceClient = %sourceObject.client;
		}
	}
	else 
	{
		%sourceClient = 0;
	}
	if (isObject (%sourceObject))
	{
		if (%sourceObject.getType () & $TypeMasks::VehicleObjectType)
		{
			if (%sourceObject.getControllingClient ())
			{
				%sourceClient = %sourceObject.getControllingClient ();
			}
		}
	}
	if (%obj.getState () $= "Dead")
	{
		if (isObject (%client))
		{
			%client.onDeath (%sourceObject, %sourceClient, %damageType, %location);
		}
		else if (isObject (%obj.spawnBrick))
		{
			%mg = getMiniGameFromObject (%sourceObject);
			if (isObject (%mg))
			{
				%obj.spawnBrick.spawnVehicle (%mg.VehicleRespawnTime);
			}
			else 
			{
				%obj.spawnBrick.spawnVehicle (5000);
			}
		}
	}
	else if (%data.useCustomPainEffects == 1)
	{
		if (%obj.painLevel >= 40)
		{
			if (isObject (%data.PainHighImage))
			{
				%obj.emote (%data.PainHighImage, 1);
			}
		}
		else if (%obj.painLevel >= 25)
		{
			if (isObject (%data.PainMidImage))
			{
				%obj.emote (%data.PainMidImage, 1);
			}
		}
		else if (isObject (%data.PainLowImage))
		{
			%obj.emote (%data.PainLowImage, 1);
		}
	}
	else if (%obj.painLevel >= 40)
	{
		%obj.emote (PainHighImage, 1);
	}
	else if (%obj.painLevel >= 25)
	{
		%obj.emote (PainMidImage, 1);
	}
	else 
	{
		%obj.emote (PainLowImage, 1);
	}
}