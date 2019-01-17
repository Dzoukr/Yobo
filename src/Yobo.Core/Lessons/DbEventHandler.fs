module Yobo.Core.Lessons.DbEventHandler

let handle = function
    | Created args -> UpdateQueries.created args
