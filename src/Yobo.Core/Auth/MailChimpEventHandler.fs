module Yobo.Core.Auth.MailChimpEventHandler

open FSharp.Control.Tasks

let private subscribe email (mailChimpManager:MailChimp.Net.MailChimpManager) = 
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
    |> Async.AwaitTask
    |> Async.RunSynchronously

let handle getById = function
    | SubscribedToNewsletters args -> 
        args.Id
        |> getById
        |> Option.map (fun (user:Yobo.Shared.Domain.User) -> subscribe user.Email)
        |> Option.defaultValue ignore
    | _ -> ignore
        
        