//########## Night Vision Goggles
$Xray::Battery = 60000; // in ms
$Xray::Battery::Enabled = false;
//### Sounds

datablock AudioProfile(XrayOnSound)
{
  filename = "./xon.wav";
  description = Audio2d;
  preload = true;
};

datablock AudioProfile(XrayOffSound)
{
  filename = "./xoff.wav";
  description = Audio2d;
  preload = true;
};

//### Item

datablock ItemData(XrayItem)
{
  uiName = "XRay";
  iconName = "./icon_nv";
  image = XrayImage;
  category = "Tools";
  className = "Weapon";
  shapeFile = "./xitem.dts";
  mass = 0.5;
  density = 0.2;
  elasticity = 0;
  friction = 0.6;
  emap = true;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";
  canDrop = true;
  
  // BBB
  singleBuy = true;
  description = "A radar that utilizes your centerprint to show the distance to other players.";
};

//### Item Image

datablock ShapeBaseImageData(XrayImage)
{
  shapefile = "base/data/shapes/empty.dts";
  emap = true;
  mountPoint = 0;
  offset = "0 0 0";
  eyeOffset = "0 0 0";
  rotation = eulerToMatrix("0 0 0");
  className = "WeaponImage";
  item = XrayItem;
  melee = false;
  doReaction = false;
  armReady = false;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";

  stateName[0] = "Activate";
  stateTimeoutValue[0] = 0.1;
  stateTransitionOnTimeout[0] = "Ready";

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

function XrayImage::onFire(%this,%obj,%slot)
{

  %client = %obj.client;
  %player = %obj;

  if(isObject(%player))
  {
    if(%player.hasRadar)
    {
	  cancel(%player.XrayBatterySchedule);	
	  
	  // %player.unmountImage(2);
      %player.hasRadar = false;
      %client.play2D(XrayOffSound);
	  commandToClient(%client, 'clearCenterPrint');
    }
    else
    {
	  // Xray is now on.
	  if(%player.xRayBattery $= "")
		%player.xRayBattery = 100;
	  if(%player.xRayBattery > 0)
	  {
		  XrayBatteryLoop(%player);
		  // %player.unmountImage(2);
		  // %player.mountImage(XrayScopeImage,2);
		  // %player.unmountImage(1);
		  // %player.mountImage(x1Image,1);
		  %player.hasRadar = true;
		  RadarSilentLoop();
			// for(%a = 0; %a < ClientGroup.getCount(); %a++)
			// {
				// %c = ClientGroup.getObject(%a);
				// %cp = %c.player;
				// if(isObject(%cp))
					// %cp.scopeToClient(%client);
			// }
			// %count = missionCleanup.getCount();
			// %client.resetGhosting();
			// %client.activateGhosting();
			// for(%i = %count - 1; %i > 0; %i--)
			// {
				// %col = missionCleanup.getObject(%i);
				
				// if(%col.getType() & $TypeMasks::PlayerObjectType)
				// {	
					// talk(%col);
					// %net = "NetConnection" SPC %client;
					// %col.scopeToClient(%client);
					// missionCleanup.bringToFront(%obj);
				// }
			// }	
			
			// for(%a = 0; %a < ClientGroup.getCount(); %a++)
			// {
				// %c = ClientGroup.getObject(%a);
				// %cp = %c.player;
				// if(isObject(%cp))	
					// %cp.scopeToClient(%client);
			// }
	
		  %client.play2D(XrayOnSound);
	  }
	  else
	  {
		  %client.play2D(XrayOffSound);
		  %client.centerPrint("Battery empty!", 1);
	  }
    }
  }
}

function XrayBatteryLoop(%player)
{
	cancel(%player.XrayBatterySchedule);
	if($Xray::Battery::Enabled == false)
		return;
	if(!isObject(%player))
		return;
		
	%client = %player.client;

	%hudLevel = mFloor(%player.xRayBattery / 10);
	%bars = "";
	for(%i = 0; %i < %hudLevel; %i++)
		%bars = %bars @ "|";

//	%client.centerPrint("<just:left>\c4Battery\c6: " @ %bars, 1);
	%client.addPrint = "<just:left>\c4Battery\c6: " @ %bars;
	if(%player.xRayBattery <= 0)
	{
	  %player.hasRadar = false;
      %client.play2D(XrayOffSound);

	  %client.centerPrint("Battery empty!", 2);
	  return;
	}
	
	%divisor = mFloor(100 / ($Xray::Battery / 1000));
	%player.xRayBattery -= %divisor;	
	%player.XrayBatterySchedule = schedule(1000, %player, XrayBatteryLoop, %player);
}

package XrayPackage
{
  function servercmdDropTool(%this,%slot)
  {
    %player = %this.player;

    if(isobject(%player.tool[%slot]))
    {
      if(%player.tool[%slot].getname() $= "XrayItem")
      {
        parent::servercmdDropTool(%this,%slot);

        if(%player.hasRadar && !%player.hasDNAScanner) 
        {
          %player.hasRadar = false;
        }
        return;
      }
      
      
    }
    parent::servercmdDropTool(%this,%slot);
  }
};
activatepackage(XrayPackage);

//### Special Images

datablock ShapeBaseImageData(XrayScopeImage)
{
  shapeFile = "./x1.dts";
  emap = true;
  mountPoint = $HeadSlot;
  offset = "0 0 0";
  eyeOffset = "0 0 -25";
  rotation = eulerToMatrix("0 0 0");
  eyeRotation = eulerToMatrix("90 0 0");
  scale = "1 1 1";
  correctMuzzleVector = true;
  doColorShift = false;
  colorShiftColor = "1 1 1 1";
};
// datablock ShapeBaseImageData(x2Image)
// {
  // shapeFile = "./x2.dts";
  // emap = true;
  // mountPoint = $HeadSlot;
  // offset = "0 0 0";
  // eyeOffset = "0 0 0";
  // rotation = eulerToMatrix("0 0 0");
  // eyeRotation = eulerToMatrix("90 0 0");
  // scale = "1 1 1";
  // correctMuzzleVector = true;
  // doColorShift = true;
  // colorShiftColor = "1 1 1 0.1";
 // hasLight = true;
 // lightType = "ConstantLight";
 // lightColor = "0.5 1 0.5 0.05";
 // lightTime = "1000";
 // lightRadius = "50";
// };