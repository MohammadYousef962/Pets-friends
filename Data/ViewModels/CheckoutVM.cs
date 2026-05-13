using Pets_friends.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class CheckoutVM
    {
        // --- Autofilled Profile Fields ---
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street Address is required.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^(07[789]\d{7}|\+9627[789]\d{7})$", ErrorMessage = "Must be a valid Jordanian mobile number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        // --- Active Financial Metrics ---
        public List<ShoppingCart> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }

        // --- Active Payment Routing ---
        public string PaymentMethod { get; set; } = "card"; // card, cliq, paypal, cod

        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvv { get; set; }
        public string? NameOnCard { get; set; }

        public string? CliqAlias { get; set; }
    }
}