// =================================================
// System_BBB - By Alphadin, LegoPepper, Nobot     
// =================================================
// File Name: SlowMoDeath.cs								
// Description: Slow motion death at the end.
// =================================================
// Table of Contents:                       
//        
// 1. Preferences
// 2. Package
							  
// =================================================

// =================================================
// 1. Preferences
// =================================================
$BBB::SlowMo::Scale = 0.4;

// =================================================
// 2. Package
// =================================================
package BBB_SlowMoDeath
{
	function BBB_Minigame::roundEnd(%so, %type, %parent)
	{
		parent::roundEnd(%so, %type);
		
		schedule(100, %so, setTimescale, $BBB::SlowMo::Scale);
		schedule($BBB::Time::Shock, %so, setTimescale, 1);
	}
};
activatePackage(BBB_SlowMoDeath);