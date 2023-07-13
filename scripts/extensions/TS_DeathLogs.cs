$DeathLogCount = 0;

package BBBDeathLogs
{
     function GameConnection::onDeath(%cl, %sObj, %kCl, %dmgType, %dmgLoc)
     {
          fcbn(zin).chatMessage("user died:" SPC %cl);
          Parent::onDeath(%cl, %sObj, %kCl, %dmgType, %dmgLoc);
          fcbn(zin).chatMessage("still running");
          if($BBB::Round::Phase !$= "Round")
               return;
          fcbn(zin).chatMessage("passed round check");

          %dlmsg = "<color:" @ (%kCl.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" @ %kCl.name SPC "(" @ %kCl.role @ ") \c6killed <color:" @ (%cl.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" SPC %cl.name SPC "(" @ %cl.role @ ")\c6 at" SPC getStringFromTime($BBB::rTimeLeft);
          fcbn(zin).chatMessage(%dlmsg);

          if(%kCl == %cl || !isObject(%kCl))
               %dlmsg = "<color:" @ (%cl.role $= "Traitor" ? "FF7744" : "4477FF") @ ">" @ %cl.name SPC "(" @  %cl.role @ ")" SPC "has died.";

          $DeathLog[$DeathLogCount] = %dlmsg;
          $DeathLogCount++;
     }
};
//activatePackage(BBBDeathLogs);

function clearDeathLogs()
{
     for(%i = 0; %i < $DeathLogCount; %i++)
     {
          echo("\c2The death log" SPC $DeathLog[%i] SPC "\c2will be removed.");
          $DeathLog[%i] = "";
     }
     $DeathLogCount = 0;
}

function announceDeathLogs()
{
     announce("\c6The \c0Traitors \c6were \c0" @ bbb_addListSeperators($BBB::Traitors) @ ".");
     for(%i = 0; %i < $DeathLogCount; %i++)
          announce($DeathLog[%i]);
}
