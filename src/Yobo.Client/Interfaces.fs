module Yobo.Client.Interfaces

[<Interface>]
type IUserAwareModel =
    abstract UpdateUser: Yobo.Shared.UserAccount.Domain.Queries.UserAccount -> IUserAwareModel