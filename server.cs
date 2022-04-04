// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot | Edited by Zinlock
// =================================================
// File Name: server.cs
// Description: What? This has to start somewhere.
// =================================================
// Table of Contents:
//
// 1. Preferences
// 2. Scripts
// 3. Minigame
// =================================================

// =================================================
// 1. Preferences
// =================================================
// Enable
$BBB::Enable = true;

// Maps
$BBB::Start::Map		= "Add-Ons/BBB_Roy_The_Shipper/save.bls";
$BBB::Map::Rounds		= 4;

// Time
$BBB::Time::Base		= "300000"; // In MS
$BBB::Time::PreRound 	= "30000";
$BBB::Time::PostRound	= "12000";
$BBB::Time::Shock		= "1500"; // The time between the round ends and the winner is shown. Should be lower than Time::PostRound.

$BBB::Time::MapVote		= "30000"; // Map vote time
$BBB::Time::Bonus		= "30000"; // Bonus time for every time an innocent is killed.

// Ratios
$BBB::Detective::MinPlayers = 8; // 1 Detective for X players. ex. if there are 16 players, there are 2 detectives.
$BBB::Traitor::MinPlayers = 4; // 1 traitor per x players

// Weapons
$BBB::Weapons_Primary = "GunItem"; //primary

$BBB::Weapons_Secondary = "R_MW92fsItem" TAB "R_92fsItem" TAB "R_SubmachineGunOPFORItem" TAB "R_PistolMagnumItem" TAB "R_PistolHeavyItem" TAB "R_PistolItem" TAB "R_PistolSopmodItem"; //secondary

$BBB::Weapons_Grenade = "grenade_smokeItem" TAB "grenade_concussionItem" TAB "grenade_electroItem" TAB "grenade_flashbangItem" TAB "grenade_nailbombItem" TAB "grenade_fragmentItem" TAB "grenade_stickItem" TAB "grenade_riotItem" TAB "grenade_decoyItem";//"SmokeGrenadeItem"; //tiertary

$BBB::Weapons_Other = "meatcleaverItem" TAB "pipeWrenchItem" TAB "fryingpanItem" TAB "baseballBatItem" TAB "macheteItem" TAB "sledgeHammerItem" TAB "hockeystickItem" TAB "shovelItem" TAB "pitchforkItem" TAB "spikeBatItem" TAB "crowbarItem" TAB "axeItem" TAB "tireironItem" TAB "leadpipeItem" TAB "R_AmmoResupplyItem" TAB "R_AmmoPackItem" TAB "R_AmmoResupplyItem" TAB "R_AmmoPackItem" TAB "R_AmmoPackItem";

// Shop Weapons
//healthStationHandItem
$BBB::Weapons_Detective = "XrayItem" TAB "medi_stimpackItem" TAB "R_ShieldPistolItem" TAB "DNAScannerItem"; 
$BBB::Weapons_Traitor = "XrayItem" TAB "DisguiserItem" TAB "grenade_mollyItem" TAB "HL1TripmineItem" TAB "ThrowingKnifeItem" TAB "silencedGunItem" TAB "FlareItem" TAB "grenade_remoteItem" TAB "grenade_clusterItem" TAB "grenade_dynamiteItem";

$BBB::FirstShopSlot = 5;

// Other
$BBB::Announce::BodyFound = true; // Announce to the server when a body is found.
$BBB::UsingSMMBodies = false;
$BBB::RoundMusic = ""; // HL1_-_Military_Precision";

$Game::Item::PopTime = $BBB::Time::Base + $BBB::Time::PreRound + $BBB::Time::PostRound;
// =================================================
// 2. Scripts
// =================================================
// Script Paths
$BBB::Path = "Add-Ons/System_BBB/";
$BBB::Path::Saving	= "config/server/BBB/";
// Script Execution
$pattern = $BBB::Path @ "scripts/*.cs";
for ( $file = findFirstFile( $pattern ) ; $file !$= "" ; $file = findNextFile( $pattern ) )
	exec( $file );

$pattern = $BBB::Path @ "weapons/*.cs";
for ( $file = findFirstFile( $pattern ) ; $file !$= "" ; $file = findNextFile( $pattern ) )
	exec( $file );
$pattern = "";

function BBB_RebuildItemTable()
{
	deleteVariables("$BBB::Weapons_*");

	%pattern = $BBB::Path @ "weapons_*.txt";
	for ( %file = findFirstFile( %pattern ) ; %file !$= "" ; %file = findNextFile( %pattern ) )
	{
		%fileName = fileBase(%file);

		%table = getSubStr(%fileName, 8, strLen(%fileName) - 7);
		if(%table !$= "")
		{
			%fileObj = new FileObject();

			if(%fileObj.openForRead(%file))
			{
				while(!%fileObj.isEOF())
				{
					%str = %fileObj.readLine();
					
					if(trim(%str) $= "")
						continue;
					
					if(isObject(%item = $uiNameTable_items[%str]))
					{
						if($BBB::Weapons_[%table] $= "")
							$BBB::Weapons_[%table] = %item.getName();
						else
							$BBB::Weapons_[%table] = $BBB::Weapons_[%table] TAB %item.getName();
					}
					else
						warn("BBB_RebuildItemTable() - Couldn't find weapon \"" @ %str @ "\" for table \"" @ %table @ "\"");
				}
			}
		}
	}
}

$pattern = "";
$file = "";

registerBBBPrefs();

// Events
registerInputEvent("fxDTSBrick", "onCorpseTouch", "Self fxDTSBrick" TAB "Corpse Player" TAB "MiniGame MiniGame");
registerInputEvent("fxDTSBrick", "onInnocentActivate", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
registerInputEvent("fxDTSBrick", "onInnocentTouch", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
registerInputEvent("fxDTSBrick", "onTraitorActivate", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
registerInputEvent("fxDTSBrick", "onTraitorTouch", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
registerOutputEvent("Player", "DeleteCorpse", "", false);

// =================================================
// 3. Minigame
// =================================================
function BBB() // -- The only non-packaged function not in functions.cs! Wow! (Its here for easy editing) [This is a lie, not the only one.]
{
	if(!isObject(BBB_Minigame) && $BBB::Enable)
	{
		new ScriptObject(BBB_Minigame)
		{
			class = "MiniGameSO";
			owner = -1;
			numMembers = 0;

			title = "Betrayal in Block Boulevard";
			colorIdx = "3";
			inviteOnly = false;
			UseAllPlayersBricks = true;
			PlayersUseOwnBricks = false;

			Points_BreakBrick = 0;
			Points_PlantBrick = 0;
			Points_KillPlayer = 0;
			Points_KillSelf = 0;
			Points_Die = 0;

			respawnTime = "-1";
			vehiclerespawntime = "10000";
			brickRespawnTime = "30000";
			playerDatablock = "BBB_Standard_Armor";

			useSpawnBricks = true;
			fallingdamage = true;
			weapondamage = true;
			SelfDamage = true;
			VehicleDamage = true;
			brickDamage = false;

			enableWand = false;
			EnableBuilding = false;
			enablePainting = false;

			StartEquip0 = 0;
			StartEquip1 = 0;
			StartEquip2 = 0;
			StartEquip3 = 0;
			StartEquip4 = 0;

			isBBB = true;
		};

		BBB_RebuildItemTable();
		BBB_BuildMapList();
		BBB_BuildShopList();

		BBB_LoadMap($BBB::Start::Map);

		for(%a = 0; %a < ClientGroup.getCount(); %a++)
			BBB_Minigame.addMember(ClientGroup.getObject(%a));
	}
}

schedule(10, 0, "BBB");