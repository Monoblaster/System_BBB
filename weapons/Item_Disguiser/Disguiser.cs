//### Item
datablock ItemData(DisguiserItem)
{
  uiName = "Disguiser";

  image = DisguiserImage;
  category = "Tools";
  className = "Weapon";
  shapeFile = "base/data/shapes/empty.dts";
  mass = 0.5;
  density = 0.2;
  elasticity = 0;
  friction = 0.6;
  emap = true;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";
  canDrop = false;
  
  // BBB
  singleBuy = true;
};

//### Item Image

datablock ShapeBaseImageData(DisguiserImage)
{
  shapeFile = "base/data/shapes/empty.dts";
  emap = true;
  mountPoint = 0;
  offset = "0 0 0";
  eyeOffset = "0 0 0";
  rotation = eulerToMatrix("0 0 0");
  className = "WeaponImage";
  item = DisguiserItem;
  melee = false;
  doReaction = false;
  armReady = false;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";

  TTT_notWeapon = true;

  stateName[0] = "Activate";
  stateTimeoutValue[0] = 0.1;
  stateTransitionOnTimeout[0] = "Ready";
  stateSound[0] = weaponSwitchSound;

  stateName[1] = "Ready";
  stateTransitionOnTriggerDown[1] = "Fire";
  stateAllowImageChange[1] = true;

  stateName[2] = "Fire";
  stateTransitionOnTimeOut[2] = "Ready";
  stateTimeoutValue[2] = "0.2";
  stateFire[2] = true;
  stateAllowImageChange[2] = true;
  stateScript[2] = "onFire";
};

function DisguiserImage::onFire(%this,%obj,%slot)
{
  %client = %obj.client;
  %player = %obj;
  if(isObject(%player))
  {
    if(!%player.isDisguised)
    {
		%player.setShapeName("", 8564862);
		%player.displayName = "Nobody";
		messageClient(%client, "", "\c4 - Disguiser: \c2ON");
		messageClient(%client, "", "\c4 - You are now hidden.");
		
		%player.isDisguised = true;
    }
    else
    {
		%player.setShapeName(%client.name, 8564862);
		%player.displayName = %client.name;
		messageClient(%client, "", "\c4 - Disguiser: \c0OFF");
		messageClient(%client, "", "\c4 - You are now visible to everyone.");
		
		%player.isDisguised = false;
    }
	%client.play2D(BBB_Chat_Sound);
  }
}
