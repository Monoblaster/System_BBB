function Timer_Create()
{
	return new ScriptObject(){class = "Timer";};
}

function Timer::Set(%timer,%ms)
{
	if(%ms !$= "")
	{
		%timer.m = %ms;
		if(!isEventPending(%timer.x))
		{
			%timer.l = getRealTime();
			%timer.x = %timer.schedule(33,"Tick");
		}
	}

	return %timer;
}

function Timer::SetTick(%timer,%callback)
{
	%timer.t = %callback;

	return %timer;
}

function Timer::SetStop(%timer,%callback)
{
	%timer.s = %callback;

	return %timer;
}

function Timer::Get(%timer)
{
	return %timer.m;
}

function Timer::Tick(%timer)
{
	%t = getRealTime();
	if((%timer.m -= %t - %timer.l) > 0)
	{
		call(%timer.t);
		%timer.x = %timer.schedule(33,"Tick");
	}
	else
	{
		echo("call" SPC %timer.s);
		call(%timer.s);
	}
	%timer.l = %t;
}