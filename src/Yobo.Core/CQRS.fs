namespace Yobo.Core

type CoreCommand =
    | Users of Users.Command
    | Lessons of Lessons.Command

type CoreEvent = 
    | Users of Users.Event
    | Lessons of Lessons.Event