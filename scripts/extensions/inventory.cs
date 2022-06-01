$inventory::UknownName = 0;

function Inventory_Create(%name,%display)
{
    %list = new ScriptObject()
	{
		superClass = "List";
        class = "Inventory";

        name = %name;

        display = %display;
	};
    %list.subscribedPlayers = new SimSet();
    return %list;
}

function InventorySlotItemData_Create(%name,%icon,%color,%unselectable)
{
    %dbName = getSafeVariableName(%name @ "Item");
    if(%dbName $= "")
    {
        %dbName = $inventory::UknownName @ getRandom(0,999999) @ "Item";
        $inventory::UknownName++;
    }

    %function = "datablock ItemData(" @ %dbName @ "){category = \"Tools\";uiName = %name;iconName = %icon;oColorShift = %color !$= \"\";colorShiftColor = %color;unselectable = %unselectable;};";
    eval(%function);
    %db = %dbName;
    %db.size = 1;
    if(isObject(%dbName))
    {
        return %dbName.getId();
    }
    return "";
}

function InventorySpace_Create(%name,%capacity,%emptySlotItem,%emptySlotMin,%select,%use,%equip,%drop)
{
    %slot = new ScriptObject(%name)
	{
        //inheritance
		superClass = "List";
        class = "InventorySpace";

        //display and capacity
        capacity = %capacity;
        emptySlotItem = %emptySlotItem;
        emptySlotMin = %emptySlotMin;
        
        //callbacks
        select = %select;
        drop = %drop;
        use = %use;
        equip = %equip;
	};

    return %slot;
}

function InventorySpace::CurrentCapacity(%space)
{
    //count all the size of the held items
    %capacity = %space.capacity;
    %count = %space.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %item = %space.getValue(%i);
        %size = %item.size;
        if(%size $= "" && isObject(%item))
        {
            %size = 1;
        }

        %capacity -= %size;
    }

    return %capacity;
}

function InventorySpace::CanEquip(%space,%item,%offest)
{
    %size = %item.size;
    return (%space.CurrentCapacity() + %offest) >= %size;
}

function InventorySpace::SetSlot(%space,%slot,%item,%silent)
{
    if(%space.CanEquip(%item))
    {
        %space.set(%slot,%item);
        %space.inventory.Display(%silent);

        %callback = %space.equip;
        if(isFunction(%callback))
        {
            %players = %space.inventory.subscribedPlayers;
            %count = %players.getCount();
            for(%i = 0; %i < %count; %i++)
            {
                %client = %players.getObject(%i).client;
                if(isObject(%client))
                {
                    call(%callback,%client,%space,%slot,1);
                }
            }
        }
        return true;
    }
    return false;
}

function InventorySpace::RemoveSlot(%space,%slot,%silent)
{
    if(%space.getValue(%slot) >= 0)
    {
        %space.set(%slot,"");
        %space.inventory.Display(%silent);

        %callback = %space.equip;
        if(isFunction(%callback))
        {
            %players = %space.inventory.subscribedPlayers;
            %count = %players.getCount();
            for(%i = 0; %i < %count; %i++)
            {
                %client = %players.getObject(%i).client;
                if(isObject(%client))
                {
                    call(%callback,%client,%space,%slot,0);
                }
            }
        }
        return true;
    }
    return false;
}

function InventorySpace::Equip(%space,%item)
{
    %c = 0;
    while(%space.getValue(%c) !$= "")
    {
        %c++;
    }
    return %space.setSlot(%c,%item,false);
}

function InventorySpace::Unequip(%space,%item)
{
    %slot = %space.FindValue(%item);
    return %space.removeSlot(%slot);

}

function Inventory_Push(%player,%inventory)
{
    if(!isObject(%player.inventoryStack))
    {
        %player.InventoryStack = List_NewList();
    }

    if(%inventory.class !$= "Inventory")
    {
        warn("Inventory_Push: Not a valid inventory " @ %inventory.getid());
        return;
    }
    //add player to the update list
    %inventory.subscribedPlayers.add(%player);

    %stack = %player.inventoryStack;
    %stack.add(%inventory);

    %client = %player.client;
    if(isObject(%client))
    {
        %client.DisplayInventory(true);  
    }
}

function Inventory_Pop(%player)
{
    if(!isObject(%player.inventoryStack))
    {
        %player.InventoryStack = List_NewList();
    }

    //remove player from the update list
    Inventory_GetTop(%player).subscribedPlayers.remove(%player);

    %stack = %player.inventoryStack;
    %stack.remove(%stack.getCount() - 1);

    %client = %player.client;
    if(isObject(%client))
    {
        %client.DisplayInventory(true);  
    }
}

function Inventory_GetTop(%player)
{
    if(!isObject(%player.inventoryStack))
    {
        %player.InventoryStack = List_NewList();
    }

    %stack = %player.inventoryStack;
    return %stack.getValue(%stack.getCount() - 1);
}

function Inventory_DisplayItem(%client,%item,%slot)
{
    %unselectable = %item.unselectable;
    
    if(%unselectable)
    {
        messageClient(%client, 'MsgItemPickup', "", %slot, %item.getName(), true);
    }
    else
    {   
        messageClient(%client, 'MsgItemPickup', "", %slot, %item.getId(), true);
    }
}

function Inventory_GetSelectedSpace(%player)
{
    %tool = %player.currTool;
    %space = %player.slotSpace[%tool];
    %item = %player.slotIndex[%tool];

    if(%item $= "")
    {
        %item = -1;
    }

    return %space SPC %item;
}

function Inventory::Add(%list,%Value,%row,%tag)
{
    %value.inventory = %list;
    return Parent::Add(%list,%Value,%row,%value.getName());
}

function Inventory::Display(%inventory,%silent)
{
    %players = %inventory.subscribedPlayers;
    %count = %players.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %player = %players.getObject(%i);
        %client = %player.client;
        if(isObject(%client) && Inventory_GetTop(%player) == %inventory)
        {
            %client.DisplayInventory(%silent);  
        }
    }
}

function Inventory::EquipTo(%inventory,%space,%item)
{
    %slot = %inventory.GetRow(%space);
    %success = false;
    if(%slot >= 0)
    {
        %space = %inventory.getValue(%slot);

        %success = %space.equip(%item);        
    }

    return %success;
}

function Inventory::UnequipFrom(%inventory,%space,%item)
{
    %slot = %inventory.GetRow(%space);
    %success = false;
    if(%slot >= 0)
    {
        %space = %inventory.getValue(%slot);

        %success = %space.unequip(%item);        
    }

    return %success;
}

function GameConnection::DisplayInventory(%client,%silent)
{
    %player = %client.player;
    if(isObject(%player))
    {
        %inventory = Inventory_GetTop(%player);
        %callback = %inventory.display;
        
        if(!isFunction(%callback))
        {
            %callback = "Inventory_DefaultDisplay";
        }

        messageClient(%client, 'MsgItemPickup', "", -1, 0, %silent);
        call(%callback,%client,%inventory,%silent);
    }
}

package Inventory
{
    function ServerCmdUseTool(%client, %slot)
    {
        %player = %client.player;
        if(isObject(%player))
        {
            %player.currTool = %slot;
            %word = Inventory_GetSelectedSpace(%player);
            %space = getWord(%word,0);
            %slot2 = getWord(%word,1);

            %callback = %space.select;

            %lastCallback = %player.inventorylastUseCallback;
            %lastSpace = getWord(%lastCallback,1);
            %lastSlot = getWord(%lastCallback,2);
            %lastCallback = getWord(%lastCallback,0);

            if(isFunction(%lastCallback) && %callback !$= %lastCallback)
            {
                call(%lastCallback,%client,%lastSpace,%lastSlot,0);
                %player.inventorylastUseCallback = ""; 
            }

            if(isFunction(%callback))
            {
                %player.inventorylastUseCallback = %callback SPC %space SPC %slot2;
                call(%callback,%client,%space,%slot2,1);
                return;
            }
        }
        Parent::ServerCmdUseTool(%client, %slot);
    }

    function ServerCmdUnUseTool(%client)
    {
        %player = %client.player;
        if(isObject(%player))
        {
            %word = Inventory_GetSelectedSpace(%player);
            %space = getWord(%word,0);
            %slot = getWord(%word,1);

            %player.currTool = -1;

            %callback = %space.select;
            if(isFunction(%callback))
            {
                call(%callback,%client,%space,%slot,0);
                return;
            }
        }

        Parent::ServerCmdUnUseTool(%client);
    }

    function ServerCmdDropTool(%client, %position)
    {
        %player = %client.player;
        if(isObject(%player) && Inventory_getTop(%player) !$= "")
        {
            %word = Inventory_GetSelectedSpace(%player);
            %space = getWord(%word,0);
            %slot = getWord(%word,1);

            //remove the dropping with closed inventory bug
            if(%slot == -1)
            {
                return;
            }

            %callback = %space.drop;
            if(isFunction(%callback))
            {
                call(%callback,%client,%space,%slot);
                return;
            }
        }

        Parent::ServerCmdDropTool(%client, %position);
    }

    function Armor::onTrigger(%this, %obj, %triggerNum, %val)
    {
        %client = %obj.client;
        if(isObject(%client))
        {
            if(%client.getClassName() $= "GameConnection")
            {
                %word = Inventory_GetSelectedSpace(%obj);
                %space = getWord(%word,0);
                %slot = getWord(%word,1);
                %callback = %space.use;

                switch(%triggerNum)
                {
                    case 0:
                        %type = 0;
                    case 4:
                        %type = 1;
                }
                if(isFunction(%callback) && %type !$= "")
                {
                    call(%callback,%client,%space,%slot,%type,%val);
                    return;
                }
            }
        }
        
        Parent::onTrigger(%this, %obj, %triggerNum, %val);
    }

};
activatePackage("Inventory");

//DISPLAY FUNCTIONS

function Inventory_DefaultDisplay(%client,%inventory,%silent)
{
    if(!isObject(%inventory))
    {
        return;
    }

    %player = %client.player;

    %slot = 0;
    %count = %inventory.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %space = %inventory.getValue(%i);
        %slotCount = %space.getCount();
        %emptyCount = getMin(%space.emptySlotMin,%space.CurrentCapacity());
        %emptyItem = %space.emptySlotItem;
        %j = 0;
        %safety = 0;
        while((%slotCount > 0 || %emptyCount > 0) && %safety < 99)
        {
            %safety++;
            %item = %space.getValue(%j);
            if(%item $= "" || !isObject(%item))
            {
                if(%emptyCount == 0)
                {
                    %j++;
                    %slotCount -= getMax(%item.size,1);
                    continue;
                }
                else
                {
                    %item = %emptyItem;
                    %emptyCount--;
                }
            }   
            Inventory_DisplayItem(%client,%item,%slot,%silent);

            //sets lookup values for later
            %player.slotSpace[%slot] = %space;
            %player.slotIndex[%slot] = %j;
            %slotCount -= getMax(%item.size,1);
            %slot++;
            %j++;
        }
        if(!(%safety < 99))
        {
            echo("safety broken");
            %space.dump();
        }
    }

    %maxTools = %client.player.getDatablock().maxTools;
    for(%i = %slot; %i < %maxTools; %i++)
    {
        messageClient(%client, 'MsgItemPickup', "", %i, 0, true);
        //sets lookup values for later
        %player.slotSpace[%i] = "";
        %player.slotIndex[%i] = "";
    }
}