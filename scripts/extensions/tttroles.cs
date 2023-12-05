function Role_Create(%player,%components)
{
	if(isObject(%player.role))
	{
		error("Player has a role" SPC %player);
		return "";
	}
	
	%role = ComponentHolder_Create(%componenets);
	%role.player = %player;
		
	%player.role = %role;
	return %role;
}

function TTT_CreateRoles()
{
	%curr = RoleComponent_Create("Traitor");
	%curr.name = "Traitor";
	%curr.color = "\c0";

	%curr = RoleComponent_Create("Innocent");
	%curr.name = "Innocent";
	%curr.color = "\c2";

	%curr = RoleComponent_Create("Detective");
	%curr.name = "Detective";
	%curr.color = "\c1";
}