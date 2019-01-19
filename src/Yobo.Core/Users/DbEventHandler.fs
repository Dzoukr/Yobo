module Yobo.Core.Users.DbEventHandler

let handle = function
    | Registered args -> UpdateQueries.registered args
    | Activated args -> UpdateQueries.activated args
    | ActivationKeyRegenerated args -> UpdateQueries.activationKeyRegenerated args
    | CreditsAdded args -> UpdateQueries.creditsAdded args
    | CreditsWithdrawn args -> UpdateQueries.creditsWithdrawn args
    | CreditsRefunded args -> UpdateQueries.creditsRefunded args
    | CashReservationsBlocked args -> UpdateQueries.cashReservationBlocked args
    | CashReservationsUnblocked args -> UpdateQueries.cashReservationUnblocked args