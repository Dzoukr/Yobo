module Yobo.Client.Router

open System
open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type Page =
    | Calendar
    // auth
    | Login
    | Registration
    | AccountActivation of Guid
    | ForgottenPassword
    | ResetPassword of Guid

[<RequireQualifiedAccess>]    
module Page =
    
    module private Paths =
        let [<Literal>] Login = "login"
        let [<Literal>] Calendar = "calendar"
        let [<Literal>] Registration = "registration"
        let [<Literal>] ForgottenPassword = "forgotten-password"
    
    let private basicMapping =
        [
            [ Paths.Login ], Login
            [ Paths.Registration ], Registration
            [ Paths.Calendar ], Calendar
            [ Paths.ForgottenPassword ], ForgottenPassword
        ]
    
    let parseFromUrlSegments = function
        | [ Yobo.Shared.ClientPaths.AccountActivation; Route.Guid id ] -> AccountActivation id
        | [ Yobo.Shared.ClientPaths.ResetPassword; Route.Guid id ] -> ResetPassword id
        | path ->
            basicMapping
            |> List.tryFind (fun (p,_) -> p = path)
            |> Option.map snd
            |> Option.defaultValue Calendar
        
    let toUrlSegments = function
        | AccountActivation i -> [ Yobo.Shared.ClientPaths.AccountActivation; string i ]
        | ResetPassword i -> [ Yobo.Shared.ClientPaths.ResetPassword; string i ]
        | page ->
            basicMapping
            |> List.tryFind (fun (_,p) -> p = page)
            |> Option.map fst
            |> Option.defaultValue []

module Router = 
    let goToUrl (e: MouseEvent) =
        e.preventDefault()
        let href : string = !!e.currentTarget?attributes?href?value
        Router.navigatePath href |> List.map (fun f -> f ignore) |> ignore
        
    let navigatePage (p:Page) = p |> Page.toUrlSegments |> Array.ofList |> Router.navigatePath     