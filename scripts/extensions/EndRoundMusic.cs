// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot
// =================================================
// File Name: EndRoundMusic.cs
// Description: End music extension.
// =================================================
// Table of Contents:
//
// 1. Preferences
// 2. Functions
// 3. Package
// =================================================

// =================================================
// 1. Preferences
// =================================================
// Other

datablock AudioDescription(mdMusic : audioClose3d)
{
	volume = 0.85;
};

datablock AudioProfile(Traitor_Win)
{
	fileName = $BBB::Path @ "sounds/traitorWin.ogg";
	preload = true;
	description = mdMusic;
};

datablock AudioProfile(Innocent_Win : Traitor_Win)
{
	fileName = $BBB::Path @ "sound/innoWin.ogg";
};

// =================================================
// 2. Functions
// =================================================
function BBB_GetMusicCount(%role)
{
	%counter = 0;
	while($BBB::Music_[%role, %counter] !$= "")
		%counter++;
	return %counter;
}

function BBB_GeneratePlaylist(%role, %playSong)
{
	%count = BBB_GetMusicCount(%role);

	for(%a = 0; %a < %count; %a++)
	{
		$BBB::Music::Playlist_[%role, %a] = $BBB::Music_[%role, %a];
	}

	%currIndex = %count;
	// While there remain elements to shuffle...
	while (0 != %currIndex)
	{
		// Pick a remaining element...
		%randIndex = mFloor(getRandom() * %currIndex);
		%currIndex -= 1;

		// And swap it with the current element.
		%temp = $BBB::Music::Playlist_[%role, %currIndex];

		$BBB::Music::Playlist_[%role, %currIndex] = $BBB::Music::Playlist_[%role, %randIndex];
		$BBB::Music::Playlist_[%role, %randIndex] = %temp;
	}

	$BBB::Music::Playlist::Queue_[%role] = 0;
	// =============================================
	// Source: http://stackoverflow.com/questions/2450954/how-to-randomize-shuffle-a-javascript-array

	if(%playSong)
		BBB_PlayGlobalMusic(%role);
}

function BBB_GetPlaylistMusic(%role)
{
	// If we're over the limit, regenerate!
	%max = BBB_GetMusicCount(%role);
	if($BBB::Music::Playlist::Queue_[%role] >= %max || $BBB::Music::Playlist::Queue_[%role] $= "")
	{
		BBB_GeneratePlaylist(%role, true);
		return -1;
	}

	%queue = $BBB::Music::Playlist::Queue_[%role];
	%song = $BBB::Music::Playlist_[%role, %queue];

	$BBB::Music::Playlist::Queue_[%role]++;

	return %song;
}

function BBB_GetRandomMusic(%role)
{
	%count = BBB_GetMusicCount(%role);

	%randNum = getRandom(0, %count - 1);
	return $BBB::Music_[%role, %randNum];
}

function BBB_PlayGlobalMusic(%role)
{
	%mg = BBB_Minigame;
	BBB_StopGlobalMusic();
	for(%i = 0; %i < ClientGroup.GetCount(); %i++)
	{
		%cl = ClientGroup.GetObject(%i);

		%cl.Play2D(%role);
	}
}

// function BBB_PlayRoundMusic()
// {
// 	%mg = BBB_Minigame;
// 	for(%i = 0; %i < %mg.numMembers; %i++)
// 	{
// 		%client = %mg.member[%i];
// 		%client.setMusic($BBB::);
// 	}
// }

function BBB_StopGlobalMusic()
{
	%mg = BBB_Minigame;
	for(%i = 0; %i < %mg.numMembers; %i++)
	{
		%client = %mg.member[%i];
		%client.setMusic(0, 0);
	}
}

// GameConnection
// Code from Port - credit: https://forum.blockland.us/index.php?topic=296249.0
function GameConnection::setMusic(%this, %profile, %volume)
{
    if (%volume !$= "")
        %volume = mClampF(%volume, 0, 5);
    else
        %volume = 0.9;

    if (isObject(%this.musicEmitter))
        %this.musicEmitter.delete();

    if (isObject(%profile))
    {
        if (%profile.getClassName() !$= "AudioProfile")
            return;

        %this.musicEmitter = new AudioEmitter()
        {
            position = "9e9 9e9 9e9";
            profile = %profile;
            volume = %volume;
            useProfileDescription = false;
            is3D = false;
        };

        MissionCleanup.add(%this.musicEmitter);

        if (%this.hasSpawnedOnce)
            %this.musicEmitter.scopeToClient(%this);
    }
}

// =================================================
// 3. Package
// =================================================
package BBB_EndRoundMusic
{
	function BBB_Minigame::roundEnd(%so, %type)
	{
		parent::roundEnd(%so, %type);
		%time = $BBB::Time::Shock + 300;
		%count = getWordCount(%type);
		for(%i = 0; %i < %count; %i++)
		{
			schedule(%time, %so, "BBB_PlayGlobalMusic", getWord(%type,%i).winSound);
			return;
		}
		
	}

	function BBB_Minigame::roundSetup(%so, %type)
	{
		parent::roundSetup(%so, %type);
		BBB_StopGlobalMusic();
	}
};
activatePackage(BBB_EndRoundMusic);
