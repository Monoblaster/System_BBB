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

datablock fxLightData(normalRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1.35;
    ConstantSizeOn = true;
    FadeTime = inf;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "0 1 0 1";

	AnimOffsets = true;
	startOffset = "0 0 1.25";
	endOffset = "0 0 1.25";
};

datablock fxLightData(traitorRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1.35;
    ConstantSizeOn = true;
    FadeTime = inf;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "1 0 0 1";

	AnimOffsets = true;
	startOffset = "0 0 1.25";
	endOffset = "0 0 1.25";
};

package tttbillboards
{
	function BBB_Minigame::roundSetup(%so)
	{
		%r = Parent::roundSetup(%so);
		//loop through minigame members and clear xray
		%count = %so.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%client = %so.member[%i];
			%client.avBillboardGroup.clear("xray");
		}
	}

    function Armor::onDisabled(%this, %obj, %state)
    {
        if(isObject(%obj.rolebillboard))
        {
            Billboard_Delete(%obj.rolebillboard);
        }
        return Parent::onDisabled(%this, %obj, %state);
    }

    function Player::Delete(%this)
    {
        if(isObject(%this.rolebillboard))
        {
            Billboard_Delete(%this.rolebillboard);
        }
        Parent::Delete(%this);
    }

	//when the player is first spawned create their always billboards group
	function GameConnection::onClientEnterGame(%client)
	{	
		%r = Parent::onClientEnterGame(%client);
		%client.centerPrint("\c6Welcome To TTT! Yes that's me talking to you!<br>\c5Click when you're ready to join.");
		%client.avBillboardGroup = %group = AVBillboards_Create("BillboardMount",30);
		%group.load(%client,"0 0 1000");
		return %r;
	}
	
	function BillboardLoadingCamera::OnTrigger(%data,%camera,%triggerNum,%triggerVal)
	{
		parent::OnTrigger(%data,%camera,%triggerNum,%triggerVal);
		%group = %camera.loading;
		%client = %group.loadedClient;
		%client.centerPrint("");
		if(!isObject(%client.player))
		{
			%client.setcontrolObject(%client.camera);
		}
	}

	function GameConnection::onClientLeaveGame(%client)
	{
		if(isObject(%client.visibleBillboardGroup))
		{
			%client.avBillboardGroup.delete();
		}
		
		return Parent::onClientLeaveGame(%client);
	}
};
deactivatePackage("tttbillboards");
activatePackage("tttbillboards");