datablock PlayerData(FloatingBillboardPlayer)
{
    shapeFile = "base/data/shapes/empty.dts";
	boundingBox = vectorScale("20 20 20", 4);
};

datablock PlayerData(MountedBillboardPlayer)
{
    shapeFile = "./billboardMount.dts";
	boundingBox = vectorScale("20 20 20", 4);
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

datablock fxLightData(ghostRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1;
    ConstantSizeOn = true;
    FadeTime = 0.000001;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "1 0 0 1";

	AnimOffsets = true;
	startOffset = "0 0 2.2";
	endOffset = "0 0 2.2";
};

datablock fxLightData(normalRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1.25;
    ConstantSizeOn = true;
    FadeTime = 999999;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "0 1 0 1";

	AnimOffsets = true;
	startOffset = "0 0 1.25";
	endOffset = "0 0 1.25";
};

function Billboard_Create(%lightDB,%mountDB,%dontGhost)
{
	if(!isObject(%lightDB))
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	if(!isObject(%mountDB))
	{
		warn("Billboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	if(%lightDB.getClassName() !$= "fxLightData")
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid fxLight datablock");
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

	//move it up so it doesn't crash the game
	%mount = new aiPlayer()
	{
		dataBlock = %mountDB;
		position = "0 0 999999";
	};
	
	%mount.setDamageLevel(10000);
	%mount.setTransform("0 0 0");

	%mount.light = %billboard;
	%billboard.attachToObject(%mount);
	

	if(!%dontGhost)
	{
		//ghost to prevent it from not ghosting
		%billboard.setNetFlag(6,true);
		%billboard.setScopeAlways();
		%billboard.setNetFlag(6,true);
		%mount.setNetFlag(6,true);
		%mount.setScopeAlways();
		%mount.setNetFlag(6,true);
	}
	else
	{
		//clear scope to prevent it from ghosting ever
		%billboard.setNetFlag(6,true);
		%billboard.ClearScopeAlways();
		%billboard.setNetFlag(6,true);
		%mount.setNetFlag(6,true);
		%mount.ClearScopeAlways();
		%mount.setNetFlag(6,true);
	}

	return %mount;
}

function Billboard_Ghost(%billboard,%client)
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

function Billboard_ClearGhost(%billboard,%client)
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

function Billboard_Mount(%billboard,%player)
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

function Billboard_Ummount(%player)
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

function VisibleBillboard_Create(%client,%mountDB,%count)
{
	%client.visibleBillboardGroup = %group = new scriptGroup()
	{
		class = "VisibleBillboard";
	}

	%mount = new aiPlayer()
	{
		dataBlock = "FloatingBillboardPlayer";
	};
	%mount.setNetFlag(6,true);
	%mount.ClearScopeAlways();
	%mount.setNetFlag(6,true);
	for(%i = 0; %i < %count; %i++)
	{
		%billboard = Billboard_Create("GhostRadarBillboard",%mountDB,true);
		Billboard_Ghost(%billboard,%client);
		%billboard.light.attachToObject(%mount);

		schedule(%count * 5, 0, "FinishvisibleBillboard", %billboard, %group);		
	}

	return %group;
}

function FinishVisibleBillboard(%billboard,%group)
{
	%group.add(%billboard);
	%billboard.light.attachToObject(%billboard);
	%billboard.light.setEnable(false);
}

function VisibleBillboard::Billboard(%group,%lightDB,%position)
{
	%active = %group.active;
	if(%active >= %group.getCount())
	{
		warn("VisibleBillboard::Billboard: " @ %group @ " group over count");
		return;
	}
	
	//get an inactive billboard
	%billboard = %group.getObject(%active);
	%group.active++;

	//set it's datablock and enable
	%billboard.light.setEnable(true);
	%billboard.light.setDatablock(%lightDB);

	if(isObject(%position))
	{
		//mount to object
		%position.mountObject(%billboard,8);
	}
	else
	{
		%billboard.setTransform(%position);
	}
}

function VisibleBillboard::ClearBillboards(%group)
{
	%group.active = 0;

	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%billboard = %group.getObject(%i);
		//disable all of the lights and unmount if mounted
		%mount = %billboard.getObjectMount();
		if(isObject(%mount))
		{
			%mount.unmountObject(%billboard);
		}

		%billboard.light.setEnable(false);
	}
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