//custom filter script based on the functionality of vFilter
//admins are able to edit the filter, listen to unfiltered messages, and list current filters
//admins may also add antifilter words. words that prevent filtering if they are included with whitespace 
//the filter should be able to ignore white space and special characters when filtering string
//if a replacement is supplied then the filter will replace the word with the replacement. otherwise block the message
    //when searching for a word whitespace and speical characters are removed
    //if the word is present check if a anti filter isn't included in its construction
    //if it passes this test continue with filtering
$DataInstance::chatFilter = 6;
ServerGroup.schedule(100,"DataInstance_ListLoad");

function ChatFilter_FilterFeedback(%list,%index)
{
    if(%list $= "AntiFilter")
    {
        %name = "\c5!";
    }
    else
    {
        %name = "\c3";
    }

    %data = ServerGroup.DataInstance($DataInstance::chatFilter);
    %s = %name @ %index + 1 @"\c7|\c6"@ IndexList_Get(%data,%list,%index);
    if(%data.filterReplace[%index] !$= "")
    {
        return %s SPC "\c0->\c6" SPC %data.filterReplace[%index];
    }
    return %s;
}

//filter functions
function serverCmdChatFilter(%client,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n)
{
    if(!%client.isAdmin)
    {
        return;
    }

    %string = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %g SPC %h SPC %i SPC %j SPC %k SPC %l SPC %m SPC %n);
    if(strLen(%string) == 0)
    {
        %client.chatMessage("Command Help");
        return;
    }

    %data = ServerGroup.DataInstance($DataInstance::chatFilter);
    %command = firstWord(%string);
    %arguments = restWords(%string);
    switch$(%command)
    {
    case "Add":
        if(%arguments $= "")
        {
            %client.chatMessage("Add Help");
            return;
        }

        if(getSubStr(%arguments,0,1) $= "!")
        {
            %value = getSubStr(%arguments,1,999999);
            %list = "AntiFilter";
        }
        else
        {
            %value = %arguments;
            %list = "Filter";
        }

        %found = IndexList_Find(%data,%list,"%a $=" @ %value);
        if(%found >= 0)
        {
            %client.chatMessage("\c5Already exists "@ChatFilter_FilterFeedback(%list,%found));
            return;
        }

        IndexList_Insert(%data,%list,%value);
        %client.chatMessage("\c3Added "@ChatFilter_FilterFeedback(%list,IndexList_Count(%data,%list) - 1));
        ServerGroup.DataInstance_ListSave();
    case "Replace":
        if(%arguments $= "")
        {
            %client.chatMessage("Replace Help");
            return;
        }

        %index = firstWord(%arguments) - 1;
        %replace = restWords(%arguments);
        if(IndexList_Get(%data,"Filter",%index) $= "")
        {
            %client.chatMessage("\c5"@ %index + 1 SPC "does not exist");
            return;
        }

        if(%replace $= "")
        {
            %client.chatMessage("\c3Removed replacement" SPC ChatFilter_FilterFeedback("Filter",%index));
        }
        %data.filterReplace[%index] = %replace;
        if(%replace !$= "")
        {
            %client.chatMessage("\c3Set replacement" SPC ChatFilter_FilterFeedback("Filter",%index));
        }

        ServerGroup.DataInstance_ListSave();
    case "Remove":
        if(%arguments $= "")
        {
            %client.chatMessage("Remove Help");
            return;
        }

        if(getSubStr(%arguments,0,1) $= "!")
        {
            %index = getSubStr(%arguments,1,999999) - 1;
            %list = "AntiFilter";
            %name = "!";
            
        }
        else
        {
            %index = %arguments - 1;
            %list = "Filter";
            %name = "";
        }

        if(IndexList_Get(%data,%list,%index) $= "")
        {
            %client.chatMessage("\c5" @ %name @ %index + 1 SPC "does not exist");
            return;
        }

        %client.chatMessage("Removed" @ ChatFilter_FilterFeedback(%list,%index));
        IndexList_Remove(%data,%list,%index);
        %data.filterReplace[%index] = "";
        ServerGroup.DataInstance_ListSave();
    case "List":
        if(getSubStr(%arguments,0,1) $= "!")
        {
            %value = getSubStr(%arguments,1,999999);
            %list = "AntiFilter";
            %name = "!";
        }
        else
        {
            %value = %arguments;
            %list = "Filter";
            %name = "";
        }

        %count = IndexList_Count(%data,%list);
        if(%count == 0)
        {
            %client.chatMessage("\c5" @ %name @ "Empty");
            return;
        }
        else
        {
            %client.chatMessage("\c5" @ %name @ "List:");
        }

        for(%i = 0; %i < %count; %i++)
        {
            %key = IndexList_Get(%data,%list,%i);
            if(%value !$= "" && striPos(%key,%value) < 0)
            {
                continue;
            }
            %client.chatMessage(ChatFilter_FilterFeedback(%list,%i));
        }
    }
}

package chatFilter
{
    function serverCmdMessageSent(%client, %text)
    {  
        
    }
};