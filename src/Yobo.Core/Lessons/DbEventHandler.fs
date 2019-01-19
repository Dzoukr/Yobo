module Yobo.Core.Lessons.DbEventHandler

let handle = function
    | Created args -> UpdateQueries.created args
    | ReservationAdded args -> UpdateQueries.reservationAdded args
    | ReservationCancelled args -> UpdateQueries.reservationCancelled args
