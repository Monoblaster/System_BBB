datablock ItemData(BodyArmorItem : HammerItem)
{
	uiName = "Body Armor";
	iconName = "./icon_TacticalVest";
}

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
		%client.play2D(XrayOffSound);
	}
	
	%player.DamageMultiplier = 0.50 ;

	%item.delete();
}

package BodyArmorPackage
{
	function Armor::Damage(%db, %target, %source, %pos, %damage, %damageType)
	{
		%multiplier = 1;
		if(%target.DamageMultiplier !$= "")
		{
			%multiplier = %target.DamageMultiplier;
		}

		%damage *= %damage;
		parent::Damage(%db, %target, %source, %pos, %damage, %damageType);
	}
};
activatePackage("BodyArmorPackage");