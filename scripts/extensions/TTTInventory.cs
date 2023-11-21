if($TTTInventory::Shop $= "")
{
    schedule(1000,0,"TTTInventoryV2_Init");
}

function TTTInventory_Prompt(%client,%slot,%select)
{
    if(%client.TTTInventory_Shop)
    {
        if(%slot == 7)
        {
            TTTInventory_ShopNextPrompt(%client,%slot,%select);
            return;
        }
        TTTInventory_ShopItemPrompt(%client,%slot,%select);
    }
    else if(%client.TTTInventory_Corpse)
    {
        corpseInventory_Prompt(%client,%slot,%select);
    }
    else
    {
        if(%slot == 7)
        {
            TTTInventory_OpenShopPrompt(%client,%slot,%select);
        }
    }
}

function TTTInventory_Use(%client,%slot)
{
    if(%client.TTTInventory_Shop)
    {
        if(%slot == 7)
        {
            TTTInventory_ShopNext(%client,%slot);
            return;
        }
        TTTInventory_ShopItemBuy(%client,%slot);
    }
    else if(%client.TTTInventory_Corpse)
    {
        corpseInventory_Drop(%client,%slot);
    }
    else
    {
        if(%slot == 7)
        {
            TTTInventory_OpenShop(%client,%slot);
        }
    }
}


function TTTInventory_OpenShopPrompt(%client,%slot,%select)
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

function TTTInventory_OpenShop(%client,%slot)
{
    %player = %client.player;

    if(isObject(%player))
    {
        %role = %client.role;
        switch$(%role)
        {
        case "Traitor":
            %client.TTTInventory_Shop = $TTTInventory::TraitorShopMain;
            $TTTInventory::TraitorShopMain.display(%client,true);
        case "Detective":
            %client.TTTInventory_Shop = $TTTInventory::DetectiveShopMain;
            $TTTInventory::DetectiveShopMain.display(%client,true);
        default:
            %client.centerPrint("\c5You do not have a shop");
        }
        
    }
}

function TTTInventory_ShopItemPrompt(%client,%slot,%select)
{
    if(%select)
    {
        %item = %client.TTTInventory_Shop.get(%slot);
        %name = %item.uiName;
        %price = %client.TTTInventory_Shop.price[%slot];
        %stock = %client.TTTInventory_Shop.stock[%slot];
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

function TTTInventory_ShopItemBuy(%client,%slot)
{
    %player = %client.player;

    %item = %client.TTTInventory_Shop.get(%slot);

    if(isObject(%player))
    {
        %success = BBB_CreditBuy(%client,%item,%client.TTTInventory_Shop.price[%slot],%client.TTTInventory_Shop.stock[%slot]);
        if(%success)
        {
            %client.chatMessage("\c6Item bought.");
        }
        else
        {
            %client.chatMessage("Purchase failed.");
        }
    }

    %client.TTTInventory_Shop.Display(%client,true);
}

function TTTInventory_ShopExit(%client,%slot)
{
    %player = %client.player;
    if(isObject(%player))
    {
        %client.TTTInventory_Shop = "";
        Inventory::Display(%player,%client,true);
        $TTTInventory::Shop.display(%client);
    }
}
function TTTInventory_ShopNextPrompt(%client,%slot,%select)
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

function TTTInventory_ShopNext(%client,%slot)
{
    %player = %client.player;

    if(isObject(%player))
    {
        %client.TTTInventory_Shop = %client.TTTInventory_Shop.nextPage;
        %client.TTTInventory_Shop.display(%client,true);
    }
}

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
    %inv = Inventory_Create();
    %inv.Corpse = true;
    
    //add the items to the inventory
    for(%i = 0; %i < 7; %i++)
    {
        %inv.set(%i,%player.tool[%i]);
    }

    return %inv;
}

function corpseInventory_Drop(%client,%slot)
{
    %obj = %client.player;
    if(!isObject(%obj))
    {
        return;
    }

    //god i hat how this gamemode's code is structured
    %currItem = %client.TTTInventory_Corpse.get(%slot);
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
    echo(%playerItem SPC %slot SPC %currItem);
    %client.TTTInventory_Corpse.set(%slot,%playerItem);
    %obj.tool[%foundSlot] = %currItem.getId();
    %client.TTTInventory_Corpse.Display(%client,true);
}

function corpseInventory_Prompt(%client,%slot,%select)
{
    if(%select)
    {
        %name = %client.TTTInventory_Corpse.get(%slot).uiName;
        %client.centerPrint("\c6" @ %name @ "<br>\c5Drop this item take it.");
    }
    else
    {
        %client.centerPrint("");
    }
}

function corpseInventory_Display(%client,%inv)
{
    %client.TTTInventory_Corpse = %Inv;
    %inv.display(%client,true);
}

function corpseInventory_UnDisplay(%client)
{
    Inventory::display(%client.player,%client,true);
    $TTTInventory::Shop.display(%client);
    %client.TTTInventory_Corpse = "";
}

datablock ItemData(TTTShop_Shop)
{
    category = "Tools";
    uiName = "Shop";
    iconName = "";
    doColorShift = true;
    colorShiftColor = "0 1 0 1";
};

datablock ItemData(TTTShop_Next)
{
    category = "Tools";
    uiName = "V";
    iconName = "";
    doColorShift = true;
    colorShiftColor = "1 1 1 1";
};

function TTTInventoryV2_Init()
{
    $TTTInventory::TraitorShopMain = TTTInventoryV2_makeShop("Traitor");
    $TTTInventory::DetectiveShopMain = TTTInventoryV2_makeShop("Detective");
    $TTTInventory::Shop = Inventory_create().set(7,TTTShop_Shop);
}

function TTTInventoryV2_makeShop(%tablename)
{
    %main = Inventory_Create();
    
    %table = $BBB::Weapons_[%tablename];
    %itemCount = getWordCount(%table);
    %itemsUsed = 0;
    %maxItemsPerPage = 7;
    %numberOfPages = mCeil(%itemCount / %maxItemsPerPage);
    %currPage = %main;
    for(%i = 0; %i < %numberOfPages; %i++)
    {
        %currPage.set(7,TTTShop_Next);

        //loop and add the items to the shop page
        %itemsOnThisPage = getMin(%itemCount - %itemsUsed,%maxItemsPerPage);
        for(%j = 0; %j < %itemsOnThisPage;%j++)
        {
            %currItem = getWord(%table,%itemsUsed + %j);
            %currPage.set(%j,%currItem);
            %currPage.price[%j] = $BBB::WeaponPrice[%tablename,%currItem.getName()];
            %currPage.stock[%j] = $BBB::WeaponStock[%tablename,%currItem.getName()];
        }
        %itemsUsed += %itemsOnThisPage;

        if(%itemsUsed != %itemCount)
        {
            //create the next page
            %nextPage = Inventory_Create();
            %currPage.nextPage = %nextPage;
            %currPage = %nextPage;
        }
        
    }
    %currPage.nextPage = %main;
    return %main;
}

package TTTInventory
{
    function GameConnection::spawnPlayer(%this)
    {
        %r = Parent::spawnPlayer(%this);

        corpseInventory_UnDisplay(%this);
        Inventory::Display(%this.player,%this,true);
        $TTTInventory::Shop.display(%this);

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
        TTTInventory_Prompt(%client,%player.currTool,false);
        
        if(isObject(%player))
        {
            %player.realCurrTool = -1;
        }
        if(%client.TTTInventory_Shop $= "")
        {
            %r = Parent::ServerCmdUnUseTool(%client);
        }
        if(%client.TTTInventory_Shop !$= "")
        {
            TTTInventory_ShopExit(%client,%player.currTool);
        }
        return %r;
    }

    function ServerCmdUseTool(%client,%slot)
    {
        TTTInventory_Prompt(%client,%client.player.currTool,false);
        %client.player.currTool = %slot;
        TTTInventory_Prompt(%client,%slot,true);
        if(%client.TTTInventory_Shop $= "" && %client.TTTInventory_Corpse $= "")
        {
            return Parent::ServerCmdUseTool(%client,%slot);
        }
    }

    function ServerCmdDropTool(%client,%position)
    {
        TTTInventory_Use(%client,%position);
        if(%client.TTTInventory_Shop $= "" && %client.TTTInventory_Corpse $= "")
        {
            return parent::ServerCmdDropTool(%client,%position);
        }
    }
};
deactivatePackage("TTTInventory");
activatePackage("TTTInventory");