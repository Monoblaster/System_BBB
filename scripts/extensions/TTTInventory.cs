if($TTTInventory::ShopItem $= "")
{
    schedule(10000,0,"TTTInventory_Create");
}

function TTTInventory_Create()
{
    $TTTInventory::ShopItem = InventorySlotItemData_Create("Shop");
    $TTTInventory::EmptyItem = InventorySlotItemData_Create("Empty","","",true);
    $TTTInventory::NextPage = InventorySlotItemData_Create("V");
    $TTTInventory::ExitShop = InventorySlotItemData_Create("Exit Shop");

    $TTTInventory::TraitorShopPage[0] = Inventory_Create("TTTTraitorShop0");
    
    %table = $BBB::Weapons_["Traitor"];
    %itemCount = getWordCount(%table);
    %itemsUsed = 0;
    %maxItemsPerPage = 6;
    %numberOfPages = mCeil(%itemCount / %maxItemsPerPage);
    %currPage = $TTTInventory::TraitorShopPage[0];

    for(%i = 0; %i < %numberOfPages; %i++)
    {
        //create the slots
        %currPage.add(InventorySpace_Create("ShopExit",1,$TTTInventory::ExitShop,1,"TTTInventory_ShopExitPrompt","","","TTTInventory_ShopExit"));
        %itemSlot = %currPage.add(InventorySpace_Create("ShopItems",%maxItemsPerPage,$TTTInventory::EmptyItem,%maxItemsPerPage,"TTTInventory_ShopItemPrompt","","","TTTInventory_ShopItemBuy"));
        %currPage.add(InventorySpace_Create("ShopNext",1,$TTTInventory::NextPage,1,"TTTInventory_ShopNextPrompt","","","TTTInventory_ShopNext"));

        //loop and add the items to the shop page
        %itemsOnThisPage = getMin(%itemCount - %itemsUsed,%maxItemsPerPage);
        for(%j = 0; %j < %itemsOnThisPage;%j++)
        {
            %currItem = getWord(%table,%itemsUsed + %j);
            %itemSlot.SetSlot(%j,%currItem,true);
        }
        %itemsUsed += %itemsOnThisPage;

        if(%itemsUsed != %itemCount)
        {
            //create the next page
            %currPage = $TTTInventory::TraitorShopPage[%i + 1] = Inventory_Create("TTTTraitorShop" @ %i + 1);
            $TTTInventory::TraitorShopPage[%i].nextPage = %currPage;
        }
        
    }
    %currPage.nextPage = $TTTInventory::TraitorShopPage[0];

    $TTTInventory::DetectiveShopPage[0] = Inventory_Create("TTTDetectiveShop0");
    
    %table = $BBB::Weapons_["Detective"];
    %itemCount = getWordCount(%table);
    %itemsUsed = 0;
    %maxItemsPerPage = 6;
    %numberOfPages = mCeil(%itemCount / %maxItemsPerPage);
    %currPage = $TTTInventory::DetectiveShopPage[0];

    for(%i = 0; %i < %numberOfPages; %i++)
    {
        //create the slots
        %currPage.add(InventorySpace_Create("ShopExit",1,$TTTInventory::ExitShop,1,"TTTInventory_ShopExitPrompt","","","TTTInventory_ShopExit"));
        %itemSlot = %currPage.add(InventorySpace_Create("ShopItems",%maxItemsPerPage,$TTTInventory::EmptyItem,%maxItemsPerPage,"TTTInventory_ShopItemPrompt","","","TTTInventory_ShopItemBuy"));
        %currPage.add(InventorySpace_Create("ShopNext",1,$TTTInventory::NextPage,1,"TTTInventory_ShopNextPrompt","","","TTTInventory_ShopNext"));

        //loop and add the items to the shop page
        %itemsOnThisPage = getMin(%itemCount - %itemsUsed,%maxItemsPerPage);
        for(%j = 0; %j < %itemsOnThisPage;%j++)
        {
            %currItem = getWord(%table,%itemsUsed + %j);
            %itemSlot.SetSlot(%j,%currItem,true);
        }
        %itemsUsed += %itemsOnThisPage;

        if(%itemsUsed != %itemCount)
        {
            //create the next page
            %currPage = $TTTInventory::DetectiveShopPage[%i + 1] = Inventory_Create("TTTDetectiveShop0" @ %i + 1);
            $TTTInventory::DetectiveShopPage[%i].nextPage = %currPage;
        }
        
    }
    %currPage.nextPage = $TTTInventory::DetectiveShopPage[0];
}

function empty()
{

}

function TTTInventory_OpenShopPrompt(%client,%space,%slot,%select)
{
    if(%select)
    {
        %client.centerPrint("\c5Drop this item to open credit shop.");
    }
    else
    {
        %client.centerPrint("");
    }
}

function TTTInventory_OpenShop(%client,%space,%slot)
{
    %player = %client.player;

    if(isObject(%player))
    {
        %role = %client.role;
        switch$(%role)
        {
        case "Traitor":
            inventory_push(%player,$TTTInventory::TraitorShopPage[0]);
        case "Detective":
            inventory_push(%player,$TTTInventory::DetectiveShopPage[0]);
        default:
            %client.centerPrint("\c5You do not have a shop");
        }
        
    }
}

function TTTInventory_ShopItemPrompt(%client,%space,%slot,%select)
{
    if(%select)
    {
        %item = %space.getValue(%slot);
        %name = %item.uiName;
        %price = %item.price;
        %stock = %item.stock;
        %currStock = %stock - %client.player.bought[%item.getId()];

        %prompt = "\c3" @ %price @ "c";
        if(%stock !$= "")
        {
            %prompt = %prompt @ "<br>\c4" @ %currStock SPC "left";
        }

        %prompt = %prompt @ "<br>\c5Drop this item to select.";
        if(%currStock <= 0 && %stock !$= "")
        {
            %prompt = "\c3Item out of stock";
        }
        
        %client.centerPrint("\c6" @ %name @ "<br>"@ %prompt);
    }
    else
    {
        %client.centerPrint("");
    }
}

function TTTInventory_ShopItemBuy(%client,%space,%slot)
{
    %player = %client.player;

    %item = %space.getValue(%slot);

    if(isObject(%player))
    {
        %success = BBB_CreditBuy(%client,%item);
        if(%success)
        {
            %client.chatMessage("\c6Item bought.");
        }
        else
        {
            %client.chatMessage("Purchase failed.");
        }
    }
}

function TTTInventory_ShopExitPrompt(%client,%space,%slot,%select)
{
    if(%select)
    {
        %name = "Exit Shop";
        %client.centerPrint("\c6" @ %name @ "<br>\c5Drop this item to select.");
    }
    else
    {
        %client.centerPrint("");
    }
}

function TTTInventory_ShopExit(%client,%space,%slot)
{
    %player = %client.player;
    if(isObject(%player))
    {
        inventory_pop(%player);
    }
}
function TTTInventory_ShopNextPrompt(%client,%space,%slot,%select)
{
    if(%select)
    {
        %name = "Next Page";
        %client.centerPrint("\c6" @ %name @ "<br>\c5Drop this item to select.");
    }
    else
    {
        %client.centerPrint("");
    }
}

function TTTInventory_ShopNext(%client,%space,%slot)
{
    %player = %client.player;

    if(isObject(%player))
    {
        %next = Inventory_GetTop(%player).nextPage;
        inventory_pop(%player);
        inventory_push(%player,%next);
    }
}

package TTTInventory
{
    function GameConnection::spawnPlayer(%this)
    {
        %r = Parent::spawnPlayer(%this);

        %player = %this.player;
        if(isObject(%player))
        {
            //clears the previous items
            %this.tttInventory.getValue(0).clear();

            Inventory_Push(%player,%this.tttInventory);
        }

        return %r;
    }

    function ShapeBase::WeaponAmmoUse(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::WeaponAmmoUse(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::aeAmmoCheck(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::aeAmmoCheck(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::AEReserveCheck(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEReserveCheck(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::AEMagReload(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEMagReload(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::AEUnloadMag(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEUnloadMag(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::AEUnloadShell(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEUnloadShell(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::AEShellReload(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEShellReload(%pl,%slot);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::baadDisplayAmmo(%obj, %this,%slot)
    {
        %pl = %obj;
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::baadDisplayAmmo(%obj, %this);
        %pl.currTool = %temp;
        return %r;
    }

    function ShapeBase::unBlockImageDismount(%pl,%slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::unBlockImageDismount(%pl);
        %pl.currTool = %temp;
        return %r;
    }

    function WeaponImage::AEOnFire(%this, %obj, %slot)
    {
        %pl = %obj;
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEOnFire(%this, %obj, %slot);
        %pl.currTool = %temp;
        return %r;
    }

    function WeaponImage::AEMountSetup(%this, %obj, %slot)
    {
        %pl = %obj;
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::AEMountSetup(%this, %obj, %slot);
        %pl.currTool = %temp;
        return %r;
    }

    function serverCmdDropAmmo(%cl, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::serverCmdDropAmmo(%cl, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7);
        %pl.currTool = %temp;
        return %r;
    }

    function Player::WeaponAmmoStart(%pl)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::WeaponAmmoStart(%pl);
        %pl.currTool = %temp;
        return %r;
    }

    function Player::WeaponAmmoUse(%pl)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::WeaponAmmoUse(%pl);
        %pl.currTool = %temp;
        return %r;
    }

    function Player::WeaponAmmoGive(%pl, %amt, %slot)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::WeaponAmmoGive(%pl, %amt, %slot);
        %pl.currTool = %temp;
        return %r;
    }

    function Player::WeaponAmmoCheck(%pl)
    {
        %temp = %pl.currTool;
        %pl.currTool = %pl.realCurrTool;
        %r = Parent::WeaponAmmoCheck(%pl);
        %pl.currTool = %temp;
        return %r;
    }

    function ItemData::onUse (%this, %player, %invPosition)
    {
        %player.realCurrTool = %invPosition;
        parent::onUse (%this, %player, %invPosition);
    }

    function Weapon::onUse (%this, %player, %invPosition)
    {
        %player.realCurrTool = %invPosition;
        parent::onUse (%this, %player, %invPosition);
    }

    function ServerCmdUnUseTool(%client)
    {
        %player = %client.player;
        if(isObject(%player))
        {
            %player.realCurrTool = -1;
        }
        Parent::ServerCmdUnUseTool(%client);
    }

    function ItemData::onPickup (%this, %obj, %user, %amount)
    {
        %r = Parent::onPickup (%this, %obj, %user, %amount);
        %client = %user.client;
        if(%r)
        {
            if(isObject(%client))
            {
                //update the inventory display
                %count = %user.getDatablock().maxTools;
                %space = %client.tttInventory.getvalue(0);
                for(%i = 0; %i < %count; %i++)
                {
                    %tool = %user.tool[%i];
                    %space.setSlot(%i,%tool,true);
                }
            }

            //redisplay so no overlap
            %inventory = Inventory_GetTop(%user);
            if(isObject(%inventory))
            {
                
                %inventory.display(true);
            }
        }
        
        return %r;
    }

    function ServerCmdDropTool(%client, %position)
    {
        %obj = %client.player;
        %wasValid = isObject(%obj.tool[%position]);

        %r = parent::ServerCmdDropTool(%client, %position);

        if(isObject(%obj) && %wasValid)
        {
            //update the inventory display
            %count = %obj.getDatablock().maxTools;
            %space = %client.tttInventory.getvalue(0);
            for(%i = 0; %i < %count; %i++)
            {
                %tool = %obj.tool[%i];
                %space.setSlot(%i,%tool,true);
            }
        }

        return %r;
    }

    function GameConnection::onClientEnterGame(%client)
	{	
        %client.tttInventory = Inventory_Create("TTTInventory");
        %client.tttInventory.add(InventorySpace_Create("Space",7,$TTTInventory::EmptyItem,7));
        %client.tttInventory.add(InventorySpace_Create("Shop",1,$TTTInventory::ShopItem,1,"TTTInventory_OpenShopPrompt","","","TTTInventory_OpenShop"));
		return Parent::onClientEnterGame(%client);
	}
};
activatePackage(TTTInventory);

package WeaponDroping
{
	function servercmdDropTool(%client,%slot)
	{
    %player = %client.player;
    if(isObject(%player) && isObject(%image = %player.getMountedImage(0)) && %image.item.aebase && isObject(%player.tool[%slot]))
    {
      %player.unmountImage(0);
    }
    return Parent::servercmdDropTool(%client,%slot);
	}
};


//corpse inventory
function Player::GetCorpseInventory(%player)
{
    %inv = Inventory_Create("CorpseInventory" @ %player);
    %space = %inv.add(InventorySpace_Create("Space",7,$TTTInventory::EmptyItem,7,"corpseInventory_Prompt","","","corpseInventory_Drop"));
    
    //add the items to the inventory
    for(%i = 0; %i < 7; %i++)
    {
        %tool = %player.tool[%i];
    
        %space.setSlot(%i,%tool,true);
    }

    return %inv;
}

function corpseInventory_Drop(%client,%space,%slot)
{
    %obj = %client.player;
    if(!isObject(%obj))
    {
        return;
    }

    //god i hat how this gamemode's code is structured
    %currItem = %space.getValue(%slot);
    if(!isObject(%currItem))
    {
        return;
    }

    %itemname = %currItem.getName();

    for(%a = 0; %a <= 3; %a++)
	{
		switch(%a)
		{
			case 0:
				%name = "Primary";
			case 1:
				%name = "Secondary";
			case 2:
				%name = "Other";
			case 3:
				%name = "Grenade";
		}

		%fieldCount = getFieldCount($BBB::Weapons_[%name]);
		for(%b = 0; %b < %fieldCount; %b++)
		{
			%field = getField($BBB::Weapons_[%name], %b);
			if(%field $= %itemName)
			{
				%found = true;
				%foundSlot = %a;
				break;
			}

		}
		if(%found)
			break;
	}

	if(%foundSlot $= "")
	{
		%name = "";
		//look for a not used slot
		for(%i = 4; %i < 7; %i++)
		{
			if(!isObject(%obj.tool[%i]))
			{
				%foundSlot = %i;
				break;
			}
		}
	}

    if(%foundSlot $= "")
	{
        %foundSlot = 6;
    }

    //swap the found slot on the player and the curr slot on the corpse
    %playerItem = %obj.tool[%foundSlot];

    %space.setSlot(%slot,%playerItem);
    %client.tttInventory.getValue(0).setSlot(%foundSlot,%currItem);
    %obj.tool[%foundSlot] = %currItem.getId();
}

function corpseInventory_Prompt(%client,%space,%slot,%select)
{
    if(%select)
    {
        %name = %space.getValue(%slot).uiName;
        %client.centerPrint("\c6" @ %name @ "<br>\c5Drop this item take it.");
    }
    else
    {
        %client.centerPrint("");
    }
}