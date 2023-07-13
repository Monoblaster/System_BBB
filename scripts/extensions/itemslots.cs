function ItemData::getSlot(%iDB,%player)
{
	%pDB = %player.getDataBlock();
	%slotCount = %pDB.maxTools;
	%itemTags = %iDB.ItemSlots_Tags;
	%itemTagsCount = getWordCount(%itemTags);

	if(%iDB.ItemSlots_Tags $= "")
	{
		for(%i = 0; %i < %slotCount; %i++)
		{
			if(%pDB.ItemSlots_Tag[%i] $= "" && !isObject(%player.tool[%i]))
			{
				return %i;
			}
		}

		return "";
	}

	for(%i = 0; %i < %slotCount; %i++)
	{
		if(isObject(%player.tool[%i]))
		{
			continue;
		}

		%currSlotTag = 	%pDb.ItemSlots_Tag[%i];
		for(%j = 0; %j < %itemTagsCount; %j++)
		{
			%currItemTag = getWord(%itemTags,%j);
			if(%currSlotTag $= %currItemTag)
			{
				return %i;
			}
		}
	}

	return "";
}

function ItemData::getSlotTag(%iDB,%player)
{
	%pDB = %player.getDataBlock();
	%slotCount = %pDB.maxTools;
	%itemTags = %iDB.ItemSlots_Tags;
	%itemTagsCount = getWordCount(%itemTags);

	for(%i = 0; %i < %slotCount; %i++)
	{
		%currSlotTag = 	%pDb.ItemSlots_Tag[%i];
		for(%j = 0; %j < %itemTagsCount; %j++)
		{
			%currItemTag = getWord(%itemTags,%j);
			if(%currSlotTag $= %currItemTag)
			{
				return %currSlotTag;
			}
		}
	}

	return "";
}

function ItemData::onPickup (%this, %obj, %user, %amount)
{
	if (%obj.canPickup == 0)
	{
		return 0;
	}

	// Get slot
	%slot = %this.getSlot(%user);

	if(%slot !$= "")
	{
		if(%obj.isStaic())
		{
			%obj.respawn();
		}
		else
		{
			%obj.delete ();
		}	
		
		%user.tool[%slot] = %this;
		if (%user.client)
		{
			messageClient (%user.client, 'MsgItemPickup', '', %slot, %this.getId());
		}
		return 1;
	}

	//create error message
	if (%user.client)
	{
		%tag = %this.getSlotTag(%user);
		if(%tag $= "")
		{
			%user.client.centerprint("\c6You do not have a slot for this item.",2);
		}
		else
		{
			%user.client.centerprint("\c6Your\c3" SPC %this.getSlotTag(%user) SPC "\c6slot is full.",2);
		}
		
	}
	

	return 0;
}

function Player::ItemSlots_UpdateEmptySlots(%player)
{
	%pDB = %player.getDataBlock();
	%slotCount = %pDB.maxTools;
	for(%i = 0; %i < %slotCount; %i++)
	{
		if(isObject(%player.tool[%i]))
		{
			continue;
		}

		if(!isObject(%pDB.ItemSlots_EmptyItem[%i]))
		{
			continue;
		}

		%player.ItemSlots_MessageItemBypass = true;
		messageClient(%player.client,'MsgItemPickup','',%i,%pDB.ItemSlots_EmptyItem[%i].GetId(),true);
		%player.ItemSlots_MessageItemBypass = false;
	}
}

package ItemSlots
{
	function PlayerData::OnAdd(%db,%obj)
	{
		%obj.ItemSlots_UpdateEmptySlots();
		return parent::OnAdd(%db,%obj);
	}

	function messageClient(%client,%msgType,%msgString,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13)
	{
		%r = parent::messageClient(%client,%msgType,%msgString,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13);

		%player = %client.player;
		if(isObject(%player))
		{
			if(!%player.ItemSlots_MessageItemBypass && %msgType == getTag("MsgItemPickup"))
			{
				%player.ItemSlots_UpdateEmptySlots();
			}
		}

		return %r;
	}
};
activatepackage("ItemSlots");