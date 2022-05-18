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
            %client = %statGroup.getObject(%i);
            %blid = %client.bl_id;
            %stat = %client.getStat(%statName);

            //check to see if this client has been added to the list already
            %row = %board.getRow(%blid);
            if(%row >= 0)
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
        %board.sort(!%board.ascending);
    }
}

$Leaderboard::MaxLines = 10;
function serverCmdLeaderBoard(%client,%stat,%start)
{
    if(!isObject(%cleint))
    {
        return;
    }

    %end = %start + $Leaderboard::MaxLines;
    //grab this stat's leaderboard and display n spaces after %start
    %leaderboard = LeaderboardGroup.getObject(LeaderboardGroup.num[%stat]);
    %client.chatMessage("\c4" @ %leaderboard.name SPC "LeaderBoard:");
    for(%i = %start; %i < %end; %i++)
    {
        %name = $Stat::BLIDToName[%leaderboard.getTag(%i)];
        %value = %leaderBoard.getValue(%i);
        %client.chatMessage("\c6" @ %i + 1 @ "." SPC %name SPC %value);
    }
}

//guh imagine wiriting a leaderboard script in only 73 lines