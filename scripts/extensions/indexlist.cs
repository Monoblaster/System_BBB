function IndexList_Count(%obj,%name)
{
    return %obj.indexListCount[%name] + 0;
}

function IndexList_Insert(%obj,%name,%value,%index)
{
    %count = IndexList_Count(%obj,%name);
    if(%index $= "")
    {
        %index = %count;
    }
    else
    {
        for(%i = %count - 1; %i >= %index; %i--)
        {
            %obj.indexList[%name,%i + 1] = %obj.indexList[%name,%i];
        }
        %obj.indexList[%name,%index] = %value;
        %obj.indexListCount[%name] = getMax(%index + 1,%count + 1);
        return %index;
    }

    %obj.indexList[%name,%index] = %value;
    %obj.indexListCount[%name] = %count + 1;
    return %index;
}

function IndexList_Set(%obj,%name,%index,%value)
{
    %count = IndexList_Count(%obj,%name);

    %obj.indexList[%name,%index] = %value;
    %obj.indexListCount[%name] = getMax(%index + 1,%count + 1);
    return %index;
}

function IndexList_Get(%obj,%name,%index)
{
    return %obj.indexList[%name,%index];
}

function IndexList_Remove(%obj,%name,%index)
{
    %count = IndexList_Count(%obj,%name);

    for(%i = %index; %i < %count; %i++)
    {
        %obj.indexList[%name,%i] = %obj.indexList[%name,%i + 1];
    }
    %obj.indexListCount[%name] = getMax(%index - 1,%count - 1);
    return %index;
}

function IndexList_Clear(%obj,%name)
{
    %obj.indexListCount[%name] = 0;
    return "";
}

function IndexList_Find(%obj,%name,%search)
{
    %search = strreplace(%search,"%a","%obj.indexList[%name,%i]");

    %count = IndexList_Count(%obj,%name);

    return eval("for(%i=0;%i<%count;%i++){if("@%search@"){return%i;}}return-1;");
}

//bottom up merge sort
function IndexList_Sort(%obj,%name,%sort)
{
    if(%sort $= "")
    {
        %sort = "%a[%i] <= %a[%j]";
    }
    else
    {
        %sort = strreplace(strreplace(%sort, "%a", "%a[%i]"), "%b", "%a[%j]");
    }

    %count = IndexList_Count(%obj,%name);
    for(%i = 0; %i < %count; %i++)
    {
        %a[%i] = %obj.indexList[%name,%i];
    }

    // "for(%width = 1; %width < %count; %width *= 2)
    // {
    //     for(%left = 0; %left < %count; %left = %left + 2 * %width)
    //     {
    //         if(%left + %width >= %count)
    //         {
    //             for(%i = %left; %i < %count; %i++)
    //             {
    //                 %b[%i] = %a[%i];
    //             }
    //             continue;
    //         }
            
    //         if(%left + 2 * %width > %count)
    //         {
    //             %i = %left;
    //             %j = %left + %width;
    //             for(%k = %left; %k < %count; %k++)
    //             {
    //                 if(%i < %left + %width && (%j >= %count || "@%sort@"))
    //                 {
    //                     %b[%k] = %a[%i];
    //                     %i++;
    //                     continue;
    //                 }

    //                 %b[%k] = %a[%j];
    //                 %j++;
    //             }
    //         }
    //         else
    //         {
    //             %i = %left;
    //             %j = %left + %width;
    //             for(%k = %left; %k < %left + 2 * %width; %k++)
    //             {
    //                 if(%i < %left + %width && (%j >= %left + 2 * %width || "@%sort@"))
    //                 {
    //                     %b[%k] = %a[%i];
    //                     %i++;
    //                     continue;
    //                 }

    //                 %b[%k] = %a[%j];
    //                 %j++;
    //             }
    //         }
    //     }

    //     for(%i = 0; %i < %count; %i++)
    //     {
    //         %a[%i] = %b[%i];
    //     }
    // }"
    
    //eval runs within this context so we can use variables and give variables
    eval("for(%width=1;%width<%count;%width*=2){for(%left=0;%left<%count;%left=%left+2*%width){if(%left+%width>=%count){for(%i=%left;%i<%count;%i++)"@
    "{%b[%i]=%a[%i];}continue;}if(%left+2*%width>%count){%i=%left;%j=%left+%width;for(%k=%left;%k<%count;%k++){if(%i<%left+%width&&(%j>=%count||"@%sort@
    ")){%b[%k]=%a[%i];%i++;continue;}%b[%k]=%a[%j];%j++;}}else{%i=%left;%j=%left+%width;for(%k=%left;%k<%left+2*%width;%k++){if(%i<%left+%width&&(%j>=%"@
    "left+2*%width||"@%sort@")){%b[%k]=%a[%i];%i++;continue;}%b[%k]=%a[%j];%j++;}}}for(%i=0;%i<%count;%i++){%a[%i]=%b[%i];}}");

    for(%i = 0; %i < %count; %i++)
    {
        %obj.indexList[%name,%i] = %a[%i];
    }

    return "";
}