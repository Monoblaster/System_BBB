datablock CameraData(Spectator){spectating = "";};

function Spectator::Spectate(%db,%camera,%obj)
{
	%camera.client.setControlObject(%camera);
	%camera.setOrbitMode(%obj,%camera.getTransform(),0,8,"");
	%camera.spectating = %obj;
}

function Spectator::Next(%db,%camera,%group,%inc)
{
	if(!isObject(%group))
	{
		return "";
	}

	%count = %group.getCount();
	if(%count > 0)
	{
		if(!isObject(%camera.spectating) || !%group.isMember(%camera.spectating))
		{
			%i = 0;
		}
		else
		{
			for(%i = 0; %i < %count; %i++)
			{
				%currObj = %group.getObject(%i);
				if(%camera.spectating == %currObj)
				{
					break;
				}
			}

			if(%inc)
			{
				%i++;
			}
			else
			{
				%i--;
			}
			
		}

		if(%i < 0)
		{
			%i = %count - 1;
		}
		else
		{
			%i %= %count;
		}
		
		%db.Spectate(%camera,%group.getObject(%i));
	}

	return "";
}

function Spectator::OnTrigger(%db,%camera,%num,%val)
{
	if(%val)
	{
		switch(%num)
		{
		case 0:
			%db.next(%camera,$testSet,false);
		case 4:
			%db.next(%camera,$testSet,true);
		}
	}
}

function SimObject::onCameraEnterOrbit(%obj, %camera){}

package spectate
{
	function GameConnection::onClientEnterGame(%client)
	{
		%client.spectateCamera = new Camera(){dataBlock = Spectator;client = %client;};
		return parent::onClientEnterGame(%client);
	}
};
activatePackage("spectate");