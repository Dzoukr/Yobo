module Yobo.Server.EmailTemplates

open System
open Yobo.Shared

let private getRegister activateLink =
    """
    <h2>Vítejte v rezervačním systému</h2>
    <p>
        Pro aktivaci účtu prosím klikněte na následující odkaz: <a href="{{{activate}}}">{{{activate}}}</a>
    </p>
    <p>
        Přejeme Vám krásný den.
    </p>
    <hr style="margin-top: 40px;" />
    <p>
        <i>Tato zpráva byla automaticky vygenerována rezervačním systémem Mindful Yoga.</i>
    </p>
    """ |> (fun x -> x.Replace("{{{activate}}}", activateLink))
    
let private getPasswordResetInit resetLink =
    """
    <h2>Dobrý den!</h2>
    <p>
        Někdo (zřejmě Vy) si vyžádal reset hesla do rezervačního systému Mindful Yoga. Pokud si chcete heslo změnit,
        klikněte prosím na následující odkaz <a href="{{{reset}}}">{{{reset}}}</a>.
    </p>
    <p>
        Přejeme Vám krásný den.
    </p>
    <hr style="margin-top: 40px;" />
    <p>
        <i>Tato zpráva byla automaticky vygenerována rezervačním systémem Mindful Yoga.</i>
    </p>
    """ |> (fun x -> x.Replace("{{{reset}}}", resetLink))

type EmailTemplateBuilder = {
    RegisterEmailMessage : Guid -> string
    PasswordResetEmailMessage : Guid -> string
}

let getDefault baseUri = {
    RegisterEmailMessage =
        string
        >> sprintf "/%s/%s" ClientPaths.AccountActivation
        >> fun link -> Uri(baseUri, link)
        >> string
        >> getRegister
    PasswordResetEmailMessage =
        string
        >> sprintf "/%s/%s" ClientPaths.ResetPassword
        >> fun link -> Uri(baseUri, link)
        >> string
        >> getPasswordResetInit
}