module Yobo.Core.Lessons.DbEventHandler

let handle = function
    | LessonCreated args -> UpdateQueries.lessonCreated args
    | ReservationAdded args -> UpdateQueries.reservationAdded args
    | ReservationCancelled args -> UpdateQueries.reservationCancelled args
    | LessonCancelled args -> UpdateQueries.lessonCancelled args
    | CreditsAdded args -> UpdateQueries.creditsAdded args
    | CreditsWithdrawn args -> UpdateQueries.creditsWithdrawn args
    | CreditsRefunded args -> UpdateQueries.creditsRefunded args
    | CashReservationsBlocked args -> UpdateQueries.cashReservationBlocked args
    | CashReservationsUnblocked args -> UpdateQueries.cashReservationUnblocked args
    | WorkshopCreated args -> UpdateQueries.workshopCreated args
    | WorkshopDeleted args -> UpdateQueries.workshopDeleted args
    | ExpirationExtended args -> UpdateQueries.expirationExtended args
    | LessonDeleted args -> UpdateQueries.lessonDeleted args
    | LessonUpdated args -> UpdateQueries.lessonUpdated args