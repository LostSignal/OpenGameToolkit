//-----------------------------------------------------------------------
// <copyright file="CharacterManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections.Generic;
    using global::PlayFab;
    using global::PlayFab.ClientModels;

    public class CharacterManager
    {
        private readonly PlayFabManager playfabManager;
        private List<CharacterResult> characterCache;

        public CharacterManager(PlayFabManager playfabManager, List<CharacterResult> characters)
        {
            this.playfabManager = playfabManager;
            this.characterCache = characters;
        }

        public void InvalidateCache()
        {
            this.characterCache = null;
        }

        public UnityTask<List<CharacterResult>> GetUserCharacters(bool forceUpdate = false)
        {
            if (forceUpdate)
            {
                this.characterCache = null;
            }

            if (this.characterCache != null)
            {
                return UnityTask<List<CharacterResult>>.Empty(this.characterCache);
            }
            else
            {
                return UnityTask<List<CharacterResult>>.Run(FetchCharacters());
            }

            IEnumerator<List<CharacterResult>> FetchCharacters()
            {
                var getCharacters = this.playfabManager.Do<ListUsersCharactersRequest, ListUsersCharactersResult>(new ListUsersCharactersRequest(), PlayFabClientAPI.GetAllUsersCharactersAsync);

                while (getCharacters.IsDone == false)
                {
                    yield return null;
                }

                this.characterCache = getCharacters.Value.Characters;

                yield return this.characterCache;
            }
        }

        public UnityTask<GetCharacterDataResult> GetCharacterData(string characterId, List<string> keys)
        {
            // TODO [bgish]: Eventually cache this data
            return this.GetCharacterData(this.playfabManager.User.PlayFabId, characterId, keys);
        }

        public UnityTask<GetCharacterDataResult> GetCharacterData(string playfabId, string characterId, List<string> keys)
        {
            // TODO [bgish]: Eventually cache this data
            return this.playfabManager.Do<GetCharacterDataRequest, GetCharacterDataResult>(
                new GetCharacterDataRequest
                {
                    PlayFabId = playfabId,
                    CharacterId = characterId,
                    Keys = keys,
                },
                PlayFabClientAPI.GetCharacterDataAsync);
        }

        public UnityTask<UpdateCharacterDataResult> UpdateCharacterData(string characterId, Dictionary<string, string> data, UserDataPermission permission = UserDataPermission.Private)
        {
            return this.playfabManager.Do<UpdateCharacterDataRequest, UpdateCharacterDataResult>(
                new UpdateCharacterDataRequest
                {
                    CharacterId = characterId,
                    Data = data,
                    Permission = permission,
                },
                PlayFabClientAPI.UpdateCharacterDataAsync);
        }
    }
}

#endif
