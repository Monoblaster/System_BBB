$TTT::Data = 1;
package TTT_Data
{
	function MiniGameSO::addMember(%this, %client)
	{
		Parent::addMember(%this, %client);
		%client.DataInstance_ListLoad();
		if(%client.dataInstance($TTT::Data).oopsies $= "")
		{
			%client.dataInstance($TTT::Data).oopsies = 3;
		}
	}
	
	function MiniGameSO::removeMember(%this, %client)
	{
		%client.DataInstance_ListSave();
		Parent::removeMember(%this, %client);
	}
};
activatePackage("TTT_Data");