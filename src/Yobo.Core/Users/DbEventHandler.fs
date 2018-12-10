module Yobo.Core.Users.DbEventHandler

let handle = function
    | Registered args -> UpdateQueries.register args
    | Activated args -> UpdateQueries.activate args
    | ActivationKeyRegenerated args -> UpdateQueries.regenerateActivationKey args