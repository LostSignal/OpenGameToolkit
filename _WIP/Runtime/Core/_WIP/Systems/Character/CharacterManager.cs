//-----------------------------------------------------------------------
// <copyright file="CharacterManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class CharacterManager : Manager
    {
        public static readonly CharacterInfo Empty = default;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public CharacterInfo MainCharacterInfo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public CharacterInfo MainCharaterCameraInfo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        //// public override void Initialize()
        //// {
        ////     //// Wait for UpdateManager
        //// 
        ////     //// TODO [bgish]: Need a way to force this to execute before almost anything else
        ////     //// UpdateManger.Instance.RegisterOnUpdate(DoWork);
        //// 
        ////     this.SetInstance(this);
        //// }

        private void DoWork(float deltaTime)
        {
            bool foundMainCharacter = false;

            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                var character = Character.AllCharacters[i];

                if (character.IsMainCharacter)
                {
                    foundMainCharacter = true;
                    this.MainCharacterInfo = new CharacterInfo
                    {
                        Position = character.Transform.position,
                        Rotation = character.Transform.rotation,
                        Forward = character.Transform.forward,
                        TeamId = character.TeamId,
                    };
                }

                if (foundMainCharacter == false)
                {
                    this.MainCharacterInfo = Empty;
                }
            }

            // mainPlayer.Postion = blah;
            // mainPlyaer.Roatation = blah;
            // mainPlayer.Forward = blah;
            // mainPlayer.Camera = blah;
        }
    }
}

#endif
