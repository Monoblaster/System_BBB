function Role_Create(%name,%color,%components)
{
	%role = new ScriptObject()
	{
		class = "Role";
		
		name = %name;
		color = %color;

		components = %components;
	};
	return %role;
}

function Role::Instance(%role)
{
	%holder = ComponentHolder_Create(%role.components);
	%holder.role = %role;
	return %holder;
}

if(!isObject($TTT::RoleGroup))
{
	$TTT::RoleGroup = new ScriptGroup()
	{
		class = "RoleGroup";
	};
}

function RoleGroup::Set(%obj,%role,%name)
{
	%obj._[%name] = %role;
	%obj.add(%role);
}

function RoleGroup::Get(%obj,%role)
{
	return %obj._[%name];
}

//TODO: something smarter than globals
function TTT_CreateRoles()
{
	$TTT::RoleGroup.deleteAll();

	
	%curr = Component_Create("Traitor");
	%components = %components SPC %curr;

	%role = Role_Create("Traitor","\c0",lTrim(%components));
	$TTT::RoleGroup.set(%role,%role.name);
	

	%curr = Component_Create("Innocent");
	%components = %components SPC %curr;

	%role = Role_Create("Innocent","\c0",lTrim(%components));
	$TTT::RoleGroup.set(%role,%role.name);


	%curr = Component_Create("Detective");
	%components = %components SPC %curr;

	%role = Role_Create("Detective","\c0",lTrim(%components));
	$TTT::RoleGroup.set(%role,%role.name);
	
	$TTT::DefaultRoleList = "Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective Traitor Innocent Innocent Innocent Traitor Innocent Innocent Detective";
}