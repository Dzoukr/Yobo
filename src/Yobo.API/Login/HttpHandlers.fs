module Yobo.API.Login.HttpHandlers

open Giraffe

open Yobo.Shared.Login.Domain
open FSharp.Rop
open Yobo.Core
open Yobo.Core.Users
open System

let login loginFn (acc:Login) =
    result {
        let! user = loginFn acc.Email acc.Password

        return user
    }
