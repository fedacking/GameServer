﻿namespace LeagueSandbox.GameServer.Content
{
    public interface IPassiveData
    {
        string PassiveAbilityName { get; set; }
        int[] PassiveLevels { get; set; }
        string PassiveLuaName { get; set; }
        string PassiveNameStr { get; set; }
    }
}