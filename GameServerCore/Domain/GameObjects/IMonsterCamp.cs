﻿using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IMonsterCamp
    {
        byte CampIndex { get; set; }
        Vector3 Position { get; set; }
        TeamId SideTeamId { get; set; }
        string MinimapIcon { get; set; }
        byte RevealEvent { get; set; }
        float Expire { get; set; }
        int TimerType { get; set; }
        float SpawnDuration { get; set; }
        bool IsAlive { get; set; }
        float RespawnTimer { get; set; }
        List<IMonster> Monsters { get; }
        void AddMonster(IMonster monster);
        void NotifyCampActivation();
    }
}
