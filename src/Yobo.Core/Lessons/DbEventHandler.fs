module Yobo.Core.Lessons.DbEventHandler

let handle = function
    | Created args -> UpdateQueries.created args
    | ReservationAdded args -> UpdateQueries.reservationAdded args
    | ReservationCancelled args -> UpdateQueries.reservationCancelled args
    | Cancelled args -> UpdateQueries.cancelled args
    | CreditsWithdrawn args -> UpdateQueries.creditsWithdrawn args
    | CreditsRefunded args -> UpdateQueries.creditsRefunded args
    | CashReservationsBlocked args -> UpdateQueries.cashReservationBlocked args
    | CashReservationsUnblocked args -> UpdateQueries.cashReservationUnblocked args