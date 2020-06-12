module Yobo.Client.View

open Yobo.Client.Router
open Domain
open Elmish
open Feliz
open Feliz
open Feliz.Bulma
open Feliz.Bulma.Operators
open Feliz.Bulma.PageLoader
open Feliz.Router
open Yobo.Client.SharedView

let private displayLoggedPage (user:Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount) (page:SecuredPage) showTerms dispatch (content:ReactElement)  =
    
    let item (pg:SecuredPage) (icon:string) (text:string) =
        let isActive = page = pg
        Bulma.navbarItem.a [
            if isActive then navbarItem.isActive
            yield! Html.Props.routed (Page.Secured pg)
            prop.children [
                Html.faIcon icon
                Html.text text
            ]
        ]
    
    let adminButtons =
        if user.IsAdmin then
            [ item Users "fas fa-users" "Uživatelé"
              item Lessons "fas fa-calendar-alt" "Lekce" ]
        else []
    
    let userInfo =
        Bulma.navbarItem.div [
            Bulma.tag [
                color.isInfo
                prop.style [ style.marginRight 10 ]
                prop.text (sprintf "%i kreditů" user.Credits)
            ]
            Html.faIcon "fas fa-user"
            Html.text (sprintf "%s %s" user.FirstName user.LastName)
        ]
    
    Html.div [
        Bulma.navbar [
            color.isLight
            prop.children [
                Bulma.container [
                    Bulma.navbarStart.div [
                        item Calendar "fas fa-calendar-alt" "Kalendář"
                        item MyAccount "fas fa-user" "Můj účet"
                    ]
                    Bulma.navbarEnd.div [
                        yield! adminButtons
                        userInfo
                        Bulma.navbarItem.div [
                            Bulma.buttons [
                                Bulma.button.a [
                                    color.isDanger
                                    prop.onClick (fun _ -> LoggedOut |> dispatch)
                                    prop.text "Odhlásit"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
        Html.main [
            prop.style [ style.paddingTop(length.rem 2) ]
            prop.children [
                Bulma.container [
                    content
                ]
                Bulma.container [
                    Html.a [
                        prop.className "terms-link"
                        prop.text "Obchodní podmínky"
                        prop.onClick (fun _ -> ShowTerms(true) |> dispatch)
                    ]
                    SharedView.StaticTextViews.showTermsModal showTerms (fun _ -> ShowTerms(false) |> dispatch)
                ]
            ]
        ]
    ]
    
let showView<'model,'msg> (fn:'model -> ('msg -> unit) -> Fable.React.ReactElement) (dispatch:'msg -> unit) (m:Model) =
    let pm = m |> Model.getPageModel<'model>
    fn pm dispatch

let notActivatedView (user:Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount) dispatch =
    let inMain (content:ReactElement) =
        Html.main [
            prop.style [ style.paddingTop(length.rem 2) ]
            prop.children [
                Bulma.container [
                    content
                ]
            ]
        ]
    
    Html.div [
        Html.p "Váš účet ještě není aktivován."
        Html.p "Podívejte se do své emailové schránky (zkontrolujte SPAM složku), kde byste měli najít odkaz na aktivaci."
        Html.p "Pokud nemůžete kód najít, klikněte na tlačítko níže a my vám pošleme nový."
        Html.div [
            prop.style [ style.paddingTop(length.rem 2) ]
            prop.children [
                Bulma.button.button [
                    color.isPrimary
                    prop.text "Poslat nový kód na email"
                    prop.onClick (fun _ -> user.Id |> ResendActivation |> dispatch)
                ]
            ]
        ]
    ]
    |> BoxedViews.showError
    |> inMain
    
let view (model:Model) (dispatch:Msg -> unit) =
    let render =
        if model.IsCheckingUser then
            PageLoader.pageLoader [
                pageLoader.isWhite
                pageLoader.isActive
                prop.children [
                    PageLoader.title "Ověřuji přihlášení"
                ]
            ]
        else            
            match model.CurrentPage with
            | Anonymous pg ->
                match pg with
                | Login -> model |> showView Pages.Login.View.view (LoginMsg >> dispatch)
                | Registration -> model |> showView Pages.Registration.View.view (RegistrationMsg >> dispatch)
                | AccountActivation _ -> model |> showView Pages.AccountActivation.View.view (AccountActivationMsg >> dispatch)
                | ForgottenPassword -> model |> showView Pages.ForgottenPassword.View.view (ForgottenPasswordMsg >> dispatch)
                | ResetPassword _ -> model |> showView Pages.ResetPassword.View.view (ResetPasswordMsg >> dispatch)
            | Secured (pg, user) ->
                if not user.IsActivated then notActivatedView user dispatch
                else
                    match pg with
                    | Calendar -> model |> showView Pages.Calendar.View.view (CalendarMsg >> dispatch)
                    | Users -> model |> showView Pages.Users.View.view (UsersMsg >> dispatch)
                    | Lessons -> model |> showView Pages.Lessons.View.view (LessonsMsg >> dispatch)
                    | MyAccount -> model |> showView Pages.MyAccount.View.view (MyAccountMsg >> dispatch)
                        
                    |> displayLoggedPage user pg model.ShowTerms dispatch
            
    Router.router [
        Router.pathMode
        Router.onUrlChanged (Page.parseFromUrlSegments >> UrlChanged >> dispatch)
        Router.application render
    ]