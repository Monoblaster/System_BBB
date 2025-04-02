function hex2dec(%v)
{
    %dec = 0;
    %count = strLen(%v);
    %conversionstring = "0123456789abcdef";
    for(%i = 0; %i < %count; %i++)
    {
        %dec += mPow(16, (%count-%i)-1) * striPos(%conversionstring,getSubStr(%v,%i,1));
    }
    return %dec;
}

function dec2hex(%v)
{
    %v = mfloor(%v);
    %hex = "";
    %conversionstring = "0123456789abcdef";
    %r = %v % 16;
    while(%v - %r != 0)
    {
        %hex = getSubStr(%conversionstring,%r,1) @ %hex;
        %v = (%v - %r) / 16;
        %r = %v % 16;
        if(%safety++ > 20)
        {
            talk("BROKEN SAFETY: dec2hex colorutils");
            return;
        }
    }
    return getSubStr(%conversionstring,%r,1) @ %hex;
}

function fillEmptyChars(%string,%num,%fill)
{
    %count = %num - strLen(%string);
    for(%i = 0; %i < %count; %i++)
    {
        %string = %fill @ %string;
    }
    return %string;
}

function rgb2hsv(%rgb)
{
    %r = hex2dec(getSubStr(%rgb,0,2))/255;
    %g = hex2dec(getSubStr(%rgb,2,2))/255;
    %b = hex2dec(getSubStr(%rgb,4,2))/255;

    %max = getMax(getMax(%r,%g),%b);
    %min = getMin(getMin(%r,%g),%b);
    %delta = %max - %min;

    if(%delta == 0)
    {
        %h = 0;
    }
    else if(%max == %r)
    {
        %h = 60 * ((%g-%b)/%delta%6);
    }
    else if(%max == %g)
    {
        %h = 60 * ((%b-%r)/%delta+2);
    }
    else
    {
        %h = 60 * ((%r-%g)/%delta+4);
    }

    
    if(%max == 0)
    {
        %s = 0;
    }
    else
    {
        %s = %delta/%max;
    }

    return %h SPC %s SPC %max;
}


function hsv2rgb(%h,%s,%v)
{
    %c = %v*%s;
    %hprime = %h/60;
    %x = %c*(1-mAbs(%hprime%2-1));
    if(0 <= %hprime && %hprime < 1)
    {
        %r = %c;%g = %x;%b = 0;
    }
    else if(1 <= %hprime && %hprime < 2)
    {
        %r = %x;%g = %c;%b = 0;
    }
    else if(2 <= %hprime && %hprime < 3)
    {
        %r = 0;%g = %c;%b = %x;
    }
    else if(3 <= %hprime && %hprime < 4)
    {
        %r = 0;%g = %x;%b = %c;
    }
    else if(4 <= %hprime && %hprime < 5)
    {
        %r = %x;%g = 0;%b = %c;
    }
    else
    {
        %r = %c;%g = 0;%b = %x;
    }
    %m = %v - %c;
    return fillEmptyChars(dec2hex((%r+%m)*255),2,0) @ fillEmptyChars(dec2hex((%g+%m)*255),2,0) @ fillEmptyChars(dec2hex((%b+%m)*255),2,0);
}