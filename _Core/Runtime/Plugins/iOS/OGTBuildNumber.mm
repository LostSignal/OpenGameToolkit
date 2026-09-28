//-----------------------------------------------------------------------
// <copyright file="OGTBuildNumber.mm" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#import <Foundation/Foundation.h>
#include <string.h>

// NOTE [bgish]: Called from UnityPlatformProvider.GetIOSBuildNumber() via DllImport("__Internal").
//               The C# side marshals an IntPtr and never frees it, so the string is cached and
//               allocated only once instead of leaking a new copy on every call.
extern "C" const char* GetBuildNumber()
{
    static const char* buildNumber = NULL;

    if (buildNumber == NULL)
    {
        NSString *build = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleVersion"];

        // Returning NULL marshals to a null string in C#, so GetVersionString() falls back to Application.version
        if (build == nil)
        {
            return NULL;
        }

        buildNumber = strdup([build UTF8String]);
    }

    return buildNumber;
}
