### Presence System
* Functions
  * LogIn()
  * LogOut()
  * UpdatePresence(Loc String, loc string params, custom data)
  * Event FriendPresenceUpdated
  * GetFriendPresense(List or PlayFab Ids)
    * This will fire off an event whenever they are updated
    * It will go through the list (5 to 10 at a time) and get their current precense
    * It will register with Ably to get updates