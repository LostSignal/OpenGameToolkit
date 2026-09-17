//-----------------------------------------------------------------------
// <copyright file="RoomCodeUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public static class RoomCodeUtil
    {
        private const string ValidMatchNameCharacters = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private static readonly System.Random random = new();

        public static string GenerateRandomFourDigitCode()
        {
            return BetterStringBuilder.New()
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .ToString();
        }
    }
}
