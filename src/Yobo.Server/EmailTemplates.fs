module Yobo.Server.EmailTemplates

open System

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
    

type EmailTemplateBuilder = {
    RegisterEmailMessage : Guid -> string
}

let getDefault baseUri = {
    RegisterEmailMessage =
        sprintf "/accountActivation/%O"
        >> fun link -> Uri(baseUri, link)
        >> string
        >> getRegister
}