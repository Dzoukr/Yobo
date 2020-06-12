module Yobo.Client.SharedView

open Feliz
open Feliz.Bulma
open Yobo.Shared.Errors
open Yobo.Shared.Validation
open Yobo.Client.Router
open Feliz.Router

module Queries =
    open Yobo.Shared.Core.Domain
    let paymentToText = function
        | LessonPayment.Credits -> "Kredity"
        | LessonPayment.Cash -> "Hotově"

module Html =
    module Props =
        let routed (p:Page) =
            [
                prop.href (p |> Page.toUrlSegments |> Router.formatPath)
                prop.onClick (Router.goToUrl)
            ]
        
    let aRouted (text:string) (p:Page) =
        Html.a [
            yield! Props.routed p
            prop.text text
        ]
        
    let faIcon (icon:string) = Html.i [ prop.className icon; prop.style [ style.marginRight 5 ] ]        
    let faIconSingle (icon:string) = Html.i [ prop.className icon ]        

module ServerResponseViews =
    open Elmish
    open Elmish.Toastr
    
    let showErrorToast (e:ServerError) : Cmd<_> =
        let basicToaster =
            match e with
            | Validation v ->
                v
                |> List.map (fun x -> x.Field, ValidationErrorType.explain x.Type)
                |> List.map (fun (n,e) -> sprintf "%s : %s" n e)
                |> String.concat "<br/>"
                |> Toastr.message
                |> Toastr.title "Data nejsou vyplněna správně"
                |> Toastr.timeout 30000
                |> Toastr.extendedTimout 10000
            | Authentication e ->
                e
                |> AuthenticationError.explain
                |> Toastr.message
                |> Toastr.timeout 5000
                |> Toastr.extendedTimout 2000
            | error ->
                error
                |> ServerError.explain
                |> Toastr.message
                |> Toastr.title "Došlo k chybě"
                
        basicToaster
        |> Toastr.position ToastPosition.TopRight
        |> Toastr.hideEasing Easing.Swing
        |> Toastr.withProgressBar
        |> Toastr.showCloseButton
        |> Toastr.error
    
    let showSuccessToast msg : Cmd<_> =
        Toastr.message msg
        |> Toastr.position ToastPosition.TopRight
        |> Toastr.success
    
module BoxedViews =
    
    let showSuccess elm =
        Bulma.message [
            color.isSuccess
            prop.children [
                Bulma.messageBody [
                    Html.i [ prop.className "fas fa-check-circle"; prop.style [ style.paddingRight 10 ] ]
                    elm
                ]
            ]
        ]
    
    let showError elm =
        Bulma.message [
            color.isDanger
            prop.children [
                Bulma.messageBody [
                    Html.i [ prop.className "fas fa-exclamation-circle"; prop.style [ style.paddingRight 10 ] ]
                    elm
                ]
            ]
        ]
    
    let showInProgress elm =
        Bulma.message [
            color.isInfo
            prop.children [
                Bulma.messageBody [
                    Html.i [ prop.className "fas fa-circle-notch fa-spin" ]
                    Html.span [
                        prop.style [ style.paddingLeft 10 ]
                        prop.children [elm]
                    ]
                ]
            ]
        ]
            
    let showSuccessMessage (msg:string) = msg |> Html.text |> showSuccess
    let showErrorMessage (msg:string) = msg |> Html.text |> showError
    let showInProgressMessage (msg:string) = msg |> Html.text |> showInProgress

module ValidationViews =

    let help errors name =
        errors
        |> List.tryFind (fun x -> x.Field = name)
        |> Option.map (fun x ->
            Bulma.help [
                color.isDanger
                prop.text (x.Type |> ValidationErrorType.explain)
            ]
        )
        |> Option.defaultValue Html.none

    let color errors name =
        errors
        |> List.tryFind (fun x -> x.Field = name)
        |> Option.map (fun _ -> color.isDanger)
        |> Option.defaultValue (Interop.mkAttr "dummy" "")


        
module StaticTextViews =
    
    let showTermsModal isActive closeDisplay =
        Bulma.modal [
            if isActive then modal.isActive
            prop.children [
                Bulma.modalBackground [ prop.onClick closeDisplay ]
                Bulma.modalCard [
                    Bulma.modalCardHead [
                        Bulma.modalCardTitle "Obchodní podmínky"
                        Bulma.delete [ prop.onClick closeDisplay ]
                    ]
                    Bulma.modalCardBody [
                        prop.dangerouslySetInnerHTML StaticText.terms
                    ]
                ]
            ]
        ]