// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot      
// =================================================
// File Name: help.cs								
// Description: Help code, from Iban's Cityrpg (Credit: Iban)
// =================================================
if(isObject($BBBHelp))
{
	$BBBHelp.delete();
}

$BBBHelp = new scriptObject()
{
	class = "Hellen";
};
	
function Hellen::onAdd(%this)
{
	// if(%this.getName() $= "")
	// {
	// 	echo("Hellen::onAdd(): This database has no name! Deleting database..");
		
	// 	%this.schedule(0, "delete");
		
	// 	return false;
	// }
	
	%this.loadData();
	
	return true;
}

function Hellen::onRemove(%this)
{	
	return true;
}

function Hellen::loadData(%this)
{
	if(!isFile("./help.hlp"))
	{
		echo(%this.getName() @ "::onAdd(): Help file not found!");
		
		return false;
	}
	
	%t = "\c7";
	%l = "\c6";
	%a = "\c2";
	%file = new fileObject();
	%file.openForRead(findFirstFile("./help.hlp"));
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		
		if(getSubStr(%line, 0, 2) $= "//")
		{
			continue;
		}
		
		if(getSubStr(%line, 0, 1) $= "[")
		{
			%line = stripChars(%line, "[]");
			%sectionName = "";
			while(%line !$= "")
			{
				%line = NextToken(%line, "%token", "_");
				if(%this.sectionIndex[%sectionName,%token] $= "")
				{
					%this.sectionName[%sectionName,%this.sectionCount[%sectionName]+0] = %token;
					%this.sectionIndex[%sectionName,%token] = %this.sectionCount[%sectionName]+0;
					%this.sectionCount[%sectionName]++;
				}
				
				if(%sectionName $= "")
				{
					%sectionName = %token;
				}
				else
				{
					%sectionName = %sectionName @ "_" @ %token;
				}
			}
		}
		
		if(%sectionName $= "")
		{
			error("Hellen: Info without sectionname");
			return;
		}

		%line = strReplace(strReplace(strReplace(collapseEscape(%line),"<t>",%t),"<l>",%l),"<a>",%a);
		if(%this.sectionInfo[%sectionName] $= "")
		{
			%this.sectionInfo[%sectionName] = %line;
			continue;
		}

		if(%this.sectionLine[%sectionName] $= "")
		{
			%this.sectionline[%sectionName] = %line;
			continue;
		}

		%this.sectionline[%sectionName] = %this.sectionLine[%sectionName] NL %line;
	}
	
	%file.close();
	%file.delete();
	
	return true;
}

function Hellen::display(%this, %client, %base, %input)
{
	%term = %base SPC %input;
	%name = getWord(%term,getWordCount(%term)-1);
	%term = strReplace(rtrim(%term)," ","_");

	if(%this.sectioninfo[%term] $= "")
	{
		return false;
	}

	%client.chatMessage("\c2" @ strupr(%name) SPC "\c6-"  SPC %this.sectioninfo[%term]);
	%line = %this.sectionLine[%term];
	%lineCount = getRecordCount(%line);
	for(%i = 0; %i < %linecount; %i++)
	{
		%client.chatMessage(collapseEscape(getRecord(%line,%i)));
	}

	%count = %this.sectionCount[%term];
	if(%count == 0)
	{
		return true;

	}
	
	%client.chatMessage(rtrim("\c6Type \c2/help" SPC %input) SPC "\c6followed by one of the following to access a category.");
	for(%i = 0; %i < %count; %i++)
	{
		//display sub sections if any
		%sectionname = %this.sectionname[%term,%i];
		%client.chatMessage("\c6- \c2"@%sectionname SPC "\c6-\c7" SPC %this.sectioninfo[%term,%sectionName]);
	}
	return true;
}

function serverCmdHelp(%client, %a, %b, %c, %d, %e, %f)
{
	%client.chatMessage("\c2===");
	%input = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f);
	while(!$BBBHelp.display(%client,"main", %input))
	{
		%input = removeWord(%input,getWordCount(%input)-1);
		if(%c++ > 20)
		{
			talk(%input SPC "safety broken");
			break;
		}
	}

	if(%client.isAdmin)
	{
		$BBBHelp.display(%client,"admin", trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f));
	}
	%client.chatMessage("\c6Press \c2pageUp \c6or \c2pageDown \c6to scroll.");
}