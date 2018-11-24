module Yobo.Core.Users.DbEventHandler

let handle = function
    | Registered args -> UpdateQueries.register args