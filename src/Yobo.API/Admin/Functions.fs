module Yobo.API.Admin.Functions

open System
open Yobo.Shared.Auth.Domain
open FSharp.Rop
open Yobo.Core.Users
open Yobo.Core
open Yobo.Shared.Auth
open System.Security.Claims
open Yobo.Shared.Communication

let mapToUser (u:Yobo.Core.Users.ReadQueries.User) =
    {
        Id = u.Id
        Email = u.Email
        FirstName = u.FirstName
        LastName = u.LastName
        ActivatedUtc = u.ActivatedUtc |> Option.map (fun x -> DateTime(x.Ticks, DateTimeKind.Utc))
    } : Yobo.Shared.Admin.Domain.User