using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using v2.Core.DTOs;

namespace v2.Core.Validators
{
    
    public static class ValidationHelper
    {
        
        public static (bool isValid, string? errorMessage) ValidateProfileDto(ProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return (false, "Username is required");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "Email is required");


            if (dto.Username.Length < 3 || dto.Username.Length > 50)
                return (false, "Username must be between 3 and 50 characters");

            if (!Regex.IsMatch(dto.Username, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]{1,48}[a-zA-Z0-9])?$"))
                return (false, "Username can only contain letters, numbers, dots, underscores, and hyphens. Cannot start or end with special characters");

            if (dto.Name.Length < 1 || dto.Name.Length > 100)
                return (false, "Name must be between 1 and 100 characters");

            if (!Regex.IsMatch(dto.Name, @"^[a-zA-Z\s\-']{1,100}$"))
                return (false, "Name can only contain letters, spaces, hyphens, and apostrophes");

            if (!Regex.IsMatch(dto.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                return (false, "Email must be in valid format (example@domain.com)");

            if (dto.Email.Length > 255)
                return (false, "Email must not exceed 255 characters");


            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!Regex.IsMatch(dto.Phone, @"^\+?[0-9\s\-()]{7,20}$"))
                    return (false, "Phone must be in valid format with 7-20 digits/symbols");
            }


            return (true, null);
        }

        public static (bool isValid, string? errorMessage) ValidateCreateVehicleDto(CreateVehicleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LicensePlate))
                return (false, "License plate is required");


            string licensePlateClean = dto.LicensePlate.Replace("-", "").ToUpperInvariant();
            if (licensePlateClean.Length < 3 || licensePlateClean.Length > 10)
                return (false, "License plate must be between 3 and 10 characters");

            if (!Regex.IsMatch(licensePlateClean, @"^[A-Z0-9]{3,10}$"))
                return (false, "License plate can only contain letters and numbers");

            if (!string.IsNullOrWhiteSpace(dto.Make))
            {
                if (dto.Make.Length > 50)
                    return (false, "Make must not exceed 50 characters");

                if (!Regex.IsMatch(dto.Make, @"^[a-zA-Z0-9\s\-&]+$"))
                    return (false, "Make can only contain letters, numbers, spaces, hyphens, and ampersands");
            }


            if (!string.IsNullOrWhiteSpace(dto.Model))
            {
                if (dto.Model.Length > 100)
                    return (false, "Model must not exceed 100 characters");

                if (!Regex.IsMatch(dto.Model, @"^[a-zA-Z0-9\s\-&()]+$"))
                    return (false, "Model can only contain letters, numbers, spaces, hyphens, ampersands, and parentheses");
            }

            if (!string.IsNullOrWhiteSpace(dto.Color))
            {
                if (dto.Color.Length > 30)
                    return (false, "Color must not exceed 30 characters");

                if (!Regex.IsMatch(dto.Color, @"^[a-zA-Z\s]+$"))
                    return (false, "Color can only contain letters and spaces");
            }

            int currentYear = DateTime.UtcNow.Year;
            if (dto.Year != 0 && (dto.Year < 1900 || dto.Year > currentYear))
                return (false, $"Year must be between 1900 and {currentYear}");

            return (true, null);
        }

        public static (bool isValid, string? errorMessage) ValidateDiscountCreateRequest(DiscountCreateRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return (false, "Discount code is required");


            if (dto.Code.Length < 3 || dto.Code.Length > 20)
                return (false, "Discount code must be between 3 and 20 characters");

            if (!Regex.IsMatch(dto.Code, @"^[A-Z0-9\-_]{3,20}$"))
                return (false, "Discount code must contain only uppercase letters, numbers, hyphens, and underscores");

            if (dto.Percentage <= 0 && (dto.FixedAmount == null || dto.FixedAmount <= 0))
                return (false, "Either percentage or fixed amount must be greater than 0");

            if (dto.Percentage > 0 && dto.Percentage > 100)
                return (false, "Percentage must not exceed 100");

            if (dto.FixedAmount.HasValue && dto.FixedAmount <= 0)
                return (false, "Fixed amount must be greater than 0");

            if (dto.StartDate.HasValue && dto.ExpiryDate.HasValue)
            {
                if (dto.ExpiryDate < dto.StartDate)
                    return (false, "Expiry date must be equal to or after start date");
            }

            if (dto.MaxUsage.HasValue && dto.MaxUsage <= 0)
                return (false, "Maximum usage must be greater than 0");

            if (!string.IsNullOrWhiteSpace(dto.AllowedLocation))
            {
                if (dto.AllowedLocation.Length < 2 || dto.AllowedLocation.Length > 100)
                    return (false, "Allowed location must be between 2 and 100 characters");

                if (!Regex.IsMatch(dto.AllowedLocation, @"^[a-zA-Z0-9\s\-]{2,100}$"))
                    return (false, "Allowed location can only contain letters, numbers, spaces, and hyphens");
            }

            return (true, null);
        }

        public static (bool isValid, string? errorMessage) ValidateCreatePaymentRequestDTO(CreatePaymentRequestDTO dto)
        {
            if (dto.Amount <= 0)
                return (false, "Amount must be greater than 0");

            if (dto.Amount > 999999.99m)
                return (false, "Amount must not exceed 999,999.99");

            if (!string.IsNullOrWhiteSpace(dto.TransactionMethod))
            {
                if (dto.TransactionMethod.Length > 50)
                    return (false, "Transaction method must not exceed 50 characters");

                if (!Regex.IsMatch(dto.TransactionMethod, @"^[a-zA-Z\s]+$"))
                    return (false, "Transaction method can only contain letters and spaces");
            }

            if (!string.IsNullOrWhiteSpace(dto.TransactionIssuer))
            {
                if (dto.TransactionIssuer.Length > 100)
                    return (false, "Transaction issuer must not exceed 100 characters");

                if (!Regex.IsMatch(dto.TransactionIssuer, @"^[a-zA-Z0-9\s\-&]{1,100}$"))
                    return (false, "Transaction issuer can only contain letters, numbers, spaces, hyphens, and ampersands");
            }

            if (!string.IsNullOrWhiteSpace(dto.TransactionBank))
            {
                if (dto.TransactionBank.Length > 100)
                    return (false, "Transaction bank must not exceed 100 characters");

                if (!Regex.IsMatch(dto.TransactionBank, @"^[a-zA-Z0-9\s\-&]{1,100}$"))
                    return (false, "Transaction bank can only contain letters, numbers, spaces, hyphens, and ampersands");
            }

            if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
            {
                if (dto.DiscountCode.Length < 3 || dto.DiscountCode.Length > 20)
                    return (false, "Discount code must be between 3 and 20 characters");

                if (!Regex.IsMatch(dto.DiscountCode, @"^[A-Z0-9\-_]{3,20}$"))
                    return (false, "Discount code must contain only uppercase letters, numbers, hyphens, and underscores");
            }

            return (true, null);
        }
    }
}