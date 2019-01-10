module Yobo.Core.Users.DbEventHandler

let handle = function
    | Registered args -> UpdateQueries.registered args
    | Activated args -> UpdateQueries.activated args
    | ActivationKeyRegenerated args -> UpdateQueries.activationKeyRegenerated args
    | CreditsAdded args -> UpdateQueries.creditsAdded args