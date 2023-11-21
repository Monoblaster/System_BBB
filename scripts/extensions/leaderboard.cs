function registerLeaderboard(%name,%ascending)
{
    if(!isObject(LeaderboardGroup))
    {
        new ScriptGroup(LeaderboardGroup);
    }

    if(LeaderboardGroup.num[%name] $= "")
    {
        LeaderboardGroup.num[%name] = LeaderboardGroup.getCount();
        LeaderboardGroup.add(new ScriptObject("List")
        {
            name = %name;
            ascending = %ascending;
        });
    }
}

function LeaderboardGroup::Update(%group)
{
    //disabled due to lag (for now)
    return "";
    //loop through our group and update each of the list then sort it
    %count = %group.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %board = %group.getObject(%i);
        %statName = %board.name;
        //loop all the loaded clients and grab the value
        %statGroup = $Stats::StatSet;
        %clientCount = %statGroup.getCount();
        for(%j = 0; %j < %clientCount; %j++)
        {
            %client = %statGroup.getObject(%j);
            %blid = %client.bl_id;
            %stat = %client.getStat(%statName);

            //check to see if this client has been added to the list already
            %row = %board.getRow(%blid);
            if(%row !$= "")
            {
                //the client is in this list set their value to their current value
                %board.set(%row,%stat); 
            }
            else
            {
                //the client is not in the list add their value in
                %board.add(%stat,"",%blid);
            }
        }

        //all the values have been updated sort the list
        %board.sort(%board.ascending);
    }
}

$Leaderboard::MaxLines = 10;
function serverCmdLeaderBoard(%client,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15)
{
    if(!isObject(%client))
    {
        return;
    }

    if(%a0 $= "")
	{
		%client.chatMessage("\c5Welcome to leaderboards. \c3[stat name or \"stats\"] \c4[starting position]");
		return;
	}
    else if(%a0 $= "STATS")
    {
        %group = LeaderboardGroup;
        %count = %group.getcount();
        for(%i = 0; %i < %count; %i++)
		{
            %leaderboard = %group.getObject(%i);
            %name = %leaderboard.name;
			%client.chatMessage("<font:consolas:20>\c6" @ (strLen(%i + 1) <= 1 ? " " @ (%i + 1) : %i + 1)  @ " | " @ %name);
		}
		return;
    }
    else
    {
        %c = 0;
        %stat = %a0;
        while((%leaderboardNum = LeaderboardGroup.num[%stat]) $= "" && %c < 16)
        {
            %c++;
            %stat = %stat SPC %a[%c];
        }

        if(%leaderboardNum  $= "")
        {
            %client.chatMessage("\c5Uknown Stat.");
            return;
        }

        %start = %a[%c++];

        %end = %start + $Leaderboard::MaxLines;
        //grab this stat's leaderboard and display n spaces after %start
        %leaderboard = LeaderboardGroup.getObject(%leaderboardNum);
        %client.chatMessage("\c5" @ %leaderboard.name SPC "Leaderboard:");
        for(%i = %start; %i < %end; %i++)
        {
            %name = $Stats::BLIDToName[%leaderboard.getTag(%i)];
            %value = %leaderBoard.getValue(%i);
            %client.chatMessage("\c6" @ %i + 1 @ "." SPC %name SPC %value);
        }
    }
}

//guh imagine wiriting a leaderboard script in only 109 lines
registerLeaderboard("T Killed",true);
registerLeaderboard("I Killed",true);
registerLeaderboard("D Killed",true);
registerLeaderboard("Kills",true);
registerLeaderboard("Miskills");
registerLeaderboard("PR Kills",true);
registerLeaderboard("Rounds",true);
registerLeaderboard("Wins",true);
registerLeaderboard("T Wins",true);
registerLeaderboard("I Wins",true);
registerLeaderboard("D Wins",true);
registerLeaderboard("Survived",true);
registerLeaderboard("Melee Kills",true);
registerLeaderboard("Throwing Knife Kills",true);