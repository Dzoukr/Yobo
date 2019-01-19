module Yobo.Shared.Admin.Domain

open System

type AddCredits = {
    UserId : Guid
    Credits : int
    Expiration : DateTimeOffset
}

type AddLesson = {
    Start : DateTimeOffset
    End : DateTimeOffset
    Name : string
    Description : string
}

