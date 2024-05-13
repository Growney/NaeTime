using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NaeTime.Client.Razor.Lib.Validation;
public class IpAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string? valueString = value as string;

        if (string.IsNullOrEmpty(valueString))
        {
            return new ValidationResult("IP address  is null");
        }

        const string regexPattern = @"^([\d]{1,3}\.){3}[\d]{1,3}$";
        Regex regex = new(regexPattern);
        if (!regex.IsMatch(valueString) || valueString.Split('.').SingleOrDefault(s => int.Parse(s) > 255) != null)
        {
            return new ValidationResult("Invalid IP Address");
        }

        return ValidationResult.Success;
    }
}
