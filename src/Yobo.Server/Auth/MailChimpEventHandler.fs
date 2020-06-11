module Yobo.Server.Auth.MailChimpEventHandler

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open Domain

let private subscribe (mailChimpManager:MailChimp.Net.MailChimpManager) email  = 
    task {
        try
            let! lists = mailChimpManager.Lists.GetAllAsync()
            let list = lists |> Seq.head
            let contact = MailChimp.Net.Models.Member(EmailAddress = email, StatusIfNew = MailChimp.Net.Models.Status.Subscribed)
            let! _ = mailChimpManager.Members.AddOrUpdateAsync(list.Id, contact)
            return ()
        with ex ->
            return ()
    } 

let handle mailchimp (tryGetUserById:Guid -> Task<Domain.Queries.BasicUserView option>) evn =
    task {
        match evn with
        | SubscribedToNewsletters args -> 
            match! args.Id |> tryGetUserById with
            | Some (user:Domain.Queries.BasicUserView) -> do! subscribe mailchimp user.Email 
            | None -> return ()
        | _ -> return ()
    }
