module Yobo.Client.Router

open System
open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type AnonymousPage =
    | Login
    | Registration
    | AccountActivation of Guid
    | ForgottenPassword
    | ResetPassword of Guid

type SecuredPage =    
    | Calendar
    | MyAccount
    // admin
    | Users
    | Lessons

[<RequireQualifiedAccess>]    
module SecuredPage =
    let isAdminOnly = function
        | Users | Lessons -> true
        | _ -> false

type Page =
    | Anonymous of AnonymousPage
    | Secured of SecuredPage

[<RequireQualifiedAccess>]    
module Page =
    
    let defaultPage = (Secured Calendar)
    
    module private Paths =
        let [<Literal>] Login = "login"
        let [<Literal>] Calendar = "calendar"
        let [<Literal>] MyAccount = "my-account"
        let [<Literal>] Users = "users"
        let [<Literal>] Lessons = "lessons"
        let [<Literal>] Registration = "registration"
        let [<Literal>] ForgottenPassword = "forgotten-password"
    
    let private basicMapping =
        [
            [ Paths.Login ], Anonymous Login
            [ Paths.Registration ], Anonymous Registration
            [ Paths.ForgottenPassword ], Anonymous ForgottenPassword
            [ Paths.Calendar ], Secured Calendar
            [ Paths.MyAccount ], Secured MyAccount
            [ Paths.Users ], Secured Users
            [ Paths.Lessons ], Secured Lessons
        ]
    
    let parseFromUrlSegments = function
        | [ Yobo.Shared.ClientPaths.AccountActivation; Route.Guid id ] -> Anonymous <| AccountActivation id
        | [ Yobo.Shared.ClientPaths.ResetPassword; Route.Guid id ] -> Anonymous <| ResetPassword id
        | path ->
            basicMapping
            |> List.tryFind (fun (p,_) -> p = path)
            |> Option.map snd
            |> Option.defaultValue (Secured Calendar)
        
    let toUrlSegments = function
        | Anonymous (AccountActivation i) -> [ Yobo.Shared.ClientPaths.AccountActivation; string i ]
        | Anonymous (ResetPassword i) -> [ Yobo.Shared.ClientPaths.ResetPassword; string i ]
        | page ->
            basicMapping
            |> List.tryFind (fun (_,p) -> p = page)
            |> Option.map fst
            |> Option.defaultValue []

module Router = 
    let goToUrl (e:MouseEvent) =
        e.preventDefault()
        let href : string = !!e.currentTarget?attributes?href?value
        Router.navigatePath href |> List.map (fun f -> f ignore) |> ignore
        
    let navigatePage (p:Page) = p |> Page.toUrlSegments |> Array.ofList |> Router.navigatePath     