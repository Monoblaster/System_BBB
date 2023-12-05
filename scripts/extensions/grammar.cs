function AnOrA(%str)
{
	if(strChr(getSubStr(%str),0,1,"aeiou") $= "" )
	{
		return "a" SPC %str;
	}

	return "an" SPC %str;
}