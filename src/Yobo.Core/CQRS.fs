namespace Yobo.Core

type CoreCommand =
    | Users of Users.Command

type CoreEvent = 
    | Users of Users.Event