function Billboard_Create(%lightDB,%mountDB)
{
	if(!isObject(%lightDB))
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	if(!isObject(%mountDB))
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid player datablock");
		return;
	}

	if(%lightDB.getClassName() !$= "fxLightData")
	{
		warn("Billboard_Create: " @ %mountDB @ " is not a valid fxLight datablock");
		return;
	}

	if(%mountDB.getClassName() !$= "PlayerData")
	{
		warn("Billboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	%billboard = new fxLight()
	{
		dataBlock = %lightDB;
	};

	%mount = new aiPlayer()
	{
		dataBlock = %mountDB;
	};

	%mount.light = %billboard;
	%billboard.attachToObject(%mount);
	%mount.setDamageLevel(10000);

	//ghost to prevent it from not ghosting
	Billboard_GhostTo(%mount,"ALL");

	return %mount;
}

function Billboard_GhostTo(%billboard,%client)
{
	if(%client $= "ALL")
	{
		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{	
			%client = %group.getObject(%i);
			Billboard_GhostTo(%billboard,%client);
		}
		return %billboard;
	}

	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_GhostTo: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_GhostTo: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(!isObject(%client))
	{
		warn("Billboard_GhostTo: " @ %client @ " is not a valid client");
		return;
	}

	if(%client.getClassName() !$= "GameConnection")
	{
		warn("Billboard_GhostTo: " @ %client @ " is not a valid client");
		return;
	}

	%Billboard.setNetFlag(6,true);
	%billboard.ScopeToClient(%client);
	%Billboard.setNetFlag(6,true);
	%Billboard.light.setNetFlag(6,true);
	%billboard.light.ScopeToClient(%client);
	%Billboard.light.setNetFlag(6,true);

	return %billboard;
}

function Billboard_ClearGhostTo(%billboard,%client)
{
	if(%client $= "ALL")
	{
		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{	
			%client = %group.getObject(%i);
			Billboard_ClearGhostTo(%billboard,%client);
		}
		return %billboard;
	}

	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_ClearGhostTo: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_ClearGhostTo: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(!isObject(%client))
	{
		warn("Billboard_ClearGhostTo: " @ %client @ " is not a valid client");
		return;
	}

	if(%client.getClassName() !$= "GameConnection")
	{
		warn("Billboard_ClearGhostTo: " @ %client @ " is not a valid client");
		return;
	}

	%Billboard.setNetFlag(6,true);
	%billboard.ClearScopeToClient(%client);
	%Billboard.setNetFlag(6,true);
	%Billboard.light.setNetFlag(6,true);
	%billboard.light.ClearScopeToClient(%client);
	%Billboard.light.setNetFlag(6,true);

	return %billboard;
}

function Billboard_MountToPlayer(%billboard,%player)
{	
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_MountToPlayer: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_MountToPlayer: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(isObject(%player.billboard))
	{
		warn("Billboard_MountToPlayer: Billboard already mounted to " @ %player);
		return;
	}

	%player.MountObject(%billboard,8);
	%player.billboard = %billboard;

	return %billboard;
}

function Billboard_UmMountFromPlayer(%player)
{	
	if(!isObject(%player.billboard))
	{
		warn("Billboard_UmMountFromPlayer: Billboard is not already mounted to " @ %player);
		return;
	}

	%billboard = %player.billboard;
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_UmMountFromPlayer: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_UmMountFromPlayer: " @ %billboard @ " is not a valid billboard");
		return;
	}
    
	%player.unMountObject(8);

	return %billboard;
}

function Billboard_Delete(%billboard)
{	
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_Delete: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_Delete: " @ %billboard @ " is not a valid billboard");
		return;
	}

	%billboard.light.delete();
	%billboard.delete();
}

package billboards
{
    function Armor::onDisabled(%this, %obj, %state)
    {
        if(isObject(%obj.billboard))
        {
            Billboard_Delete(%obj.billboard);
        }
        return Parent::onDisabled(%this, %obj, %state);
    }

    function Player::Delete(%this)
    {
        if(isObject(%this.billboard))
        {
            Billboard_Delete(%this.billboard);
        }
        Parent::Delete(%this);
    }
};
activatePackage("billboards");

datablock PlayerData(BillboardMountPlayer)
{
    emap = true;
    shapeFile = "./billboardMount.dts";
};

//custom billboards
datablock fxLightData(detectiveBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./detective.png";
	ConstantSize = 0.5;
    ConstantSizeOn = true;
    FadeTime = 0.1;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "0 0 1 1";
};

datablock fxLightData(traitorBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
    ConstantSizeOn = true;
    FadeTime = 0.1;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "1 0 0 1";
};