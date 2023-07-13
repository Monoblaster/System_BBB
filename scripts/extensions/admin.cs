//gamemode commands
CommandHandler_New("mute","Admin_Mute",1,"target \t words 5","target minutes","Mutes target for minutes with decimals");
function Admin_Mute(%client,%target,%minutes)
{

}

CommandHandler_New("slay","Admin_Slay",1,"target \t words 1","target [rounds]","Slays target optionaly for rounds");
function Admin_Slay(%client,%target,%rounds)
{

}

CommandHandler_New("rev","Admin_Revive",1,"target","target","Revives target");
function Admin_Revive(%client,%target)
{

}

//chat commands
CommandHandler_New("report","Admin_Report",0,"words 18","","Sends a message to online staff");
function Admin_Report(%client,%str)
{

}

CommandHandler_New("dm","Admin_DM",0,"target \t words 18","target","Sends a message to target");
function Admin_DM(%client,%target,%str)
{
	
}

//admin chat commands
CommandHandler_New("ac","Admin_AdminChat",1,"words 18","","Admin Chat");
function Admin_AdminChat(%client,%str)
{
	
}

CommandHandler_New("sc","Admin_SuperAdminChat",2,"words 18","","Super Admin Chat");
function Admin_SuperAdminChat(%client,%str)
{
	
}

CommandHandler_New("r","Admin_Reply",1,"words 18","","Replies to the last report");
function Admin_Reply(%client,%str)
{

}

CommandHandler_New("rn","Admin_ReplyName",1,"target \t words 18","target","Replies to the target");
function Admin_ReplyName(%client,%target,%str)
{

}