module Yobo.Client.Interfaces

[<Interface>]
type IUserAwareModel =
    abstract UpdateUser: Yobo.Shared.Core.UserAccount.Domain.Queries.UserAccount -> IUserAwareModel