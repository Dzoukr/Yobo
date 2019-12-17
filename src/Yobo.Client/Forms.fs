module Yobo.Client.Forms

open Yobo.Shared.Validation

type ValidatedForm<'a> = {
    ValidationErrors : ValidationError list
    FormData : 'a
}

module ValidatedForm =
    let updateWith f form = { form with FormData = f }
    let updateWithErrors errs form = { form with ValidationErrors = errs }
    let validateWith validFn form = { form with ValidationErrors = validFn form.FormData }
    let validateConditionalWith predicate validFn form = if predicate then validateWith validFn form else form
    let isValid form = form.ValidationErrors.IsEmpty
    let init v = { FormData = v; ValidationErrors = [] }
