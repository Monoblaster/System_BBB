//CONVERSION
function dec2base(%dec,%base)
{
	%str = "";
	while(%dec > 0)
	{
		%rem = %dec % %base;
		%dec = mFloor(%dec / %base);
		%str = getSubStr("0123456789ABCDEFHIJKLMNOPQRSTUVWXYZ", %rem, 1) @ %str;
	}
	
	return %str;
}

$Util::Base2Dec["A"] = 10;
$Util::Base2Dec["B"] = 11;
$Util::Base2Dec["C"] = 12;
$Util::Base2Dec["D"] = 13;
$Util::Base2Dec["E"] = 14;
$Util::Base2Dec["F"] = 15;
$Util::Base2Dec["G"] = 16;
$Util::Base2Dec["H"] = 17;
$Util::Base2Dec["I"] = 18;
$Util::Base2Dec["J"] = 19;
$Util::Base2Dec["K"] = 20;
$Util::Base2Dec["L"] = 21;
$Util::Base2Dec["M"] = 22;
$Util::Base2Dec["N"] = 23;
$Util::Base2Dec["O"] = 24;
$Util::Base2Dec["P"] = 25;
$Util::Base2Dec["Q"] = 26;
$Util::Base2Dec["R"] = 27;
$Util::Base2Dec["S"] = 28;
$Util::Base2Dec["T"] = 29;
$Util::Base2Dec["U"] = 30;
$Util::Base2Dec["V"] = 31;
$Util::Base2Dec["W"] = 32;
$Util::Base2Dec["X"] = 33;
$Util::Base2Dec["Y"] = 34;
$Util::Base2Dec["Z"] = 35;

function base2dec(%str,%base)
{
	%d = 0;
	%count = strLen(%str);
	for(%i = 0; %i < %count; %i++)
	{
		%w = %count - %i - 1;
		%n = getSubStr(%str,%i,1);
		if(%n !$= "0" && %n == 0)
		{
			%n = $Util::Base2Dec[%n];
		}

		%d += %n * mPow(%base,%w);
	}

	return %d;
}

//COLOR CONVERSION
function hex2co(%hex)
{
	%r = base2Dec(getSubStr(%hex,0,2),16) / 255;
	%g = base2Dec(getSubStr(%hex,2,2),16) / 255;
	%b = base2Dec(getSubStr(%hex,4,2),16) / 255;

	return %r SPC %g SPC %b;
}

function co2hex(%co)
{
	%r = "00" @ dec2base(getWord(%co,0) * 255,16);
	%g = "00" @ dec2base(getWord(%co,1) * 255,16);
	%b = "00" @ dec2base(getWord(%co,2) * 255,16);

	%r = getSubStr(%r,strLen(%r) - 2, 2);
	%g = getSubStr(%g,strLen(%g) - 2, 2);
	%b = getSubStr(%b,strLen(%b) - 2, 2);

	return %r @ %g @ %b;
}

function floatLerp(%from, %to, %at)
{
	%at = mClampF(%at, 0, 1.0);
	%to = mClampF(%to, 0, 1.0);
	%from = mClampF(%from, 0, 1.0);
	return mClampF(((%to - %from) * %at) + %from, 0, 1.0);
}

function coLerp(%from, %to, %at)
{
	%r = floatLerp(getWord(%from, 0), getWord(%to, 0), %at);
	%g = floatLerp(getWord(%from, 1), getWord(%to, 1), %at);
	%b = floatLerp(getWord(%from, 2), getWord(%to, 2), %at);

	return %r SPC %g SPC %b;
}

function hexLerp(%from, %to, %at)
{
	return co2hex(coLerp(hex2co(%from),hex2co(%to),%at));
}

//MISC
function GetSourceClient(%source)
{
	if (isObject(%source))
	{
		if (%source.getClassName () $= "GameConnection")
		{
			%sourceClient = %source;
		}
		else 
		{
			%sourceClient = %source.client;
		}
	}

	if (isObject (%sourceObject))
	{
		if (%sourceObject.getType () & $TypeMasks::VehicleObjectType)
		{
			if (%sourceObject.getControllingClient ())
			{
				%sourceClient = %sourceObject.getControllingClient ();
			}
		}
	}

	return %sourceClient;
}

function addListSeperators(%s)
{
	%wcount = getWordCount(%s); 
	for(%j = 0; %j < %wcount; %j++)
	{
		%lists = "";
		if(%j <= %wCount - 2)
		{
			if(%wCount > 2)
			{
				%lists = ",";
			}
			
			if(%j == %wCount - 2)
			{
				%lists = %lists @ " and";
			}
		}
		%w = getWord(%s,%j);
		%s = setWord(%s,%j,%w @ "\c6" @ %lists);
	}
	return %s;
}