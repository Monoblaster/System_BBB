$DataInstance::FilePath = "Config/Server/DataInstance";
if(!isObject($DataInstance::Group))
{
	$DataInstance::Group = new ScriptGroup();
}

function DataInstance_ListDelete(%list)
{
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%curr = getWord(%list,%i);
		if(isObject(%curr))
		{
			%curr.delete();
		}
	}
}

function DataInstance_ListStringSerialize(%list)
{
	%curr = getWord(%list, 0);
	%s = "";
	if(isObject(%curr))
	{
		%s = %curr.StringSerialize();
	}
	%count = getWordCount(%list);
	for(%i = 1; %i < %count; %i++)
	{
		%curr = getWord(%list, %i);
		%serialized = "";
		if(isObject(%curr))
		{
			%serialized = %curr.StringSerialize();
		}
		%s = %s NL %serialized;
	}
	return %s;
}

function DataInstance::OnAdd(%obj)
{
	$DataInstance::Group.add(%obj);
}

function DataInstance::OnRemove(%obj)
{
	$DataInstance::Group.remove(%obj);
	if(%obj.DataInstance_List !$= "")
	{
		DataInstance_ListDelete(%obj.DataInstance_List);
	}
}

function DataInstance_Create(%obj,%index)
{
	%d = new ScriptObject(){class = "DataInstance";DataInstance_Parent = %obj;};
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%index,%d);
	return %d;
}

// if index is empty returns nothing
function SimObject::GetDataInstance(%obj,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13)
{
	%c = 0;
	while(%a[%c] !$= "")
	{
		%d = getWord(%obj.DataInstance_List,%a[%c]);
		if(%d $= "")
		{
			return "";
		}
		%obj = %d;
		%c++;
	}
	return %d;
}

function SimObject::DataInstance(%obj,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13)
{
	%c = 0;
	while(%a[%c] !$= "")
	{
		%d = getWord(%obj.DataInstance_List,%a[%c]);
		if(%d $= "")
		{
			%d = DataInstance_Create(%obj,%a[%c]);
		}
		%obj = %d;
		%c++;
	}
	return %d;
}

function SimObject::DataInstance_MoveTo(%obj,%from,%target,%to)
{
	%curr = getWord(%target.DataInstance_List,%to);
	if(%curr !$= "")
	{
		%curr.delete();
	}

	%target.DataInstance_List = setWord(%target.DataInstance_List,%to,getWord(%obj.DataInstance_List,%from));
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%from,"");
	return "";
}

function SimObject::DataInstance_Set(%obj,%slot,%s)
{
	%curr = getWord(%obj.DataInstance_List,%slot);
	if(%curr !$= "")
	{
		%curr.delete();
	}
	%s = getWord(%s,0);
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%slot,%s);
	return "";
}

function SimObject::DataInstance_Add(%obj,%s,%slot)
{
	if(%slot $= "")
	{
		%slot = getWordCount(%obj.DataInstance_List);
	}
	%s = getWord(%s,0);
	%obj.DataInstance_List = setWord(%obj.DataInstance_List,%slot,trim(%s SPC getWord(%obj.DataInstance_List,%slot)));
	return "";
}

function SimObject::DataInstance_ListGet(%obj)
{
	return %obj.DataInstance_List;
}


function SimObject::DataInstance_ListSet(%obj,%s)
{
	%obj.DataInstance_List = %s;
	return "";
}

function SimObject::DataInstance_ListSave(%obj)
{
	%id = %obj.DataIdentifier();
	if(%id $= "")
	{
		warn("Cannot save " @ %obj.getClassName());
		return;
	}
	%fo = new FileObject();
	%s = DataInstance_ListStringSerialize(%obj.DataInstance_List);
	%fo.close();
	if(%fo.OpenForWrite($DataInstance::FilePath @ "/" @ %id @ ".cs"))
	{
		%fo.writeLine(%s);
	}
	%fo.close();
	%fo.delete();
	return %id;
}

function SimObject::DataInstance_ListLoad(%obj)
{
	%id = %obj.DataIdentifier();
	if(%id $= "")
	{
		warn("Cannot load " @ %obj.getClassName());
		return "";
	}

	DataInstance_ListLoad($DataInstance::FilePath @ "/" @ %id @ ".cs", %obj);
}

function DataInstance_ListLoad(%path,%parent)
{
	%fo = new FileObject();
	if(%fo.OpenForRead(%path))
	{
		%c = 0;
		while(!%fo.isEOF())
		{
			%o = eval(%fo.readLine());
			%o.DataInstance_Parent = %parent;
			%data[%c] = %o;
			%c++;
		}
	}
	%fo.close();
	%fo.delete();

	DataInstance_ListDelete(%parent.DataInstance_List);

	if(%c == 0)
	{
		%parent.DataInstance_List = "";
		return "";
	}

	//unwrapped first loop to ensure propper formatting without trimming
	%currData = %data[0];
	%s = %currData;
	if(isObject(%currData))
	{
		%currData.DataInstance_ListLoad();
	}
	
	for(%i = 1; %i < %c; %i++)
	{
		%currData = %data[%i];
		%s = %s SPC %currData;
		if(isObject(%currData))
		{
			%currData.DataInstance_ListLoad();
		}
	}
	
	%parent.DataInstance_List = %s;
}

function SimObject::DataIdentifier(%obj,%append)
{
	return "";
}

function Player::DataIdentifier(%obj,%append)
{
	%c = %obj.client;
	if(isObject(%c))
	{
		return %obj.getClassName() @ %c.getBLID() @ %append;
	}
}

function GameConnection::DataIdentifier(%obj,%append)
{
	return %obj.getClassName() @ %obj.getBLID() @ %append;
}

function MinigameSO::DataIdentifier(%obj,%append)
{
	return %obj.class @ %obj.getName() @ %append;
}


function fxDtsBrick::DataIdentifier(%obj,%append)
{
	return %obj.getClassName() @ %obj.getTransform() @ %obj.getDatablock() @ %append;
}

function DataInstance::DataIdentifier(%obj,%append)
{
	%parent = %obj.DataInstance_Parent;
	%list = %parent.DataInstance_List;
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		if(getWord(%list,%i) == %Obj)
		{
			return %parent.DataIdentifier(%obj.class @ %i) @ %append;
		}
	}
	return "";
}

function DataInstance::StringSerialize(%d)
{
	%s = "new" SPC %d.getClassName() @ "(){class = \"DataInstance\";";
	%c = 0;
	while((%field = %d.getTaggedField(%c)) !$= "")
	{
		%name = getField(%field,0);
		%value = getField(%field,1);
		if(%name $= "DataInstance_List")
		{
			%value = %d.DataInstance_ListSave();
		}
		%s = %s @ getSubStr(%name,0,1) @ "[\"" @ getSubStr(%name,1,999999) @ "\"]" @ "=\"" @ %value @ "\";";
		%c++;
	}
	%s = %s @ "};";
	return %s;
}

$DataInstance::Item = 0;
function DataInstance_GetFromThrower(%item)  //item support
{
    %player = findClientByBl_Id(%item.bl_id).player;
    if(!isObject(%player))
    {
		return "";
    }

	%itemData = %player.GetDataInstance($DataInstance::Item);
	%itemDataList = %itemData.DataInstance_List;
	if(%itemDataList $= "")
	{
		return "";
	}
	%count = getWordCount(%itemDataList);
	for(%i = 0; %i < %count; %i++)
	{
		if(!isObject(%player.tool[%i]) && getWord(%itemDataList,%i) !$= "")
		{
			%itemData.DataInstance_MoveTo(%i,%item,0);
		}
	}
}

package SimObject_OnRemove
{
	function SimObject::OnRemove(%data,%obj)
	{
		return "";
	}
};

if(!isFunction("SimObject","OnRemove"))
{
	activatePackage("SimObject_OnRemove");
}

package DataInstance
{
	function Armor::OnRemove(%data,%obj) //cleanup
	{
		if(%obj.DataInstance_List !$= "")
		{
			DataInstance_ListDelete(%obj.DataInstance_List);
		}
		parent::OnRemove(%data,%obj);
	}

	function fxDtsBrick::OnRemove(%data,%obj) //cleanup
	{
		if(%obj.DataInstance_List !$= "")
		{
			DataInstance_ListDelete(%obj.DataInstance_List);
		}
		parent::OnRemove(%data,%obj);
	}

	function Vehicle::OnRemove(%data,%obj) //cleanup
	{
		if(%obj.DataInstance_List !$= "")
		{
			DataInstance_ListDelete(%obj.DataInstance_List);
		}
		parent::OnRemove(%data,%obj);
	}

	function SimObject::OnRemove(%data,%obj) //cleanup
	{
		if(%obj.DataInstance_List !$= "")
		{
			DataInstance_ListDelete(%obj.DataInstance_List);
		}
		parent::OnRemove(%data,%obj);
	}

	function ItemData::onPickup (%this, %obj, %user, %amount) //item support
    {
        //sigh looks like i have to play "find the difference"
        %maxTools = %user.getDatablock().maxTools;
        for(%i = 0; %i < %maxTools; %i++)
        {
            %before[%i] = %user.tool[%i];
        }

		%itemData = %obj.GetDataInstance(0);
		if(%itemData $= "")
		{
			return parent::onPickup(%this, %obj, %user, %amount);
		}
		%obj.DataInstance_ListSet("");
        %r = parent::onPickup(%this, %obj, %user, %amount);

		for(%i = 0; %i < %maxTools; %i++)
		{
			if(%before[%i] != %user.tool[%i])
			{
				%user.dataInstance($DataInstance::Item).DataInstance_Set(%i,%itemData);
				return %r;
			}
		}
        
        return %r;
    }

    function ItemData::OnAdd(%db, %obj) //item support
    {
    	schedule(0,%obj,"DataInstance_GetFromThrower",%obj);
        return Parent::OnAdd(%db, %obj);
    }
};
activatePackage("DataInstance");