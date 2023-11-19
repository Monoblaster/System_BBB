package TTT_Data
{
	function MiniGameSO::addMember(%this, %client)
	{
		Parent::addMember(%this, %member);
		%member.DataInstance_ListLoad();
	}
	
	function MiniGameSO::removeMember(%this, %client)
	{
		%member.DataInstance_ListSave();
		Parent::removeMember(%this, %member);
	}
};
activatePackage("TTT_Data");