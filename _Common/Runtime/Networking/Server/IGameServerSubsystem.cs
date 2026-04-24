//-----------------------------------------------------------------------
// <copyright file="IGameServerSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System.Threading.Tasks;

    public interface IGameServerSubsystem
    {
        string Name { get; }

        void Initialize(GameServer gameServer);

        Task<bool> Run();

        Task Shutdown();

        Task<bool> AllowPlayerToJoin(UserInfo userInfo);

        Task UpdatePlayerInfo(UserInfo userInfo);
    }
}
