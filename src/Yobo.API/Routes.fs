module Yobo.API.Routes

open Giraffe
open FSharp.Control.Tasks.V2

open Yobo.Shared.Communication
open Yobo.Shared.Text
open Yobo.Shared.Validation


let webApp : HttpHandler =
    subRoute "/api" <| choose [
        
        route "/register" >=>
            fun next ctx ->
                task {
                    let! acc = ctx.BindJsonAsync<Yobo.Shared.Login.Register.Domain.Account>()
                    let err = TextValue.FirstName |> ValidationError.IsEmpty |> List.singleton |> ServerError.ValidationError
                    return! RequestErrors.BAD_REQUEST err next ctx
                }
    ]