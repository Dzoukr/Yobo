module Yobo.Client.View

open Yobo.Client.Router
open Domain
open Elmish
open Feliz
open Feliz.Bulma
open Feliz.Bulma.PageLoader
open Feliz.Router
open Yobo.Client.SharedView

let private displayLoggedPage (user:Yobo.Shared.UserAccount.Domain.Queries.UserAccount) (page:Page) content dispatch =
    //let item (pg:string) icon text =
        
    
    Html.div [
        Bulma.navbar [
            navbar.isLight
            prop.children [
                Bulma.container [
                    Bulma.navbarStart [
                        
                    ]
                ]
            ]
        ]
    ]
    
//    let item (pg:string) icon text =
//        let isActive = page.Path = pg
//        Navbar.Item.a [ Navbar.Item.IsActive isActive ; Navbar.Item.Option.Props [ Href pg; OnClick Router.goToUrl ] ] [
//            i [ ClassName icon; Style [ MarginRight 5] ] [ ]
//            str text
//        ]
//
//    let adminButtons =
//        match user with
//        | Some { IsAdmin = true } ->
//                [ item Router.Users.Path "fas fa-users" "Uživatelé"
//                  item Router.Lessons.Path "fas fa-calendar-alt" "Lekce" ]
//        | _ -> []
//
//    let userInfo =
//        match user with
//        | Some user ->
//            Navbar.Item.div [] [
//                Tag.tag [ Tag.Color IsInfo; Tag.Props [ Style [ MarginRight 10 ] ] ] [ sprintf "%i kreditů" user.Credits |> str ]
//                i [ ClassName "fas fa-user"; Style [ MarginRight 5 ] ] []
//                sprintf "%s %s" user.FirstName user.LastName |> str
//            ]
//        | None -> str ""
//    
//    div [] [
//        Navbar.navbar [ Navbar.Color IsLight; ] [
//            Container.container [] [
//                Navbar.Start.div [] [
//                    item Router.Calendar.Path "fas fa-calendar-alt" "Kalendář"
//                    item Router.MyLessons.Path "fas fa-user" "Můj účet"
//                ]
//                Navbar.End.div [] [
//                    yield! adminButtons
//                    yield userInfo
//                    yield Navbar.Item.div [] [
//                        div [ ClassName "buttons" ] [
//                            Button.a [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> LoggedOut |> dispatch) ] ] [
//                                str "Odhlásit"
//                            ]
//                        ]
//                    ]
//                ]
//            ]
//        ]
//        main [ Style [ PaddingTop "2rem" ] ] [
//            Container.container [ ] [
//                content
//            ]
//            Container.container [ ] [
//                a [ ClassName "terms-link"; OnClick (fun _ -> ToggleTermsView |> dispatch) ] [str "Obchodní podmínky"]
//                SharedView.termsModal termsViewed (fun _ -> ToggleTermsView |> dispatch)
//            ]
//        ]
//    ]

let showView<'model,'msg> (fn:'model -> ('msg -> unit) -> Fable.React.ReactElement) (dispatch:'msg -> unit) (m:Model) =
    let pm = m |> Model.getPageModel<'model>
    fn pm dispatch
    
let view (model:Model) (dispatch:Msg -> unit) =
    let render =
        if model.IsCheckingUser then
            PageLoader.pageLoader [
                pageLoader.isWhite
                pageLoader.isActive
                prop.children [
                    PageLoader.title "Přihlašuji vás"
                ]
            ]
        else            
            match model.PageWithModel.Page with
            | Login -> model |> showView Pages.Login.View.view (LoginMsg >> dispatch)
            | Registration -> model |> showView Pages.Registration.View.view (RegistrationMsg >> dispatch)
                
            | AccountActivation _ -> model |> showView Pages.AccountActivation.View.view (AccountActivationMsg >> dispatch)
            | ForgottenPassword -> model |> showView Pages.ForgottenPassword.View.view (ForgottenPasswordMsg >> dispatch)
            | ResetPassword _ -> model |> showView Pages.ResetPassword.View.view (ResetPasswordMsg >> dispatch)
            | Calendar ->
                Html.div [
                    Html.text (sprintf "%A" model.LoggedUser)
                    Html.aRouted "Login" Page.Login
                ]
            
    Router.router [
        Router.pathMode
        Router.onUrlChanged (Page.parseFromUrlSegments >> UrlChanged >> dispatch)
        Router.application render
    ]