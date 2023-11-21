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
function Armor::Damage(%db, %target, %source, %pos, %damage, %damageType)
{
	%multiplier = 1;
	if(%target.DamageMultiplier !$= "")
	{
		%multiplier = %target.DamageMultiplier;
	}

	%damage *= %multiplier;
	parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
}
