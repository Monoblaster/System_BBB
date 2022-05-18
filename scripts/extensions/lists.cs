function List_NewList()
{
    %list = new ScriptObject()
	{
		class = "List";
	};

    return %list;
}

function List::ListValues(%list)
{
    %count = %list.getcount();
    for(%i = 0; %i < %count; %i++)
    {
        echo(%list.getvalue(%i));
    }
}

function List::IsTag(%list,%tag)
{
	return %list.row[%tag] !$= "";
}

function List::GettagString(%list)
{
    return %list.string;
}

function List::GetCount(%list)
{
    return getWordCount(%list.string);
}

function List::GetRow(%list,%tag)
{
    return %list.row[%tag];
}

function List::GetTag(%list,%row)
{
    return getWord(%list.string,%row);
}

function List::GetValue(%list,%row)
{
    return %list.value[%list.getTag(%row)];
}

function List::FindValue(%list,%Value)
{
    %count = %list.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %thisValue = %list.getValue(%i);
        if(strPos(%thisValue,%Value) >= 0)
        {
            return %i;
        }
    }
    return -1;
}

function List::Add(%list,%Value,%row,%tag)
{
    %count = %list.getCount();
    if(%row < 0 || %row >= %count || %row $= "")
    {
        %row = %count;
    }
    for(%i = %count; %i > %row; %i--)
    {
        %list.Swap(%i,%i - 1);
    }
    %list.Set(%row,%Value,%tag);
}

function List::Set(%list,%row,%Value,%tag)
{
	if(%tag $= "")
	{
		%tag = %list.getTag(%row);
	}

	if(!%list.istag(%tag))
	{
		if(%tag $= "")
		{
			%tag = %list.adds + 0;
			%list.adds++;
		}

		%safety = 0;
		while(%list.istag(%tag))
		{
			%tag = getRandom(0,999999);
			%safety++;
			
			if(%safety > 100)
			{
				warn("Lists: Set tag failure");
				return "";
			}
		}
	}
	
    %list.string = setWord(%list.string,%row,%tag);
    %list.row[%tag] = %row;
    %list.Value[%tag] = %Value;
}

function List::Swap(%list,%row1,%row2)
{
    %tempValue1 = %list.getValue(%row1);
    %temptag1 = %list.gettag(%row1);
	%list.row[%temptag1] = "";
	
	%tempValue2 = %list.getValue(%row2);
    %temptag2 = %list.gettag(%row2);
	%list.row[%temptag2] = "";

    %list.Set(%row1,%tempValue2,%temptag2);
    %list.Set(%row2,%tempValue1,%temptag1);
}

function List::Remove(%list,%row)
{
    %tag = %list.gettag(%row);
    if(!%list.istag(%tag))
	{
		return false;
	}
	
    %list.string = removeWord(%list.string,%row);
    %list.Value[%tag] = "";
    %list.row[%tag] = "";
	return true;
}

function List::Clear(%list)
{
    %count = %list.getCount();
    for(%i = %count - 1; %i >= 0; %i--)
    {
        %list.remove(%i);
    }
}

function List::Sort(%list,%reverse,%sortFunction)
{
	if(%sortFunction $= "" || !isFunction(%sortFunction))
	{
		%sortFunction = "list_numericalSort";
	}

	list_quicksort(%list,0,%list.getCount() - 1,%reverse,%sortFunction);
}

function list_quicksort(%list,%lo,%hi,%reverse,%sortFunction)
{
	if(%lo >= 0 && %hi >= 0 && %lo < %hi)
	{
		%partition = list_partition(%list,%lo,%hi,%reverse,%sortFunction);
		list_quicksort(%list,%lo,%partition,%reverse,%sortFunction);
		list_quicksort(%list,%partition + 1,%hi,%reverse,%sortFunction);
	}
}

function list_partition(%list,%lo,%hi,%reverse,%sortFunction)
{
	%pivot = %list.getValue(mFloor((%hi + %lo) / 2));
	
	%i = %lo - 1;
	%j = %hi + 1;
	while(true)
	{
		%i = call(%sortFunction,%list,%pivot,%i,1,%reverse);

		%j = call(%sortFunction,%list,%pivot,%j,-1,!%reverse);

		if(%i >= %j)
		{
			return %j;
		}
		
		%list.swap(%i, %j);
	}
}

function list_numericalSort(%list,%pivot,%i,%direction,%reverse)
{
	%modifier = 1;
	if(%reverse)
	{
		%modifier = -1;
	}
    while(%list.getValue(%i += %direction) * %modifier < %pivot * %modifier){}
	
	return %i;
}