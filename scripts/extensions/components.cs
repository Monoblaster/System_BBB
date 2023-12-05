// An instance that holds onto an ordered list Components and can start a callback chain with their callbacks
function ComponentHolder_Create(%components)
{
	%count = getWordCount(%components);
	for(%i = 0; %i < %count; %i++)
	{
		%comp = getWord(%components,%i);	
		if(%comp.class !$= "Component")
		{
			error("Invalid component" SPC %comp);
			return "";
		}

		%componenets = setWord(%componenets,%i,%comp.getid());
	}

	return = new ScriptObject()
	{
		class = "ComponentHolder";
		components = %components;
	};
}

function ComponentHolder::Prepend(%obj,%comp)
{
	if(%comp.class !$= "Component")
	{
		error("Invalid component" SPC %comp);
		return "";
	}

	%obj.components = %obj.components SPC %comp.getid();
	return %obj;
}

function ComponentHolder::Remove(%obj,%comp)
{
	if(%comp.class !$= "Component")
	{
		error("Invalid component" SPC %comp);
		return "";
	}
	%comp = %comp.getid();

	%list = %obj.components;
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%curr = getWord(%list,%i);
		if(%curr == %comp)
		{
			%obj.components = removeWord(%list,%i);
			return %obj;
		}
	}
}

function ComponentHolder::IsComponent(%obj,%comp)
{
	if(%comp.class !$= "Component")
	{
		error("Invalid component" SPC %comp);
		return 0;
	}
	%comp = %comp.getid();

	%list = %obj.components;
	%count = getWordCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%curr = getWord(%list,%i);
		if(%curr == %comp)
		{
			return true;
		}
	}
	return false;
}

function ComponentHolder::StartCallback(%obj,%name,%target,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l)
{
	if(%obj.doingCallback !$= "")
	{
		error("Cannot start another callback on the same instance");
		return "";
	}
	%obj.callback = %name;
	%obj.callbackTarget = %target;
	%obj.callbackIndex = -1;
	
	%obj.Continue(%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l);

	%obj.callback = "";
}

function ComponentHolder::Continue(%obj,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l)
{
	if(%obj.doingCallback $= "")
	{
		error("Cannot continue without starting a callback");
		return "";
	}

	%callback = %obj.callback;

	%list = %obj.components;
	%count = getWordCount(%list);
	for(%num = %obj.callbackIndex + 1; %num < %count; %num++)
	{
		%func = getWord(%list,%num).getFunction(%callback);
		if(!isFunction(%func))
		{
			continue;
		}

		%obj.callbackIndex = %num;
		call(%func,%obj,%obj.callbackTarget,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l);
		return "";
	}
	
	return "";
}

// A singleton that can hold multipe function definitions for different callback names
function Component_Create(%name)
{
	%name = "Component_" @ %name;
	if(nameToID(%name) > 0)
	{
		%name.ClearCallbacks();
		return;
	}

	%obj = new ScriptObject()
	{
		class = "Component";
	};
	%obj.setName(%name);
	return %obj;
}

function Component::Callback(%obj,%name,%func)
{
	if(%obj._[%name] $= "")
	{
		%obj.CallbackList = lTrim(%obj.CallbackList TAB %name);
	}

	%obj._[%name] = %func;
}

function Component::GetFunction(%obj,%name)
{
	return getWord(%obj._[%name]);
}


function Component::ClearCallbacks(%obj)
{
	%list = %obj.CallbackList;
	%count = getFieldCount(%list);
	for(%i = 0; %i < %count; %i++)
	{
		%obj._[getField(%list, %i)] = "";
	}
	%obj.CallbackList = "";
}

