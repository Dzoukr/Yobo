module Yobo.Core.Auth.DbEventHandler

let handle = function
    | Registered args -> UpdateQueries.registered args
    | Activated args -> UpdateQueries.activated args
    | ActivationKeyRegenerated args -> UpdateQueries.activationKeyRegenerated args
    | PasswordResetInitiated args -> UpdateQueries.passwordResetInitiated args
    | PasswordReset args -> UpdateQueries.passwordReset args
    
    