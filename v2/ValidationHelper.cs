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

            // Username validation: 3-50 chars, alphanumeric + dot/underscore/hyphen
            // Regex: ^[a-zA-Z0-9]([a-zA-Z0-9._-]{1,48}[a-zA-Z0-9])?$
            if (dto.Username.Length < 3 || dto.Username.Length > 50)
                return (false, "Username must be between 3 and 50 characters");

            if (!Regex.IsMatch(dto.Username, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]{1,48}[a-zA-Z0-9])?$"))
                return (false, "Username can only contain letters, numbers, dots, underscores, and hyphens. Cannot start or end with special characters");

            // Name validation: 1-100 chars, letters/spaces/hyphens/apostrophes
            // Regex: ^[a-zA-Z\s\-']{1,100}$
            if (dto.Name.Length < 1 || dto.Name.Length > 100)
                return (false, "Name must be between 1 and 100 characters");

            if (!Regex.IsMatch(dto.Name, @"^[a-zA-Z\s\-']{1,100}$"))
                return (false, "Name can only contain letters, spaces, hyphens, and apostrophes");

            // Email validation: basic format check
            // Regex: ^[^\s@]+@[^\s@]+\.[^\s@]+$
            if (!Regex.IsMatch(dto.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                return (false, "Email must be in valid format (example@domain.com)");

            if (dto.Email.Length > 255)
                return (false, "Email must not exceed 255 characters");

            // Phone validation (optional): if provided, must be valid
            // Regex: ^\+?[0-9\s\-()]{7,20}$
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!Regex.IsMatch(dto.Phone, @"^\+?[0-9\s\-()]{7,20}$"))
                    return (false, "Phone must be in valid format with 7-20 digits/symbols");
            }

            // Password validation is skipped here - EF Core Identity handles it
            // Password will be validated by UserManager.ResetPasswordAsync()

            return (true, null);
        }


        // ==================== VEHICLE DTO VALIDATION ====================

        /// <summary>
        /// Validates CreateVehicleDto
        /// 
        /// Validation Rules:
        /// - LicensePlate: 3-10 chars, alphanumeric (with optional hyphens for formatting)
        ///   Regex (if needed): ^[A-Z0-9]{3,10}$ (without hyphens)
        ///   Or: ^[A-Z0-9]{2,3}-[A-Z0-9]{2,3}$ (with hyphens like NL-AA-12)
        ///   Matches: ABC123, ABC-123, AAAA11 (after hyphen removal)
        ///   Rejects: AB, ABCDEFGHIJK, ab123 (lowercase)
        ///
        /// - Make: 1-50 chars, letters/spaces/numbers (car manufacturer)
        ///   Regex: ^[a-zA-Z0-9\s\-&]+$
        ///   Matches: Toyota, BMW, Range Rover, Rolls-Royce, Porsche & Company
        ///   Rejects: Toyota!, BMW@, $Audi
        ///
        /// - Model: 1-100 chars, letters/spaces/numbers/hyphens
        ///   Regex: ^[a-zA-Z0-9\s\-&()]+$
        ///   Matches: Model S, X5, C-Class, M440i xDrive
        ///   Rejects: Model@, Series!, invalid#model
        ///
        /// - Color: 1-30 chars, letters/spaces only
        ///   Regex: ^[a-zA-Z\s]+$
        ///   Matches: Black, Silver, Dark Blue, Metallic Red
        ///   Rejects: Black123, Blue!, color123
        ///
        /// - Year: Between 1900 and current year
        ///   No regex needed - numeric range check
        ///   Valid: 1985, 2023, 2024
        ///   Invalid: 1899, 2025, 0, -2020
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidateCreateVehicleDto(CreateVehicleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LicensePlate))
                return (false, "License plate is required");

            // License plate validation: 3-10 chars, alphanumeric (hyphens removed by controller)
            // Regex: ^[A-Z0-9]{3,10}$
            string licensePlateClean = dto.LicensePlate.Replace("-", "").ToUpperInvariant();
            if (licensePlateClean.Length < 3 || licensePlateClean.Length > 10)
                return (false, "License plate must be between 3 and 10 characters");

            if (!Regex.IsMatch(licensePlateClean, @"^[A-Z0-9]{3,10}$"))
                return (false, "License plate can only contain letters and numbers");

            // Make validation: 1-50 chars, alphanumeric/spaces/hyphens/ampersand
            // Regex: ^[a-zA-Z0-9\s\-&]+$
            if (!string.IsNullOrWhiteSpace(dto.Make))
            {
                if (dto.Make.Length > 50)
                    return (false, "Make must not exceed 50 characters");

                if (!Regex.IsMatch(dto.Make, @"^[a-zA-Z0-9\s\-&]+$"))
                    return (false, "Make can only contain letters, numbers, spaces, hyphens, and ampersands");
            }

            // Model validation: 1-100 chars, alphanumeric/spaces/hyphens/ampersand/parentheses
            // Regex: ^[a-zA-Z0-9\s\-&()]+$
            if (!string.IsNullOrWhiteSpace(dto.Model))
            {
                if (dto.Model.Length > 100)
                    return (false, "Model must not exceed 100 characters");

                if (!Regex.IsMatch(dto.Model, @"^[a-zA-Z0-9\s\-&()]+$"))
                    return (false, "Model can only contain letters, numbers, spaces, hyphens, ampersands, and parentheses");
            }

            // Color validation: 1-30 chars, letters and spaces only
            // Regex: ^[a-zA-Z\s]+$
            if (!string.IsNullOrWhiteSpace(dto.Color))
            {
                if (dto.Color.Length > 30)
                    return (false, "Color must not exceed 30 characters");

                if (!Regex.IsMatch(dto.Color, @"^[a-zA-Z\s]+$"))
                    return (false, "Color can only contain letters and spaces");
            }

            // Year validation: must be between 1900 and current year
            int currentYear = DateTime.UtcNow.Year;
            if (dto.Year != 0 && (dto.Year < 1900 || dto.Year > currentYear))
                return (false, $"Year must be between 1900 and {currentYear}");

            return (true, null);
        }


        // ==================== DISCOUNT DTO VALIDATION ====================

        /// <summary>
        /// Validates DiscountCreateRequest
        /// 
        /// Validation Rules:
        /// - Code: 3-20 chars, alphanumeric + hyphens/underscores, uppercase
        ///   Regex: ^[A-Z0-9\-_]{3,20}$
        ///   Matches: SUMMER2024, HOLIDAY-50, WELCOME_10, BLACK_FRIDAY
        ///   Rejects: abc, TOOLONGCODENAMEFORTHIS, CODE@2024, code123 (lowercase)
        ///
        /// - Percentage: 0-100, decimal allowed (0.01 to 100)
        ///   No regex - numeric validation
        ///   Valid: 10, 50.5, 99.99
        ///   Invalid: -10, 150, 101.5
        ///
        /// - FixedAmount: positive decimal (if Percentage not set)
        ///   No regex - numeric validation
        ///   Valid: 5, 10.50, 99.99
        ///   Invalid: -10, 0, null (at least one of Percentage or FixedAmount required)
        ///
        /// - StartDate/ExpiryDate: ExpiryDate must be >= StartDate, both optional
        ///   No regex - date comparison
        ///   Valid: StartDate=2024-01-01, ExpiryDate=2024-12-31
        ///   Invalid: StartDate=2024-12-31, ExpiryDate=2024-01-01
        ///
        /// - AllowedLocation: Optional location code/name
        ///   Regex: ^[a-zA-Z0-9\s\-]{2,100}$
        ///   Matches: Amsterdam, Parking Lot A, Lot-1, P1
        ///   Rejects: A, Amsterdam@, Lot!!, invalid#location
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidateDiscountCreateRequest(DiscountCreateRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return (false, "Discount code is required");

            // Code validation: 3-20 chars, alphanumeric + hyphens/underscores, uppercase
            // Regex: ^[A-Z0-9\-_]{3,20}$
            if (dto.Code.Length < 3 || dto.Code.Length > 20)
                return (false, "Discount code must be between 3 and 20 characters");

            if (!Regex.IsMatch(dto.Code, @"^[A-Z0-9\-_]{3,20}$"))
                return (false, "Discount code must contain only uppercase letters, numbers, hyphens, and underscores");

            // Either Percentage or FixedAmount must be set
            if (dto.Percentage <= 0 && (dto.FixedAmount == null || dto.FixedAmount <= 0))
                return (false, "Either percentage or fixed amount must be greater than 0");

            // Percentage validation: 0-100
            if (dto.Percentage > 0 && dto.Percentage > 100)
                return (false, "Percentage must not exceed 100");

            // FixedAmount validation: positive number
            if (dto.FixedAmount.HasValue && dto.FixedAmount <= 0)
                return (false, "Fixed amount must be greater than 0");

            // Date validation: ExpiryDate must be >= StartDate
            if (dto.StartDate.HasValue && dto.ExpiryDate.HasValue)
            {
                if (dto.ExpiryDate < dto.StartDate)
                    return (false, "Expiry date must be equal to or after start date");
            }

            // MaxUsage validation: must be positive if set
            if (dto.MaxUsage.HasValue && dto.MaxUsage <= 0)
                return (false, "Maximum usage must be greater than 0");

            // AllowedLocation validation (optional): 2-100 chars, alphanumeric/spaces/hyphens
            // Regex: ^[a-zA-Z0-9\s\-]{2,100}$
            if (!string.IsNullOrWhiteSpace(dto.AllowedLocation))
            {
                if (dto.AllowedLocation.Length < 2 || dto.AllowedLocation.Length > 100)
                    return (false, "Allowed location must be between 2 and 100 characters");

                if (!Regex.IsMatch(dto.AllowedLocation, @"^[a-zA-Z0-9\s\-]{2,100}$"))
                    return (false, "Allowed location can only contain letters, numbers, spaces, and hyphens");
            }

            return (true, null);
        }


        // ==================== PAYMENT DTO VALIDATION ====================

        /// <summary>
        /// Validates CreatePaymentRequestDTO
        /// 
        /// Validation Rules:
        /// - Amount: Decimal, must be > 0 and <= 999,999.99
        ///   No regex - numeric validation
        ///   Valid: 10.50, 100, 999999.99
        ///   Invalid: 0, -10, 1000000
        ///
        /// - TransactionMethod: Optional, if provided must be valid payment method
        ///   Regex: ^[a-zA-Z\s]+$ (letters and spaces only)
        ///   Matches: Credit Card, Bank Transfer, PayPal, Apple Pay
        ///   Rejects: Credit-Card, PayPal123, Method@
        ///
        /// - TransactionIssuer: Optional, 1-100 chars, alphanumeric/spaces
        ///   Regex: ^[a-zA-Z0-9\s\-&]+$
        ///   Matches: Visa, Mastercard, ABN AMRO, ING Bank
        ///   Rejects: Visa!, Bank@, issuer123!
        ///
        /// - TransactionBank: Optional, 1-100 chars, alphanumeric/spaces
        ///   Regex: ^[a-zA-Z0-9\s\-&]+$
        ///   Matches: ING, Rabobank, Deutsche Bank
        ///   Rejects: Bank!, issuer#
        ///
        /// - DiscountCode: Optional, same format as discount code validation
        ///   Regex: ^[A-Z0-9\-_]{3,20}$
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidateCreatePaymentRequestDTO(CreatePaymentRequestDTO dto)
        {
            // Amount validation: must be > 0 and <= 999,999.99
            if (dto.Amount <= 0)
                return (false, "Amount must be greater than 0");

            if (dto.Amount > 999999.99m)
                return (false, "Amount must not exceed 999,999.99");

            // TransactionMethod validation (optional): letters and spaces only
            // Regex: ^[a-zA-Z\s]+$
            if (!string.IsNullOrWhiteSpace(dto.TransactionMethod))
            {
                if (dto.TransactionMethod.Length > 50)
                    return (false, "Transaction method must not exceed 50 characters");

                if (!Regex.IsMatch(dto.TransactionMethod, @"^[a-zA-Z\s]+$"))
                    return (false, "Transaction method can only contain letters and spaces");
            }

            // TransactionIssuer validation (optional): alphanumeric/spaces/hyphens/ampersand
            // Regex: ^[a-zA-Z0-9\s\-&]{1,100}$
            if (!string.IsNullOrWhiteSpace(dto.TransactionIssuer))
            {
                if (dto.TransactionIssuer.Length > 100)
                    return (false, "Transaction issuer must not exceed 100 characters");

                if (!Regex.IsMatch(dto.TransactionIssuer, @"^[a-zA-Z0-9\s\-&]{1,100}$"))
                    return (false, "Transaction issuer can only contain letters, numbers, spaces, hyphens, and ampersands");
            }

            // TransactionBank validation (optional): same as issuer
            // Regex: ^[a-zA-Z0-9\s\-&]{1,100}$
            if (!string.IsNullOrWhiteSpace(dto.TransactionBank))
            {
                if (dto.TransactionBank.Length > 100)
                    return (false, "Transaction bank must not exceed 100 characters");

                if (!Regex.IsMatch(dto.TransactionBank, @"^[a-zA-Z0-9\s\-&]{1,100}$"))
                    return (false, "Transaction bank can only contain letters, numbers, spaces, hyphens, and ampersands");
            }

            // DiscountCode validation (optional): same as discount code
            // Regex: ^[A-Z0-9\-_]{3,20}$
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