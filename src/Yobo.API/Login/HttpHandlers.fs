module Yobo.API.Login.HttpHandlers

open Giraffe
open FSharp.Control.Tasks.V2

open Yobo.Shared.Text
open Yobo.Shared.Validation
open Yobo.Shared.Communication
open Microsoft.AspNetCore.Http

let register : HttpHandler = fun next (ctx:HttpContext) ->
    task {
        let! acc = ctx.BindJsonAsync<Yobo.Shared.Login.Register.Domain.Account>()
        let err = TextValue.FirstName |> ValidationError.IsEmpty |> List.singleton |> ServerError.ValidationError
        return! RequestErrors.BAD_REQUEST err next ctx
    }

//let test = bindJson<Yobo.Shared.Login.Register.Domain.Account> (fun x -> x.Email)
