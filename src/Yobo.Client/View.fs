module Yobo.Client.View

open Domain
open Elmish
open Feliz
open Feliz.Bulma
open Feliz.Bulma.PageLoader
open Feliz.Router

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
            match model.CurrentPage with
            | Login m -> Pages.Login.View.view m (LoginMsg >> dispatch)
            | Registration m -> Pages.Registration.View.view m (RegistrationMsg >> dispatch)
                
            | AccountActivation m -> Pages.AccountActivation.View.view m (AccountActivationMsg >> dispatch)
            | ForgottenPassword m -> Pages.ForgottenPassword.View.view m (ForgottenPasswordMsg >> dispatch)
            | ResetPassword m -> Pages.ResetPassword.View.view m (ResetPasswordMsg >> dispatch)
            | Calendar ->
                Html.div [
                    Html.text (sprintf "%A" model.LoggedUser)
                    Html.a [
                        prop.text "Login"
                        prop.href (Router.formatPath Paths.Login)
                        prop.onClick Router.goToUrl
                    ]
                ]
            
    Router.router [
        Router.pathMode
        Router.onUrlChanged (Page.parseFromUrlSegments >> UrlChanged >> dispatch)
        Router.application render
    ]