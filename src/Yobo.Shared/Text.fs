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

type TextMessageValue =
    | ActivatingYourAccount
    | AccountSuccessfullyActivated
    | RegistrationSuccessful
    | AccountNotActivatedYet
    | ActivationLinkSuccessfullyResent
    | ActivationLinkResendError