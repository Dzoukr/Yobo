module Yobo.API.Extensions

type System.DateTime with
    member x.ToUtc () = System.DateTime.SpecifyKind(x, System.DateTimeKind.Utc)