module Yobo.Core.Workshops.DbEventHandler

let handle = function
    | Created args -> UpdateQueries.created args
    | Deleted args -> UpdateQueries.deleted args
