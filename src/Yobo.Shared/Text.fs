module Yobo.Shared.Text

type TextValue =
    | Id
    | FirstName
    | LastName
    | Password
    | SecondPassword
    | Email
    | Registration
    | Register
    | Login
    | BackToLogin
    | ResendActivationLink
    | ActivationDate
    | Innactive
    | AddCredits
    | User
    | CreditsCount
    | ExpirationDate

type TextMessageValue =
    | ErrorOccured
    | ActivatingYourAccount
    | AccountSuccessfullyActivated
    | RegistrationSuccessful
    | AccountNotActivatedYet
    | ActivationLinkSuccessfullyResent
    | ActivationLinkResendError
    | CreditsSuccessfullyAdded